using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Audiograms;
using KLib;

using BasicMeasurements;
using DigitsTest;

public class DigitsTestController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private TextAsset _defaultInstructions;
    [SerializeField] private Text _finishText;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private QuestionBox _questionBox;
    [SerializeField] private GameObject _workPanel;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private DigitDisplay _digitDisplay;
    [SerializeField] private GameObject _fixationImage;
    [SerializeField] private GameObject _speakerPrefab;

    private bool _isRemote;

    private bool _stopMeasurement = false;
    private bool _localAbort = false;

    private DigitsTestSettings _settings = new DigitsTestSettings();

    private string _dataPath;
    private string _mySceneName = "Digits Test";
    private string _configName;

    private InputAction _abortAction;

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += OnAbortAction;

        Application.logMessageReceived += HandleException;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleException;
    }

    private void Start()
    {
        HTS_Server.SetCurrentScene(_mySceneName, this);

        _title.text = "";

#if HACKING
        Application.targetFrameRate = 60;
        GameManager.SetSubject("Scratch/_shit");
        _configName = "Test";
#else
        _configName = GameManager.DataForNextScene;
#endif

        _digitDisplay.OnFirstPress = OnFirstDigitPressCallback;
        _digitDisplay.OnEnter = OnEnterCallback;
        _digitDisplay.SetButtonStates(KeypadButtonState.DisabledAndGrayed, 7);
        _digitDisplay.Hide();

        _fixationImage.gameObject.SetActive(false);

        if (string.IsNullOrEmpty(_configName))
        {
            _isRemote = HTS_Server.RemoteConnected;
            if (!_isRemote)
            {
                ShowFinishPanel("Nothing to do");
            }
        }
        else
        {
            var fn = FileLocations.ConfigFile("Digits", _configName);
//            _settings = FileIO.XmlDeserialize<BasicMeasurementConfiguration>(fn) as DigitsTestSettings;
            _settings = new DigitsTestSettings();
            InitializeMeasurement();
            Begin();
        }
    }

    void InitializeMeasurement()
    {
        InitDataFile();

        HTS_Server.SendMessage(_mySceneName, $"File:{Path.GetFileName(_dataPath)}");
    }

    void InitDataFile()
    {
        var fileStemStart = $"{GameManager.Subject}-{_mySceneName}";
        while (true)
        {
            string fileStem = $"{fileStemStart}-Run{GameManager.GetNextRunNumber(_mySceneName):000}";
            fileStem = Path.Combine(FileLocations.SubjectFolder, fileStem);
            _dataPath = fileStem + ".json";
            if (!File.Exists(_dataPath))
            {
                break;
            }
        }

        var header = new BasicMeasurementFileHeader()
        {
            measurementType = _mySceneName,
            configName = _configName,
            subjectID = GameManager.Subject
        };

        string json = FileIO.JSONStringAdd("", "info", KLib.FileIO.JSONSerializeToString(header));
        json = KLib.FileIO.JSONStringAdd(json, "params", KLib.FileIO.JSONSerializeToString(_settings));
        json += Environment.NewLine;

        File.WriteAllText(_dataPath, json);
    }

    private void CreatePlan()
    {

    }

    private void Begin()
    {
        _localAbort = false;
        _stopMeasurement = false;

        if (_settings.UseDefaultInstructions || !string.IsNullOrEmpty(_settings.InstructionMarkdown))
        {
            if (_settings.UseDefaultInstructions)
            {
                _settings.InstructionMarkdown = _defaultInstructions.text;
            }

            HTS_Server.SendMessage(_mySceneName, "Status:Instructions");
            ShowInstructions(
                instructions: _settings.InstructionMarkdown,
                fontSize: _settings.InstructionFontSize);
        }
        else
        {
            StartMeasurement();
        }
    }

    private void StartMeasurement()
    {
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(true);

        StartCoroutine(Shit());
    }

    IEnumerator Shit()
    {
        yield return new WaitForSeconds(2);
        _digitDisplay.Show();
    }

    void OnAbortAction(InputAction.CallbackContext context)
    {
        _abortAction.Disable();

        _workPanel.SetActive(false);
        _instructionPanel.gameObject.SetActive(false);
        _quitPanel.SetActive(true);
    }

    void Update()
    {
        if (_stopMeasurement)
        {
            _abortAction.Disable();
            _stopMeasurement = false;
            EndRun(abort: true);
        }
    }

    public void OnQuitConfirmButtonClick()
    {
        _localAbort = true;
        _stopMeasurement = true;
    }

    public void OnQuitCancelButtonClick()
    {
        _quitPanel.SetActive(false);
        _abortAction.Enable();
    }

    private void OnFirstDigitPressCallback()
    {
        Debug.Log("First Press!!!");
    }

    private void OnEnterCallback()
    {
        Debug.Log("Enter!");
    }

    private void ShowInstructions(string instructions, int fontSize)
    {
        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = StartMeasurement;
        _instructionPanel.ShowInstructions(new Turandot.Instructions() { Text = instructions, FontSize = fontSize });
    }

    private void EndRun(bool abort)
    {
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(false);

        string status = abort ? "Measurement aborted" : "Measurement finished";
        HTS_Server.SendMessage(_mySceneName, $"Finished:{status}");
        HTS_Server.SendMessage(_mySceneName, $"ReceiveData:{Path.GetFileName(_dataPath)}:{File.ReadAllText(_dataPath)}");

        if (_localAbort)
        {
            SceneManager.LoadScene("Home");
        }

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

    void IRemoteControllable.ProcessRPC(string command, string data)
    {
        switch (command)
        {
            case "Initialize":
                _settings = FileIO.XmlDeserializeFromString<BasicMeasurementConfiguration>(data) as DigitsTestSettings;
                InitializeMeasurement();
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
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    public void HandleException(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Log || type == LogType.Warning)
        {
            return;
        }

        try
        {
            _workPanel.SetActive(false);
        }
        catch { }

        HandleError(condition, stackTrace);
    }

    void HandleError(string error, string stackTrace = "")
    {
        if (error.Equals("Exception"))
        {
            error = "An exception occurred";
        }

        HTS_Server.SendMessage(_mySceneName, $"Error:{error}");
        Debug.Log($"[{_mySceneName} error]: {error}{Environment.NewLine}{stackTrace}");

        if (!_isRemote)
        {
            ShowFinishPanel("The run was stopped because of an error");
        }
    }
}
