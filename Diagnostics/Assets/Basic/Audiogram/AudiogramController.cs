using Audiograms;
using KLib;
using Protocols;
using Pupillometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudiogramController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private Text _finishText;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private GameObject _workPanel;

    private Protocol _protocol;
    private ProtocolHistory _history;

    private bool _isRemote;
    private int _nextTestIndex;
    private bool _advanceAfterInstructions = false;

    private bool _waitingForResponse = false;
    private bool _stopMeasurement = false;

    private AudiogramMeasurementSettings _settings = new AudiogramMeasurementSettings();

    private string _dataPath;
    private string _mySceneName = "Audiogram";

    private void Start()
    {
        HTS_Server.SetCurrentScene(_mySceneName, this);

        _title.text = "";

        if (string.IsNullOrEmpty(GameManager.DataForNextScene))
        {
            _isRemote = HTS_Server.RemoteConnected;
            if (!_isRemote)
            {
                ShowFinishPanel("Nothing to do");
            }
        }     
    }
    void InitializeMeasurement(string data)
    {
        Cursor.visible = false;

        _settings = FileIO.XmlDeserializeFromString<BasicMeasurementConfiguration>(data) as AudiogramMeasurementSettings;

        InitDataFile();

        HTS_Server.SendMessage(_mySceneName, $"File:{Path.GetFileName(_dataPath)}");

        _workPanel.SetActive(false);
    }

    void InitDataFile()
    {
        var fileStemStart = $"{GameManager.Subject}-Audiogram";
        while (true)
        {
            string fileStem = $"{fileStemStart}-Run{GameManager.GetNextRunNumber("Audiogram"):000}";
            fileStem = Path.Combine(FileLocations.SubjectFolder, fileStem);
            _dataPath = fileStem + ".json";
            if (!File.Exists(_dataPath))
            {
                break;
            }
        }

        //    var header = new Turandot.FileHeader();
        //    header.Initialize(_mainDataFile, _paramFile);
        //    header.audioSamplingRate = AudioSettings.outputSampleRate;
        //    AudioSettings.GetDSPBufferSize(out header.audioBufferLength, out header.audioNumBuffers);
        //    if (_params.screen.ApplyCustomScreenColor)
        //    {
        //        header.screenColor = GameManager.ScreenColorString;
        //        if (HardwareInterface.LED.IsInitialized)
        //        {
        //            header.ledColor = GameManager.LEDColorString;
        //        }
        //    }

        string json = KLib.FileIO.JSONStringAdd("", "params", KLib.FileIO.JSONSerializeToString(_settings));
        json += Environment.NewLine;

        File.WriteAllText(_dataPath, json);
        Debug.Log($"data path = {_dataPath}");

        //    _state.SetDataFile(_mainDataFile);
    }


    void Update()
    {
        if (_stopMeasurement)
        {
            _stopMeasurement = false;
            EndRun(abort: true);
        }

        if (!_waitingForResponse) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            _waitingForResponse = false;

            if (!string.IsNullOrEmpty(_protocol.Tests[_nextTestIndex].Instructions))
            {
                ShowInstructions(
                    _protocol.Tests[_nextTestIndex].Instructions, 
                    fontSize: _protocol.Appearance.InstructionFontSize,
                    autoAdvance: true);
            }
            else
            {
                HTS_Server.SendMessage("Protocol", "Advance");
            }
        }
    }

    void OnGUI()
    {
        if (!_waitingForResponse) return;

        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A)
        {
            _waitingForResponse = false;
            _quitPanel.SetActive(true);
        }
    }

    public void OnQuitConfirmButtonClick()
    {
        SceneManager.LoadScene("Home");
    }

    public void OnQuitCancelButtonClick()
    {
        _quitPanel.SetActive(false);
        _waitingForResponse = true;
    }

    private void ShowInstructions(string instructions, int fontSize, bool autoAdvance)
    {
        _advanceAfterInstructions = autoAdvance;
        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = OnInstructionsFinished;
        _instructionPanel.ShowInstructions(new Turandot.Instructions() { Text = instructions, FontSize = fontSize });
    }
    private void OnInstructionsFinished()
    {
        _instructionPanel.gameObject.SetActive(false);
        if (_advanceAfterInstructions)
        {
            HTS_Server.SendMessage("Protocol", "Advance");
        }
        else
        {
//            StartCoroutine(AnimateOutline());
        }
    }

    private void EndRun(bool abort)
    {
        _workPanel.SetActive(false);

        string status = abort ? "Measurement aborted" : "Measurement finished";
        HTS_Server.SendMessage(_mySceneName, $"Finished:{status}");
        HTS_Server.SendMessage(_mySceneName, $"ReceiveData:{Path.GetFileName(_dataPath)}:{File.ReadAllText(_dataPath)}");

        bool finished = true;
        if (finished && !_isRemote)
        {
            ShowFinishPanel();
        }
    }

    private void ShowFinishPanel(string message = "")
    {
        _finishText.text = message;
        _finishPanel.SetActive(true);
    }

    public void OnFinishButtonClick()
    {
        Return();
    }

    private void Return()
    {
        SceneManager.LoadScene("Home");
    }

    private void RpcBegin()
    {
        _stopMeasurement = false;
        _workPanel.SetActive(true);
        return;

        if (!string.IsNullOrEmpty(_protocol.Introduction))
        {
            HTS_Server.SendMessage("Protocol", "Instructions");
            ShowInstructions(
                _protocol.Introduction, 
                fontSize: _protocol.Appearance.InstructionFontSize,
                autoAdvance: false);
        }
        else
        {
//            StartCoroutine(AnimateOutline());
        }
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
                RpcBegin();
                break;
            case "Abort":
                _stopMeasurement = true;
                break;
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }
}
