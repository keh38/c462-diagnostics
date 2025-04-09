using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using KLib;
using Pupillometry;

public class PupillometryCal : MonoBehaviour
{
    //public UISprite target;
    //public UISprite hole;
    //public UILabel prompt;

    //private Sockets.KTcpListener _socket = null;
    private bool _stopServer;

    //private CalibrationSettings _settings = new CalibrationSettings();
    bool _finished = false;
    int _readPort = 4975;
    int _writePort = 4976;

    int _numTargets;
    int _numAcquired;

    int _left = 0;
    int _top = 0;
    int _right = 0;
    int _bottom = 0;

    void Start()
    {
        //NGUITools.SetActive(prompt.gameObject, false);

        string configFilePath = FileLocations.ConfigFile("PupilCal.DefaultSettings");

        if (!File.Exists(configFilePath))
        {
            configFilePath = "";
        }

        //if (!string.IsNullOrEmpty(DiagnosticsManager.Instance.SettingsFile))
        //{
        //    configFilePath = DataFileLocations.ConfigFile(DiagnosticsManager.Instance.SettingsFile);
        //}

        //if (!string.IsNullOrEmpty(configFilePath))
        //{
        //    _settings = KLib.FileIO.XmlDeserialize<CalibrationSettings>(configFilePath);
        //}

        //_numTargets = int.Parse(_settings.calibrationType.Substring(_settings.calibrationType.Length - 1)) + 1;
        //_numAcquired = 0;

        //if (_settings.width > 0)
        //{
        //    _left = (Screen.width - _settings.width) / 2;
        //    _top = (Screen.height - _settings.height) / 2;
        //    _right = _left + _settings.width;
        //    _bottom = _top + _settings.height;
        //}
        //else
        //{
        //    _left = 0;
        //    _top = 0;
        //    _right = Screen.width;
        //    _bottom = Screen.height;
        //}

        //IPC.Instance.SendCommand("PupilCal:" + _settings.calibrationType + " " + _left + " " + _top + " " + _right + " " + _bottom);
        ////IPC.Instance.SendCommand("PupilCal:" + _settings.calibrationType + " " + _right + " " + _bottom);

        //target.width = Mathf.RoundToInt((float)Screen.width / _settings.targetSizeFactor);
        //target.height = target.width;
        //target.color = KLib.Unity.ColorFromARGB(_settings.targetColor);

        //hole.width = Mathf.RoundToInt((float)Screen.width / _settings.holeSizeFactor);
        //hole.height = hole.width;
        //hole.color = KLib.Unity.ColorFromARGB(_settings.backgroundColor);

        //NGUITools.SetActive(target.gameObject, false);
        //NGUITools.SetActive(hole.gameObject, false);
        //GetComponent<Camera>().backgroundColor = KLib.Unity.ColorFromARGB(_settings.backgroundColor);

        //StartServer();
	}
	
    private void ShowTarget(int x, int y)
    {
        //NGUITools.SetActive(target.gameObject, true);
        //NGUITools.SetActive(hole.gameObject, true);

        x = -Screen.width / 2 + x;
        y = Screen.height / 2 - y;

        //target.transform.localPosition = new Vector2(x, y);
        _numAcquired++;
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A)
        {
            if (!_finished)
            {
                //IPC.Instance.SendCommand(_writePort, "Stop");
                StartCoroutine(Finished());
            }
        }
    }

    void Update()
    {
        //if (!_finished && (Input.GetButtonDown("XboxA") || Input.GetMouseButtonDown(0) || Input.GetKeyDown(_settings.keyCode)))
        //{
        //    IPC.Instance.SendCommand(_writePort, "Response");

        //    if (_numAcquired == _numTargets)
        //    {
        //        _numAcquired++;
        //        NGUITools.SetActive(target.gameObject, false);
        //        NGUITools.SetActive(hole.gameObject, false);
        //        NGUITools.SetActive(prompt.gameObject, true);

        //        string msg = "press A or click mouse to finish";
        //        if (!string.IsNullOrEmpty(_settings.finalPrompt))
        //            msg = _settings.finalPrompt;

        //        prompt.text = msg;
        //    }
        //}
    }

    IEnumerator Finished()
    {
        yield break;
        //DiagnosticsManager.Instance.AdvanceProtocol();
        //yield return new WaitForSeconds(1);
        //Application.LoadLevel(DiagnosticsManager.Instance.ReturnToScene);
    }

    public void StartServer()
    {
        //_stopServer = false;

        //_socket = new Sockets.KTcpListener();
        //_socket.StartListener(_readPort);

        //StartCoroutine(TCPServer());

        //Debug.Log("started pupil calibration TCP server");
    }

    public void StopServer()
    {
        //_stopServer = true;
        //if (_socket != null)
        //{
        //    _socket.CloseListener();
        //    _socket = null;
        //}

    }

    void OnDestroy()
    {
        StopServer();
    }

    IEnumerator TCPServer()
    {
        yield break;
        //while (!_stopServer)
        //{
        //    if (_socket.Pending())
        //    {
        //        ProcessMessage();
        //    }
        //    yield return null;
        //}
    }

    void ProcessMessage()
    {
        //_socket.AcceptTcpClient();
        //string command = _socket.ReadStringFromInputStream();
        //Debug.Log("Command received: " + command);

        //var parts = command.Split(new char[] { ':' });
        //command = parts[0];

        //switch (command)
        //{
        //    case "Ping":
        //        _socket.SendAcknowledgement();
        //        break;

        //    case "finished":
        //        _socket.SendAcknowledgement();
        //        _stopServer = true;
        //        StartCoroutine(Finished());
        //        Debug.Log("Finished!");
        //        break;

        //    case "location":
        //        _socket.SendAcknowledgement();
        //        int x = int.Parse(parts[1]);
        //        int y = int.Parse(parts[2]);
        //        ShowTarget(x, y);
        //        break;

        //}

        //_socket.CloseTcpClient();
    }

}

