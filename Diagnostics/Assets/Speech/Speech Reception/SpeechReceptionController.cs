using Audiograms;
using BasicMeasurements;
using DigitsTest;
using KLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using SpeechReception;
using JimmysUnityUtilities;
using System.Runtime.Serialization;
using KLib.Signals;
using NUnit.Framework.Internal;
using Project;
using TMPro.EditorUtilities;
using Digits;

public class SpeechReceptionController : MonoBehaviour, IRemoteControllable
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

    private bool _isRemote;

    private bool _stopMeasurement = false;
    private bool _localAbort = false;

    private SpeechTest _settings = null;

    private string _dataPath;
    private string _mySceneName = "SpeechReception";
    private string _configName;

    private InputAction _abortAction;

    private TestPlan _plan;

    public AudioSource audioPlay;
    public AudioSource audioRecord;
    public AudioSource audioAlert;

    public AudioClip recordStartClip;
    public AudioClip recordEndClip;

    public SpeechMasker masker;

    public ClosedSetController closedSetController;
    public MatrixTestController _matrixTestController;

    private SpeechReception.ListDescription _srItems;
    private ListProperties _srList;
    private SpeechReception.Data _srData;
    private Data.Response _tentativeResponse;
    private int _qnum = -1;
    private int _responseAttempt;
    private bool _responseAccepted;

    private enum RecordingState { Waiting, Start, Recording, TimedOut, Stop, StopAndRedo, StopAndContinue, Validating };
    private RecordingState _recordingState;

    private Color _buttonColor;

    private static float sMaxRecordTime_sec = 10;

    private bool _reviewResponses = false;
    private static readonly int maxNumRecordAttempts = 3;

    private float _volumeAtten;
    private VolumeManager _volumeManager;

    private int _numListsCompleted;

    private string _testXmlFile;

    private bool _recordButtonPressed;
    private bool _stopButtonPressed;
    private bool _rerecordButtonPressed;
    private bool _itsGoodButtonPressed;

    private int _numPracticeLists;
    private int _numSinceLastBreak;

    private ClosedSetData _srClosedSetData = new ClosedSetData();
    private MatrixTestData _srMatrixTestData = new MatrixTestData();

    private TestLog _log = new TestLog();

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += OnAbortAction;
        Application.logMessageReceived += HandleException;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleException;
    }

    private void Start()
    {
        HTS_Server.SetCurrentScene(_mySceneName, this);

        _title.text = "";
        _volumeManager = new VolumeManager();

#if HACKING
        Application.targetFrameRate = 60;
        GameManager.SetSubject("Scratch/_shit");
        _configName = "QuickSIN_Test1";
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
            var fn = FileLocations.ConfigFile("SpeechTest", _configName);
            _settings = FileIO.XmlDeserialize<SpeechTest>(fn);
            InitializeMeasurement();
            Begin();
        }
    }

    void InitializeMeasurement()
    {
        CreatePlan();
        HTS_Server.SendMessage(_mySceneName, $"File:{Path.GetFileName(_dataPath)}");
    }

    void CreateDataFileName()
    {
        var fileStemStart = $"{GameManager.Subject}-Speech";
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
    }

    private void CreatePlan()
    {
        var customizations = UserCustomizations.Initialize(FileLocations.SubjectCustomSpeechPath, GameManager.Subject);
        _plan = new TestPlan()
        {
            name = _settings.TestName,
            testType = _settings.TestType,
            source = _settings.TestSource
        };

       if (_settings.TestEars.Count == 0)
        {
            _settings.TestEars = new List<SpeechReception.TestEar>() { SpeechReception.TestEar.SubjectDefault };
        }

        if (_settings.EarOrder == "Random")
        {
            foreach (int k in KMath.Permute(_settings.TestEars.Count))
            {
                var t = _settings.Clone();
                t.Initialize(_settings.TestEars[k], customizations.Get(_settings.TestSource));
                _plan.tests.Add(t);
            }
        }
        else
        {
            foreach (var e in _settings.TestEars)
            {
                var t = _settings.Clone();
                t.Initialize(e, customizations.Get(_settings.TestSource));
                _plan.tests.Add(t);
            }
        }
    }

    private void Begin()
    {
        _localAbort = false;
        _stopMeasurement = false;

        _reviewResponses = _settings.ReviewResponses;
        _numListsCompleted = 0;
        _numSinceLastBreak = 0;

        StartCoroutine(StartNextList());
    }

    private void StartMeasurement()
    {
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(true);
    }

    private float SetLevel(float level, SpeechReception.LevelUnits units, SpeechReception.TestEar testEar)
    {
        float dB_wavfile_fullscale = float.NaN;

        float ref_dB = _settings.GetReference(GameManager.Transducer, units);

        dB_wavfile_fullscale = ref_dB;
        Debug.Log("dB_wavfile_fullscale" + " = " + dB_wavfile_fullscale);

        float atten = level - dB_wavfile_fullscale;
        Debug.Log("atten" + " = " + atten);
        atten = Math.Min(0f, atten);

        audioPlay.volume = Mathf.Pow(10, atten / 20);
        audioAlert.volume = Mathf.Pow(10, (atten - 10) / 20);
        //audioPlay.panStereo = laterality.ToBalance();

        audioPlay.volume = Mathf.Pow(10, atten / 20);
        audioAlert.volume = Mathf.Pow(10, (atten - 10) / 20);
        atten = 0;

        _volumeManager.SetMasterVolume(atten, VolumeManager.VolumeUnit.Decibel);

        return atten;
    }

    public IEnumerator StartNextList()
    {
        yield return null;

        _progressBar.gameObject.SetActive(true);
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(true);

        // Pop next test off the stack
        _srList = _plan.GetNextList();

        if (_settings.TestType == TestType.ClosedSet)
        {
//            closedSetController.Initialize(_srList.closedSet, _srList.GetClosedSetResponses());
        }
        else if (_settings.TestType == TestType.Matrix)
        {
            //_matrixTestController.Initialize();
            //_srList.matrixTest.Initialize();
        }

        _srList.SetSequence();

        _volumeAtten = SetLevel(_srList.Level, _srList.Units, _srList.TestEar);

        _log.Clear();
        _log.Add($"List started: {_srList.Title}");
        Debug.Log($"List started: {_srList.Title}");
        HTS_Server.SendMessage(_mySceneName, $"Status: Starting list '{_srList.Title}'");

        _settings.Lists.RemoveAt(0);

        // Initialize summary data object
        if (_settings.TestType == TestType.ClosedSet)
        {
            _srClosedSetData = new ClosedSetData(
                _numListsCompleted < _settings.NumPracticeLists,
                _srList.Sequence.NumBlocks,
                _srList.Sequence.ItemsPerBlock,
                _srList.ClosedSet.PerformanceCriteria);

            _srClosedSetData.date = DateTime.Now.ToString();
            _srClosedSetData.test = _settings.TestType + "-" + _srList.Title;
            _srClosedSetData.testEar = _srList.TestEar;
            //_srClosedSetData.runNumber = SubjectManager.Instance.PeekDiagnosticsRun();
        }
        else if (_settings.TestType == TestType.Matrix)
        {
            _srMatrixTestData = new MatrixTestData(_srList.MatrixTest.StimLevel, _srList.MatrixTest.MaskerLevel, _srList.MatrixTest.Mode);

            _srMatrixTestData.date = DateTime.Now.ToString();
            _srMatrixTestData.test = _settings.TestType + "-" + _srList.Title;
            _srMatrixTestData.testEar = _srList.TestEar;
            //_srMatrixTestData.runNumber = SubjectManager.Instance.PeekDiagnosticsRun();
        }
        else
        {
            _srData = new SpeechReception.Data(_numListsCompleted < _settings.NumPracticeLists);
            _srData.date = DateTime.Now.ToString();
            _srData.test = _settings.TestType + "-" + _srList.Title;
            _srData.Fs = 22050;
            //_srData.runNumber = SubjectManager.Instance.PeekDiagnosticsRun();
            _srData.testEar = _srList.TestEar;
            //_dataPath = DiagnosticsManager.Instance.StartTest(_srData, "Speech", _srData.test);
        }

        _qnum = 0;
        if (_srList.UseMasker)
        {
            float maskerLevel = _srList.Level - _srList.sentences[_qnum].SNR;
            if (_settings.TestType == TestType.Matrix) maskerLevel = _srList.MatrixTest.MaskerLevel;
            //yield return StartCoroutine(masker.Initialize(
            //    _srList.masker,
            //    maskerLevel,
            //    GameManager.Transducer,
            //    _srList.Units,
            //    _srList.laterality));
        }

        // Display message, if appropriate
        //var instructionsPath = _settings.Instructions == null ? null : _settings.Instructions.Find(_numListsCompleted);

        var instructions = GetInstructions(_plan.currentListIndex);
        if (!string.IsNullOrEmpty(instructions))
        {
            ShowInstructions(instructions);
        }
        else if (_numListsCompleted > 0)
        {
            //commonUI.FormatHelpBox("Great!\nLet's try some more.");
            //commonUI.ShowNextButton("Press Next to continue", StartNextSentence);
        }
        else
        {
            //StartNextSentence();
        }
    }

    public string GetInstructions(int listNum)
    {
        string value = null;

        var f = _settings.Instructions.Find(o => o.Before == listNum);
        if (f != null)
        {
            var instructionsPath = Path.Combine(
                FileLocations.LocalResourceFolder("Config Files"),
                $"Instructions.{f.Name}.md");
            value = File.ReadAllText(instructionsPath);
        }

        return value;
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

    private void ShowInstructions(string instructions)
    {
        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = StartMeasurement;
        _instructionPanel.ShowInstructions(
            new Turandot.Instructions() 
            { 
                Text = instructions, 
                FontSize = _settings.InstructionFontSize,
                VerticalAlignment = Turandot.Instructions.VerticalTextAlignment.Middle
            });
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

    void IRemoteControllable.ProcessRPC(string command, string data)
    {
        switch (command)
        {
            case "Initialize":
                _settings = FileIO.XmlDeserializeFromString<SpeechTest>(data);
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
