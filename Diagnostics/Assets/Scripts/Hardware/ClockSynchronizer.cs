using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(SyncPulseDetector))]
public class ClockSynchronizer : MonoBehaviour
{
    public float PollInterval_s = 1;
    [SerializeField] private string _comPort = "COM23";

    [SerializeField] private SyncPulseDetector _syncPulseDetector;

    private int _numPulsesPerPoll = 4;
    private float _pulseCarrierFreq = 1000f;
    private float _pulseInterval_ms = 1f;
    private int _pulseChannel = -1;

    private float[] _signal;

    private bool _generatePulse = false;
    private double _lastAudioDSPTime = 0;

    private string _logPath = "";

    void Start()
    {
        CreateSignal();
    }

    public void Initialize(string comPort)
    {
        _comPort = comPort;
        if (string.IsNullOrEmpty(_comPort))
        {
            Debug.Log("[Clock Synchronizer] no COM port specified, running without");
            return;
        }

        _syncPulseDetector.InitializeSerialPort(_comPort);
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
        InitializeLogFile(logPath);

        Debug.Log($"[ClockSynchronizer] Start synchronizing to {_logPath}");
        InvokeRepeating("Synchronize", 1, PollInterval_s);
    }

    public void StopSynchronizing()
    {
        Debug.Log("[ClockSynchronizer] Stop synchronizing");
        CancelInvoke("Synchronize");
    }

    private void InitializeLogFile(string logPath)
    {
        if (string.IsNullOrEmpty(logPath))
        {
            var folder = Path.Combine(Application.persistentDataPath, "Audio Sync Logs");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var ts = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _logPath = Path.Combine(folder, $"Audio-Sync-{ts}.log");
        }
        else
        {
            _logPath = logPath;
        }

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
        var systemTime = HighPrecisionClock.UtcNowIn100nsTicks;
        var unityTime = Time.realtimeSinceStartupAsDouble;

        _generatePulse = true;

        await Task.Delay(200);
        var syncPulseEvent = await Task.Run(() => _syncPulseDetector.DetectOnePulse());

        string logEntry =
            $"{systemTime,20}\t" +
            $"{unityTime,12:0.000000}\t" +
            $"{syncPulseEvent.result,18}\t";
        if (syncPulseEvent.result == SyncPulseDetector.SyncPulseEvent.Result.Detected)
        {
            logEntry +=
                $"{_lastAudioDSPTime,12:0.000000}\t" +
                $"{syncPulseEvent.systemTime,20}\t" +
                $"{syncPulseEvent.offset,10:0.000000}\t" +
                $"{syncPulseEvent.rtt,6:0.000000}";
        }
        else
        {
            logEntry +=
                $"{float.NaN,12}\t" +
                $"{float.NaN,20}\t" +
                $"{float.NaN,10}\t" +
                $"{float.NaN,6}";
        }

        File.AppendAllText(_logPath, logEntry + Environment.NewLine);
    }

    private void BlockingOperation()
    {
        //var startTime = Time.realtimeSinceStartup;
        //while (Time.realtimeSinceStartup - startTime < 5) { }

        var startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds < 5) { }
    }

    private void MeasureOnePulse()
    {
        _generatePulse = true;
        _syncPulseDetector.DetectOnePulse();
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_generatePulse)
        {
            _lastAudioDSPTime = AudioSettings.dspTime;
            _generatePulse = false;

            int index = _pulseChannel >=0 ? _pulseChannel : (channels - 1);
            for (int k=0; k < _signal.Length; k++)
            {
                data[index] = _signal[k];
                index += channels;
            }

        }
        
    }
}
