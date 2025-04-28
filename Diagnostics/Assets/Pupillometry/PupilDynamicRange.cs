using System;
using System.Collections;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;


using Pupillometry;

public class PupilDynamicRange : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private Camera _camera;

    private bool _isRunning = false;

    private bool _useLEDs = false;

    private float _preBaseLine = 2f;
    private float _postBaseLine = 2f;
    private float _curTime = 0;
    private float _modRateHz = 0.05f;
    private int _curPeriod = 0;
    private int _numReps = 4;

    private float _endStimTime;
    private float _endRunTime;

    private float _nextUpdate;

    private string _dataPath;
    private DynamicRangeData _data;

    private bool _stopMeasurement;

    private string _mySceneName = "Pupil Dynamic Range";

    void Start()
    {
        HTS_Server.SetCurrentScene(_mySceneName, this);
    }

    void InitializeMeasurement(string data)
    {
        _stopMeasurement = false;

        _endStimTime = _preBaseLine + _numReps / _modRateHz;
        _endRunTime = _endStimTime + _postBaseLine;

        _nextUpdate = 1;

        string fn = $"{GameManager.Subject}-PupilDR-{DateTime.Now.ToString("yyyy-MM-dd_HHmmss")}.json";
        _dataPath = Path.Combine(FileLocations.SubjectFolder, fn);

        float approxDuration = 10;
        int npts = Mathf.RoundToInt(approxDuration * 100);

        _data = new DynamicRangeData(_dataPath, npts);
        HTS_Server.SendMessage(_mySceneName, $"File:{Path.GetFileName(_dataPath)}");

        _useLEDs = HardwareInterface.LED.IsInitialized;
        if (_useLEDs)
        {
            HardwareInterface.LED.Clear();
            HardwareInterface.LED.Open();
        }
    }

    void Begin()
    {
        _isRunning = true;
    }

    void Update()
    {
        if (!_isRunning) return;

        float intensity = 0;

        if (_curTime >= _preBaseLine && _curTime < _endStimTime)
        {
            int nper = Mathf.FloorToInt(_curTime * _modRateHz) + 1;
            if (nper > _curPeriod)
            {
                _curPeriod++;
            }

            intensity = 0.5f * (1 - Mathf.Cos(2 * Mathf.PI * (_curTime - _preBaseLine) * _modRateHz));
        }

        _data.Add(Time.realtimeSinceStartupAsDouble, intensity);
        SetScreenIntensity(intensity);

        if (_useLEDs)
        {
            HardwareInterface.LED.SetColorDynamically(intensity);
        }

        _curTime += Time.deltaTime;

        if (_curTime > _nextUpdate)
        {
            _nextUpdate += 1;
            HTS_Server.SendMessage(_mySceneName, $"Progress:{Mathf.RoundToInt(_curTime/_endRunTime * 100)}");
        }

        if (_curTime > _endRunTime || _stopMeasurement)
        {
            _isRunning = false;
            EndTest();
        }
    }

    private void SetScreenIntensity(float intensity)
    {
        _camera.backgroundColor = new Color(intensity, intensity, intensity);
    }

    private void EndTest()
    {
        if (_useLEDs)
        {
            HardwareInterface.LED.Close();
        }

        _data.Trim();
        KLib.FileIO.JSONSerialize(_data, _dataPath);

        string status = _stopMeasurement ? "Measurement aborted" : "Measurement finished";
        HTS_Server.SendMessage(_mySceneName, $"Finished:{status}");
        HTS_Server.SendMessage(_mySceneName, $"ReceiveData:{Path.GetFileName(_dataPath)}:{File.ReadAllText(_dataPath)}");
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (_isRunning && e.control && e.keyCode == KeyCode.A)
        {
            _isRunning = false;
            _stopMeasurement = true;
            EndTest();
            SceneManager.LoadScene("Home");
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    void IRemoteControllable.ProcessRPC(string command, string data)
    {
        switch (command)
        {
            case "Initialize":
                InitializeMeasurement(data);
                break;
            case "StartSynchronizing":
                HardwareInterface.ClockSync.StartSynchronizing(Path.GetFileName(data));
                break;
            case "StopSynchronizing":
                HardwareInterface.ClockSync.StopSynchronizing();
                break;
            case "Begin":
                Begin();
                break;
            case "Abort":
                _stopMeasurement = true;
                break;
            case "SendSyncLog":
                var logPath = HardwareInterface.ClockSync.LogFile;
                if (!string.IsNullOrEmpty(logPath))
                {
                    HTS_Server.SendMessage(_mySceneName, $"ReceiveData:{Path.GetFileName(logPath)}:{File.ReadAllText(logPath)}");
                }
                break;
        }
    }
}

