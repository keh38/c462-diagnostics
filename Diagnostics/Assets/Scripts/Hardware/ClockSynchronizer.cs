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

    [SerializeField] private float _pollInterval_s = 1;
    [SerializeField] private string _comPort = "COM23";
    [SerializeField] private SyncPulseDetector _syncPulseDetector;

    public SyncStatus Status { get; private set; }
    public string LogFile { get; private set; }

    private int _numPulsesPerPoll = 4;
    private float _pulseCarrierFreq = 1000f;
    private float _pulseInterval_ms = 1f;
    private int _pulseChannel = -1;

    private float[] _signal;

    private bool _generatePulse = false;
    private double _lastAudioDSPTime = 0;

    private string _logPath = "";
    private bool _serialPortOK;

    private Thread _syncThread;
    private bool _stopThread;

    public void Initialize(string comPort)
    {
        CreateSignal();

        _comPort = comPort;
        Status = SyncStatus.Idle;

        if (string.IsNullOrEmpty(_comPort))
        {
            _serialPortOK = true;
            Debug.Log("[Clock Synchronizer] no COM port specified, running without");
            return;
        }

        _serialPortOK = _syncPulseDetector.InitializeSerialPort(_comPort);
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

        //_syncThread = new Thread(new ThreadStart(SyncThread));
        //_syncThread.IsBackground = true;
        //_stopThread = false;

        //_syncThread.Start();


        InvokeRepeating("Synchronize", 1, _pollInterval_s);

        //Status = _serialPortOK ? SyncStatus.Recording : SyncStatus.Error;
        Status = SyncStatus.Recording;
    }

    public void StopSynchronizing()
    {
        Debug.Log("[ClockSynchronizer] Stop synchronizing");
        CancelInvoke("Synchronize");
        //_stopThread = true;
        //_syncThread.Abort();

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

    private void SyncThread()
    {
        var lastTime = DateTime.MinValue;
        while (!_stopThread)
        {
            Synchronize();
            lastTime = DateTime.Now;

            while (!_stopThread && ((DateTime.Now - lastTime).TotalSeconds < _pollInterval_s))
            {
                Thread.Sleep(500);
            }
        }
    }

    private async void Synchronize()
    {
        var systemTime = HighPrecisionClock.UtcNowIn100nsTicks;
        var unityTime = Time.realtimeSinceStartupAsDouble;

        _generatePulse = true;

        await Task.Delay(200);
        //var syncPulseEvent = await Task.Run(() => _syncPulseDetector.DetectOnePulse());
        var syncPulseEvent = new SyncPulseDetector.SyncPulseEvent();

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
