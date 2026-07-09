using System;
using System.Collections;
using System.IO;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using C462.Shared;

using KLib.Signals;
using KLibU;
using KLibU.Net;

using BasicMeasurements;
using Tapping;
using KLib.Logging;

public class TappingSceneController : MonoBehaviour, IRemoteControllable
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

    private TappingConfiguration _settings = new TappingConfiguration();

    private string _dataPath;
    private string _mySceneName = "Tapping";
    private string _configName;

    private TappingPatternGenerator _patternGenerator;
    private TapStreamer _tapStreamer;

    private bool _audioEnabled = false;
    private bool _stopAudio = false;
    private bool _runEnded = false;

    private InputAction _abortAction;

    private AudioDspLog _audioDspLog = new AudioDspLog();

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
        StartCoroutine(StartNextFrame());
    }

    private IEnumerator StartNextFrame()
    {
        yield return new WaitForEndOfFrame();

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
            var fn = SharedFileLocations.GetConfigFile("Tapping", _configName);
            _settings = Files.XmlDeserialize<BasicMeasurementConfiguration>(fn) as TappingConfiguration;
            InitializeMeasurement();
            Begin();
        }
    }

    void InitializeMeasurement()
    {
        InitDataFile();
    }

    void InitDataFile()
    {
        var fileStemStart = $"{GameManager.Subject}-{_mySceneName}";
        while (true)
        {
            string fileStem = $"{fileStemStart}-Run{GameManager.GetNextRunNumber(_mySceneName):000}";
            fileStem = Path.Combine(SharedFileLocations.HtsSubjectDataFolder, fileStem);
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

        string json = Files.JSONStringAdd("", "info", Files.JSONSerializeToString(header));
        json = Files.JSONStringAdd(json, "params", Files.JSONSerializeToString(_settings));
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

            HTS_Server.SendRequest(_mySceneName, "Status:Instructions");
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

        InitializePatternGenerator();
        StartTapStreamer();

        _audioEnabled = true;
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
        if (_runEnded) return;
        
        if (_patternGenerator != null && _patternGenerator.IsComplete)
        {
            _stopAudio = true;
            _runEnded = true;
            EndRun(abort: false);
            return;
        }

        if (_stopMeasurement)
        {
            _abortAction.Disable();
            _stopMeasurement = false;
            _stopAudio = true;
            _runEnded = true;
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

    private void EndRun(bool abort)
    {
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(false);

        StopTapStreamer();

        _audioDspLog.dspEvents.Add(_patternGenerator.DspEventLog);
        KLib.Utilities.AppendToJsonFile(_dataPath, _audioDspLog.ToJsonString());

        string status = abort ? "Measurement aborted" : "Measurement finished";
        HTS_Server.SendRequest(_mySceneName, $"Finished:{status}");
        HTS_Server.SendDataFile(_mySceneName, _dataPath);

        if (_localAbort)
        {
            SceneManager.LoadScene("Home");
            return;
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

        HTS_Server.SendRequest(_mySceneName, $"Error:{error}");
        Debug.Log($"[{_mySceneName} error]: {error}{Environment.NewLine}{stackTrace}");

        if (!_isRemote)
        {
            ShowFinishPanel("The run was stopped because of an error");
        }
    }

    private void InitializePatternGenerator()
    {
        _patternGenerator = new TappingPatternGenerator();

        _patternGenerator.Initialize(
            channel: _settings.Channel.Clone(),
            minISI: _settings.MinISI,
            intervalExpression: _settings.IntervalExpression,
            numIntervals: _settings.PatternLength,
            numRepeats: _settings.NumRepeats);
    }

    private void StartTapStreamer()
    {
        _tapStreamer = new TapStreamer();
        bool initialized = _tapStreamer.Initialize();
        if (!initialized)
        {
            HandleError("Failed to initialize TapStreamer");
            return;
        }

        _tapStreamer.StartStreaming(_dataPath.Replace(".json", ".wav"));
    }

    private void StopTapStreamer()
    {
        if (_tapStreamer != null)
        {
            _tapStreamer.StopStreaming();
            _tapStreamer = null;
        }
    }

    TcpMessage IRemoteControllable.ProcessRPC(TcpMessage request)
    {
        var data = request.GetPayload<string>();
        switch (request.Command)
        {
            case "Initialize":
                _settings = Files.XmlDeserializeFromString<BasicMeasurementConfiguration>(data) as TappingConfiguration;
                InitializeMeasurement();
                return TcpMessage.Ok(Path.GetFileName(_dataPath));
            case "Begin":
                StartCoroutine(BeginNextFrame());
                return TcpMessage.Ok();
            case "Abort":
                _stopMeasurement = true;
                return TcpMessage.Ok();
            default:
                return TcpMessage.NotFound(request.Command);
        }
    }

    IEnumerator BeginNextFrame()
    {
        yield return null;
        Begin();
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_runEnded) return;

        if (_audioEnabled)
        {
            _patternGenerator.Process(data, channels, _audioDspLog.CurrentBlockNumber);

            if (_stopAudio)
            {
                Gate.RampDown(data);
                _audioEnabled = false;
            }

            if (_patternGenerator.IsComplete)
            {
                _audioEnabled = false;
            }
        }

        _audioDspLog.AddBlock();
    }
}
