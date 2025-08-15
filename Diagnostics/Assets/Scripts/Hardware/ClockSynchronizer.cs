using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(SyncPulseDetector))]
public class ClockSynchronizer : MonoBehaviour
{
    public enum SyncStatus { Idle, Recording, Error}

    [SerializeField] public float pollInterval_s = 5;
    [SerializeField] private string _comPort = "COM23";
    [SerializeField] private SyncPulseDetector _syncPulseDetector;
    [SerializeField] private AudioSource _audioSource;

    public SyncStatus Status { get; private set; }
    public string LogFile { get; private set; }

    private int _numPulsesPerPoll = 4;
    private float _pulseCarrierFreq = 1000f;
    private float _pulseInterval_ms = 1f;
    private int _pulseChannel = -1;

    private float[] _signal;

    private bool _generatePulse = false;
    private double _lastAudioDSPTime = 0;
    private bool _stopSynchronizing;

    private string _logPath = "";
    private bool _serialPortOK;
    private bool _detectPulses = true;

    public int ChannelIndex { get; private set; }
    public int PulsesGenerated { get; private set; }
    public int PulsesDetected { get; private set; }

    public bool Initialize(string comPort)
    {
        CreateSignal();

        _comPort = comPort;
        Status = SyncStatus.Idle;

        if (string.IsNullOrEmpty(_comPort))
        {
            _serialPortOK = true;
            _detectPulses = false;
            Debug.Log("[Clock Synchronizer] no COM port specified, running without");
        }
        else
        {
            _serialPortOK = _syncPulseDetector.InitializeSerialPort(_comPort);
        }
        return _serialPortOK;
    }

    void CreateSignal()
    {
        var config = AudioSettings.GetConfiguration();

        int npts = Mathf.RoundToInt(config.sampleRate / _pulseCarrierFreq);
        var pulse = new float[npts];
        for (int k = 0; k < npts; k++) pulse[k] = Mathf.Sin(2 * Mathf.PI * _pulseCarrierFreq * k / config.sampleRate);

        _signal = new float[config.dspBufferSize];

        int nskip = Mathf.RoundToInt(config.sampleRate * _pulseInterval_ms / 1000);
        int offset = 0;
        for (int k=0; k<_numPulsesPerPoll; k++)
        {
            for (int j = 0; j < npts; j++) _signal[j + offset] = pulse[j];
            offset += nskip;
        }
    }

    public void StartSynchronizing(string logPath = "")
    {
        _stopSynchronizing = false;
        _audioSource.Play();
        PulsesGenerated = 0;
        PulsesDetected = 0;

        InitializeLogFile(logPath);

        Debug.Log($"[ClockSynchronizer] Start synchronizing to {_logPath}");

        InvokeRepeating("Synchronize", 5, pollInterval_s);

        Status = SyncStatus.Recording;
    }

    public void StopSynchronizing()
    {
        Debug.Log("[ClockSynchronizer] Stop synchronizing");
        _stopSynchronizing = true;
        CancelInvoke("Synchronize");

        _audioSource.Stop();

        Status = SyncStatus.Idle;
    }

    private void InitializeLogFile(string logPath)
    {
        LogFile = "";
        if (string.IsNullOrEmpty(logPath))
        {
            var folder = Path.Combine(Application.persistentDataPath, "Audio Sync Logs");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logPath = Path.Combine(folder, $"Audio-Sync-{ts}.log");
        }
        else
        {
            _logPath = Path.Combine(FileLocations.SubjectFolder, logPath.Replace(".json", "-AudioSync.log"));
        }
        LogFile = _logPath;

        string headerText = 
            $"{"SystemTime",20}\t" +
            $"{"UnityTime",12}\t" + 
            $"{"SyncPulseDetection",18}\t" +
            $"{"SyncDSPTime",12}\t" +
            $"{"SyncSystemTime",20}\t" + 
            $"{"SyncOffset",10}\t" + 
            $"{"RTT", 6}";

        File.WriteAllText(_logPath, headerText + Environment.NewLine);
    }

    private async void Synchronize()
    {
        if (_stopSynchronizing) return;

        var systemTime = HighPrecisionClock.UtcNowIn100nsTicks;
        var unityTime = Time.realtimeSinceStartupAsDouble;

        _generatePulse = true;
        PulsesGenerated++;

        await Task.Delay(500);
        var syncPulseEvent = new SyncPulseDetector.SyncPulseEvent();
        if (_detectPulses)
        {
            syncPulseEvent = await Task.Run(() => _syncPulseDetector.DetectOnePulse());
        }

        string logEntry =
            $"{systemTime,20}\t" +
            $"{unityTime,12:0.000000}\t" +
            $"{syncPulseEvent.result,18}\t";

        if (syncPulseEvent.result == SyncPulseDetector.SyncPulseEvent.Result.Detected)
        {
            if (!_stopSynchronizing)
            {
                Status = SyncStatus.Recording;
            }
            PulsesDetected++;
            logEntry +=
                $"{_lastAudioDSPTime,12:0.000000}\t" +
                $"{syncPulseEvent.systemTime,20}\t" +
                $"{syncPulseEvent.offset,10:0.000000}\t" +
                $"{syncPulseEvent.rtt,6:0.000000}";
        }
        else
        {
            Status = _detectPulses ? SyncStatus.Error : SyncStatus.Recording;
            logEntry +=
                $"{float.NaN,12}\t" +
                $"{float.NaN,20}\t" +
                $"{float.NaN,10}\t" +
                $"{float.NaN,6}";
        }

        File.AppendAllText(_logPath, logEntry + Environment.NewLine);
    }

    private void MeasureOnePulse()
    {
        _generatePulse = true;
        _syncPulseDetector.DetectOnePulse();
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_generatePulse && channels > 2)
        {
            _lastAudioDSPTime = AudioSettings.dspTime;
            _generatePulse = false;

            ChannelIndex = _pulseChannel >= 0 ? _pulseChannel : (channels - 1);
            int index = ChannelIndex;
            for (int k=0; k < _signal.Length; k++)
            {
                data[index] = _signal[k];
                index += channels;
            }

        }
        
    }
}
