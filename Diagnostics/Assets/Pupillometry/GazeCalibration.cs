using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using KLib;
using KLibU;
using KLibU.Net;
using Pupillometry;

using C462.Shared;

using KeyCode = UnityEngine.KeyCode;
using C462.Shared.Protocol.DTOs;

public class GazeCalibration : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private Image _target;
    [SerializeField] private Image _hole;

    private bool _stopServer;

    bool _isRunning = false;
    bool _canRespond = true;

    int _numTargets;
    int _numAcquired;

    private GazeCalibrationSettings _settings;

    private string _mySceneName = "Gaze Calibration";
    private string _dataPath;

    private GazeCalibrationLog _data = new GazeCalibrationLog();

    void Start()
    {
        Cursor.visible = false;

        _target.gameObject.SetActive(false);
        HTS_Server.SetCurrentScene(_mySceneName, this);
    }

    private void OnDisable()
    {
        Cursor.visible = true;
    }

    void Initialize(GazeCalibrationSettings settings)
    {
        try
        {
            _settings = settings;

            _numTargets = int.Parse(_settings.CalibrationType.Substring(_settings.CalibrationType.Length - 1)) + 1;
            _numAcquired = 0;

            float size = Screen.width / _settings.TargetSizeFactor;
            _target.rectTransform.sizeDelta = new Vector2(size, size);
            _target.color = KLib.UnityColorTranslator.ColorFromARGB(_settings.TargetColor);

            size = Screen.width / _settings.HoleSizeFactor;
            _hole.rectTransform.sizeDelta = new Vector2(size, size);
            _hole.color = KLib.UnityColorTranslator.ColorFromARGB(_settings.BackgroundColor);

            GetComponent<Camera>().backgroundColor = KLib.UnityColorTranslator.ColorFromARGB(_settings.BackgroundColor);

            string fn = $"{GameManager.Subject}-GazeCalibration-{DateTime.Now.ToString("yyyy-MM-dd_HHmmss")}.json";
            _dataPath = Path.Combine(SharedFileLocations.HtsSubjectDataFolder, fn);

            var header = new BasicMeasurementFileHeader()
            {
                measurementType = "GazeCalibration",
                subjectID = GameManager.Subject
            };

            string json = Files.JSONStringAdd("", "info", KLibU.Files.JSONSerializeToString(header));
            File.WriteAllText(_dataPath, json);

            HTS_Server.SendRequest(_mySceneName, $"File:{Path.GetFileName(_dataPath)}");
        }
        catch (Exception ex)
        {
            Debug.Log($"Gaze calibration error: {ex.Message}");
        }

        _isRunning = true;
    }

    private void ShowTarget(int x, int y)
    {
        _target.gameObject.SetActive(true);
        _data.Add(Time.time, x, y);
        Debug.Log($"set target to {x}, {y}");

        _target.rectTransform.position = new Vector2(x, Screen.height - y);
        _numAcquired++;
    }

    private void ShowTargetNormalized(float x, float y)
    {
        _target.gameObject.SetActive(true);
        _data.Add(Time.time, x, y);
        Debug.Log($"set target to {x}, {y}");

        _target.rectTransform.anchorMin = new Vector2(x, 1 - y);
        _target.rectTransform.anchorMax = new Vector2(x, 1 - y);
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A)
        {
            if (_isRunning)
            {
                _target.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        //if (!_finished && (Input.GetButtonDown("XboxA") || Input.GetMouseButtonDown(0) || Input.GetKeyDown(_settings.keyCode)))
        if (_isRunning && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(_settings.KeyCode)))
        {
            HTS_Server.SendRequest("Gaze Calibration", "Response");
            //_canRespond = false;

            if (_numAcquired == _numTargets)
            {
                _numAcquired++;
                _target.gameObject.SetActive(false);
                _isRunning = false;
                HTS_Server.SendRequest("Gaze Calibration", "GazeCalibrationFinished");
            }
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    TcpMessage IRemoteControllable.ProcessRPC(TcpMessage request)
    {
        switch (request.Command)
        {
            case "Initialize":
                var settings = request.GetPayload<GazeCalibrationSettings>();
                Initialize(settings);
                return TcpMessage.Ok(_dataPath);
            case "Abort":
                _isRunning = false;
                _target.gameObject.SetActive(false);
                return TcpMessage.Ok();
            case "Finish":
                _isRunning = false;
                _target.gameObject.SetActive(false);
                HTS_Server.SendRequest("Gaze Calibration", "GazeCalibrationFinished");
                return TcpMessage.Ok();
            case "SendData":
                _data.Trim();
                File.AppendAllText(_dataPath, Files.JSONSerializeToString(_data));
                var dataFilePayload = new TextFilePayload()
                {
                    Filename = Path.GetFileName(_dataPath),
                    Content = File.ReadAllText(_dataPath)
                };
                return TcpMessage.Ok(dataFilePayload);
            case "Location":
                var data = request.GetPayload<string>();
                var parts = data.Split(',');
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                _canRespond = true;
                ShowTarget(x, y);
                return TcpMessage.Ok();
            case "SetLocationNormalized":
                var targetPayload = request.GetPayload<TargetPointPayload>();
                ShowTargetNormalized(targetPayload.X, targetPayload.Y);
                return TcpMessage.Ok();
            default:
                return TcpMessage.NotFound(request.Command);
        }

    }

}

