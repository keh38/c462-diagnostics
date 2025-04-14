using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using KLib;
using Pupillometry;

public class GazeCalibration : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private Image _target;
    [SerializeField] private Image _hole;

    private bool _stopServer;

    bool _isFinished = false;
    bool _canRespond = true;

    int _numTargets;
    int _numAcquired;

    private GazeCalibrationSettings _settings;

    private string _mySceneName = "Gaze Calibration";

    void Start()
    {
        _target.gameObject.SetActive(false);
        HTS_Server.SetCurrentScene(_mySceneName, this);
    }

    void Initialize(string data)
    {
        _settings = FileIO.XmlDeserializeFromString<GazeCalibrationSettings>(data);

        _numTargets = int.Parse(_settings.CalibrationType.Substring(_settings.CalibrationType.Length - 1)) + 1;
        _numAcquired = 0;

        float size = Screen.width / _settings.TargetSizeFactor;
        _target.rectTransform.sizeDelta = new Vector2(size, size);
        _target.color = KLib.ColorTranslator.ColorFromARGB(_settings.TargetColor);

        size = Screen.width / _settings.HoleSizeFactor;
        _hole.rectTransform.sizeDelta = new Vector2(size, size);
        _hole.color = KLib.ColorTranslator.ColorFromARGB(_settings.BackgroundColor);

        GetComponent<Camera>().backgroundColor = KLib.ColorTranslator.ColorFromARGB(_settings.BackgroundColor);

        _isFinished = false;
    }

    private void ShowTarget(int x, int y)
    {
        _target.gameObject.SetActive(true);
        Debug.Log($"set target to {x}, {y}");

        _target.rectTransform.position = new Vector2(x, Screen.height - y);
        _numAcquired++;
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A)
        {
            if (!_isFinished)
            {
                _target.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        //if (!_finished && (Input.GetButtonDown("XboxA") || Input.GetMouseButtonDown(0) || Input.GetKeyDown(_settings.keyCode)))
        if (!_isFinished && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(_settings.KeyCode)))
        {
            HTS_Server.SendMessage("Gaze Calibration", "Response");
            //_canRespond = false;

            if (_numAcquired == _numTargets)
            {
                _numAcquired++;
                _target.gameObject.SetActive(false);
                _isFinished = true;
                HTS_Server.SendMessage("Gaze Calibration", "GazeCalibrationFinished");
            }
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
                Initialize(data);
                break;
            case "Abort":
                _isFinished = true;
                _target.gameObject.SetActive(false);
                break;
            case "Location":
                var parts = data.Split(',');
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);
                _canRespond = true;
                ShowTarget(x, y);
                break;
        }

    }

}

