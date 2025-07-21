using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using KLib;
using Questionnaires;

public class QuestionnaireController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private Text _finishText;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private GameObject _workPanel;

    [SerializeField] private QuestionnaireChecklist _checklist;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _nextButton;

    private bool _isRemote;

    private bool _stopMeasurement = false;
    private bool _localAbort = false;

    private Questionnaire _questionnaire = new Questionnaire();
    private QuestionnaireData _data;
    private int _qnum;

    private string _dataPath;
    private string _mySceneName = "Questionnaire";
    private string _configName;

    private InputAction _abortAction;

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += OnAbortAction;

        _checklist.SelectionChanged += OnChecklistSelectionChanged;
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
            var fn = FileLocations.ConfigFile("Questionnaire", _configName);
            _questionnaire = FileIO.XmlDeserialize<BasicMeasurementConfiguration>(fn) as Questionnaire;

            InitializeMeasurement();
            Begin();
        }
    }

    void InitializeMeasurement()
    {
        _title.text = _questionnaire.Title;

        _data = new QuestionnaireData(_questionnaire);
        InitDataFile();

        HTS_Server.SendMessage(_mySceneName, $"File:{Path.GetFileName(_dataPath)}");
    }

    void InitDataFile()
    {
        var fileStemStart = $"{GameManager.Subject}-{_mySceneName}-{_configName}";
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
            measurementType = "LDL",
            configName = _configName,
            subjectID = GameManager.Subject
        };

        string json = FileIO.JSONStringAdd("", "info", KLib.FileIO.JSONSerializeToString(header));
        json += Environment.NewLine;

        File.WriteAllText(_dataPath, json);
    }

    private void Begin()
    {
        _localAbort = true;
        _stopMeasurement = false;

        if (!string.IsNullOrEmpty(_questionnaire.InstructionMarkdown))
        {
            HTS_Server.SendMessage(_mySceneName, "Status:Instructions");
            ShowInstructions(
                instructions: _questionnaire.InstructionMarkdown,
                fontSize: _questionnaire.InstructionFontSize);
        }
        else
        {
            StartMeasurement();
        }
    }

    private void StartMeasurement()
    {
        HTS_Server.SendMessage(_mySceneName, "Status:Questions started");
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(true);

        _qnum = 0;
        ShowQuestion();
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

    private void ShowInstructions(string instructions, int fontSize)
    {
        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = StartMeasurement;
        _instructionPanel.ShowInstructions(new Turandot.Instructions() { Text = instructions, FontSize = fontSize });
    }

    private void ShowQuestion()
    {
        _checklist.LayoutChecklist(
            _questionnaire.Questions[_qnum],
            _data.responses[_qnum].selectionNumbers, 
            _questionnaire.FontSize);

        _backButton.interactable = _qnum > 0;
        _nextButton.interactable = _data.responses[_qnum].Answered;
    }

    public void OnNextClick()
    {
        //_data.responses[_qnum].selectionValues = 

        _qnum++;
        ShowQuestion();
    }

    public void OnBackClick()
    {
        _qnum--;
        ShowQuestion();
    }

    private void OnChecklistSelectionChanged(bool anySelected)
    {
        _nextButton.interactable = anySelected;
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
                _questionnaire = FileIO.XmlDeserializeFromString<BasicMeasurementConfiguration>(data) as Questionnaire;
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
}
