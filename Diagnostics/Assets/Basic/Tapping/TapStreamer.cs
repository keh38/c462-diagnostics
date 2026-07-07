using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using UnityEngine;

// Direct-AudioClient capture that sidesteps two things the stock WasapiCapture does
// that fail on this device under Unity/Mono:
//   1. It marshals the WaveFormat *manually* (StructureToPtr -> IntPtr), the exact
//      path we verified produces correct WAVEFORMATEX bytes, instead of relying on
//      NAudio's COM class-parameter marshalling.
//   2. It initializes in POLLING mode (no EventCallback) with the buffer-alignment
//      retry that exclusive mode requires, instead of the stock hardcoded
//      EventCallback + 100 ms periodicity that the driver rejects.
// Everything else (enumeration, activation, the GetBuffer pump) still uses NAudio.
public class TapStreamer
{
    // Minimal redeclaration of IAudioClient exposing ONLY Initialize, with the
    // format as IntPtr. Same IID as NAudio's IAudioClient; Initialize is the first
    // method (vtable slot 3), so this maps correctly.
    [ComImport, Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioClientInit
    {
        [PreserveSig]
        int Initialize(AudioClientShareMode shareMode, AudioClientStreamFlags streamFlags,
                       long hnsBufferDuration, long hnsPeriodicity, IntPtr pFormat,
                       [In] ref Guid audioSessionGuid);
    }

    private const int E_NOT_ALIGNED = unchecked((int)0x88890009); // AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED
    private const long REFTIMES_PER_SEC = 10000000;               // 100-ns units per second

    private MMDevice _device;
    private AudioClient _client;
    private AudioCaptureClient _captureClient;
    private WaveFileWriter _writer;
    private WaveFormat _format = new WaveFormat(48000, 16, 2);    // proven-good exclusive format
    private int _blockAlign;
    private byte[] _managed = new byte[0];
    private Thread _pump;
    private volatile bool _stop;

    public bool StreamStopped { get; private set; } = false;

    static readonly PropertyKey PKEY_Device_ContainerId =
        new PropertyKey(new Guid("8c7ed206-3f8a-4827-b3ab-ae9e1faefc6c"), 2);

    static readonly PropertyKey PKEY_FormFactor =
        new PropertyKey(new Guid("1da5d803-d492-4edd-8c23-e0c0ffee7f0e"), 0);

    public bool Initialize()
    {
        _device = FindLineInOnDefaultOutput();

        if (_device == null)
        {
            Debug.LogError($"Could not resolve line-in device.");
            return false;
        }

        Debug.Log($"[TapStreamer]: using line-in '{_device.FriendlyName}'");

        return true;
    }

    // Manually marshal the format and Initialize via the IntPtr interface, with the
    // exclusive-mode alignment retry. Returns an initialized AudioClient or null.
    private AudioClient InitializeAligned()
    {
        long duration = REFTIMES_PER_SEC / 10; // start at 100 ms
        Guid session = Guid.Empty;

        for (int attempt = 0; attempt < 2; attempt++)
        {
            var client = _device.AudioClient;  // fresh activation each attempt

            var field = typeof(AudioClient).GetField("audioClientInterface",
                            BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogError("Could not reflect 'audioClientInterface' — field name may differ in this NAudio build.");
                client.Dispose();
                return null;
            }

            var init = (IAudioClientInit)field.GetValue(client);

            IntPtr fmtPtr = Marshal.AllocHGlobal(64);
            Marshal.StructureToPtr(_format, fmtPtr, false);   // verified correct bytes
            int hr = init.Initialize(AudioClientShareMode.Exclusive,
                                     AudioClientStreamFlags.None,   // polling, no EventCallback
                                     duration, duration, fmtPtr, ref session);
            Marshal.FreeHGlobal(fmtPtr);

            if (hr == 0)
            {
                //Debug.Log($"Initialize OK (buffer {client.BufferSize} frames).");
                return client;
            }

            if (hr == E_NOT_ALIGNED)
            {
                int aligned = client.BufferSize;   // valid after NOT_ALIGNED
                duration = (long)(REFTIMES_PER_SEC * aligned / (double)_format.SampleRate + 0.5);
                Debug.Log($"Buffer not aligned; retrying with {aligned} frames ({duration} hns).");
                client.Dispose();
                continue;
            }

            Debug.LogError($"Initialize failed: hr=0x{hr:X8}");
            client.Dispose();
            return null;
        }
        Debug.LogError("Initialize failed after alignment retry.");
        return null;
    }

    public bool StartStreaming(string filePath)
    {
        try
        {
            _client = InitializeAligned();
            if (_client == null) return false;

            _captureClient = _client.AudioCaptureClient;   // GetService works post-Initialize
            _blockAlign = _format.BlockAlign;
            _writer = new WaveFileWriter(filePath, _format);

            _stop = false;
            StreamStopped = false;
            _client.Start();

            _pump = new Thread(PumpLoop) { IsBackground = true };
            _pump.Start();

            Debug.Log("[TapStreamer]: started");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to start streaming: {ex}");
            return false;
        }
    }

    // Runs on a background thread. Touches NO Unity API — disk writer only.
    private void PumpLoop()
    {
        try
        {
            while (!_stop)
            {
                Thread.Sleep(10);
                int packet = _captureClient.GetNextPacketSize();
                while (packet != 0 && !_stop)
                {
                    IntPtr buf = _captureClient.GetBuffer(out int frames, out AudioClientBufferFlags flags);
                    int bytes = frames * _blockAlign;
                    if (_managed.Length < bytes) _managed = new byte[bytes];

                    if ((flags & AudioClientBufferFlags.Silent) == 0)
                        Marshal.Copy(buf, _managed, 0, bytes);
                    else
                        Array.Clear(_managed, 0, bytes);

                    _writer.Write(_managed, 0, bytes);
                    _captureClient.ReleaseBuffer(frames);
                    packet = _captureClient.GetNextPacketSize();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Capture pump error: {ex}");
        }
    }

    public void StopStreaming()
    {
        _stop = true;
        _pump?.Join(500);

        try { _client?.Stop(); } catch { }
        _writer?.Dispose();
        _writer = null;
        _client?.Dispose();
        _client = null;
        StreamStopped = true;
        Debug.Log("[TapStreamer]: stopped");
    }

    enum FF
    {
        RemoteNetwork, Speakers, LineLevel, Headphones, Microphone,
        Headset, Handset, UnknownPassthrough, SPDIF, DigitalDisplay, Unknown = 10
    }

    private FF FormFactor(MMDevice d)
    {
        try
        {
            if (d.Properties.Contains(PKEY_FormFactor))
                return (FF)Convert.ToInt32(d.Properties[PKEY_FormFactor].Value);
        }
        catch { }
        return FF.Unknown;
    }

    private MMDevice FindLineInOnDefaultOutput()
    {
        using var en = new MMDeviceEnumerator();
        var output = en.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        string adapter = output.DeviceFriendlyName;
        Guid container = TryGetContainerId(output);   // from before; Guid.Empty if unreadable

        var onSameBox = en.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active)
            .Where(c => container != Guid.Empty
                ? TryGetContainerId(c) == container
                : string.Equals(c.DeviceFriendlyName, adapter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (onSameBox.Count == 0)
        {
            //throw new InvalidOperationException(
            Debug.Log(
                $"Output '{output.FriendlyName}' exposes no capture endpoints — wrong device or line-in disabled.");
            return null;
        }

        var lineIns = onSameBox.Where(c => FormFactor(c) == FF.LineLevel).ToList();
        if (lineIns.Count == 1) return lineIns[0];

        // Zero or several — do not pick blindly. Report what the box offers.
        var dump = string.Join("\n", onSameBox.Select(c => $"    {c.FriendlyName}  ->  {FormFactor(c)}"));
        //throw new InvalidOperationException(
        Debug.Log(
            $"No unique Line-In on '{adapter}' ({lineIns.Count} LineLevel endpoints). Capture endpoints:\n{dump}");

        return null;
    }

    private Guid TryGetContainerId(MMDevice d)
    {
        try
        {
            if (d.Properties.Contains(PKEY_Device_ContainerId)
                && d.Properties[PKEY_Device_ContainerId].Value is Guid g)
                return g;
        }
        catch { /* this NAudio build doesn't decode VT_CLSID */ }
        return Guid.Empty;   // fall back to DeviceFriendlyName
    }
}
