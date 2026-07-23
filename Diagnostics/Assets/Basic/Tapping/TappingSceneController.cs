using BasicMeasurements;
using C462.Shared;
using CombinedAudioLDL;
using KLib.Expressions;
using KLib.Logging;
using KLib.Signals;
using KLibU;
using KLibU.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tapping;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private TMPro.TMP_Text _trialPrompt;

    private bool _isRemote;

    private bool _stopMeasurement = false;
    private bool _localAbort = false;

    private TappingConfiguration _settings = new TappingConfiguration();

    private string _dataPath;
    private string _trialDataPath;
    private string _mySceneName = "Tapping";
    private string _configName;

    private TappingPatternGenerator _pacer;
    private TappingPatternGenerator _distractor;
    private TapStreamer _tapStreamer;

    private bool _audioEnabled = false;
    private bool _stopAudio = false;
    private bool _runEnded = false;
    private bool _endRunStarted = false;
    private bool _trialEnded = false;

    private InputAction _abortAction;

    private TappingTrialList _trialList = new TappingTrialList();
    private int _currentTrialIndex = -1;

    private TappingTrialList _data = new TappingTrialList();

    private AudioDspLog _audioDspLog = new AudioDspLog();

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += HandleAbortAction;
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
        try
        {
            _dataPath = null;
            CreatePlan();
            InitDataFile();
        }
        catch (Exception ex)
        {
            Debug.Log($"[TAPPING] Failed to initialize measurement: {ex.Message}");
            _dataPath = $"error: {ex.Message}";
        }
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
        if (string.IsNullOrEmpty(_settings.TrialListFile))
        {
            throw new ArgumentException("Trial list file name is not specified in the configuration.");
        }

        string trialListPath = Path.Combine(SharedFileLocations.HtsConfigFolder, $"Tapping.{_settings.TrialListFile}.json");
        if (!File.Exists(trialListPath))
        {
            throw new FileNotFoundException($"Trial list file not found: {trialListPath}");
        }
        _trialList = Files.JSONDeserialize<TappingTrialList>(trialListPath);

        if (_trialList.Trials == null || _trialList.Trials.Count == 0)
        {
            throw new InvalidOperationException("Trial list is empty.");
        }

        _currentTrialIndex = -1;
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
        _trialPrompt.text = "";
        _workPanel.SetActive(true);

        InitializePatternGeneration();
        Advance();
    }

    private void Advance()
    {
        _currentTrialIndex++;
        if (_currentTrialIndex >= _trialList.Trials.Count)
        {
            EndRun(abort: false);
            return;
        }

        _progressBar.value = (float)(_currentTrialIndex + 1) / _trialList.Trials.Count;
        _trialDataPath = _dataPath.Replace(".json", $"-Trial{_currentTrialIndex + 1:000}.json");
        
        var trial = _trialList.Trials[_currentTrialIndex];
        StartCoroutine(StartNextTrial(trial));
    }

    private IEnumerator StartNextTrial(TappingTrial tappingTrial)
    {
        HTS_Server.SendRequest(_mySceneName, $"Status:{tappingTrial.Label}");

        yield return StartCoroutine(ShowPrompts(tappingTrial.Pacer, tappingTrial.ResponseInstructions));

        StartTapStreamer();

        InitializeNextPattern(tappingTrial);
        _audioEnabled = true;
        _trialEnded = false;
        yield break;
    }

    private IEnumerator ShowPrompts(PacerStimulus pacerSource, ResponseInstructions responseInstructions)
    {
        float delay = 1;

        string pacerName = pacerSource == PacerStimulus.A ? _settings.StimulusA.Name : _settings.StimulusB.Name;
        _trialPrompt.text = $"Pay attention to the {pacerName.ToLower()} stimulus";
        yield return new WaitForSeconds(2);

        _trialPrompt.text = responseInstructions == ResponseInstructions.AllElements ?
            "Tap along with all elements of the stimulus"
            : "Tap to the downbeat of the stimulus";
        yield return new WaitForSeconds(2);

        _trialPrompt.text = "Ready...";
        yield return new WaitForSeconds(delay);
        _trialPrompt.text = "Set...";
        yield return new WaitForSeconds(delay);
        _trialPrompt.text = "Go!";
        yield return new WaitForSeconds(delay);
        _trialPrompt.text = "";
    }

    private IEnumerator EndTrial()
    {
        yield return null;

        StopTapStreamer();

        SaveTrialData();
        _data.Trials.Add(_trialList.Trials[_currentTrialIndex]);

        Advance();
    }

    private void SaveTrialData()
    {
        var trialData = new TappingTrialData()
        {
            trial = _trialList.Trials[_currentTrialIndex],
            pacerLog = _pacer.DspEventLog
        };

        Files.JSONSerialize(trialData, _trialDataPath); 
    }

    void HandleAbortAction(InputAction.CallbackContext context)
    {
        _abortAction.Disable();

        _stopAudio = true;
        StopTapStreamer();

        _workPanel.SetActive(false);
        _instructionPanel.gameObject.SetActive(false);
        _quitPanel.SetActive(true);
    }

    void Update()
    {
        if (_runEnded) return;

        if (!_trialEnded && _pacer != null && _pacer.IsComplete)
        {
            _trialEnded = true;
            //_stopAudio = true;
            //_runEnded = true;
            StartCoroutine(EndTrial());
            return;
        }

        if (_stopMeasurement)
        {
            _abortAction.Disable();
            StopTapStreamer(); // just to be sure (it's idempotent)
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

        // restart the current trial
        var trial = _trialList.Trials[_currentTrialIndex];
        StartCoroutine(StartNextTrial(trial));
    }

    private void ShowInstructions(string instructions, int fontSize)
    {
        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = StartMeasurement;
        _instructionPanel.ShowInstructions(new Turandot.Instructions() { Text = instructions, FontSize = fontSize });
    }

    private void EndRun(bool abort)
    {
        if (_endRunStarted) return;
        _endRunStarted = true;

        _runEnded = true;

        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(false);

        //_audioDspLog.dspEvents.Add(_pacer.DspEventLog);
        KLib.Utilities.AppendToJsonFile(_dataPath, Files.JSONStringAdd("", "trials", KLibU.Files.JSONSerializeToString(_data)));
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

    private void InitializePatternGeneration()
    {
        var audioConfig = AudioSettings.GetConfiguration();

        BindProperties(_settings.StimulusA, _settings.PropertyBindings);
        BindProperties(_settings.StimulusB, _settings.PropertyBindings);

        _pacer = new TappingPatternGenerator(audioConfig.sampleRate);
    }

    private void InitializeNextPattern(TappingTrial tappingTrial)
    {
        _pacer.Initialize(
            channel: _settings.StimulusA,
            intervals: tappingTrial.PacerIntervals,
            parameterProfiles: tappingTrial.ParameterProfiles,
            isPacer: true);
    }

    private void BindProperties(Channel channel, List<PropertyBinding> bindings)
    {
        foreach (var binding in bindings.FindAll(b => b.Item.StartsWith($"{channel.Name}.")))
        {
            var value = Expressions.EvaluateToFloatScalar(binding.Expression);
            if (float.IsNaN(value))
                throw new ApplicationException($"Expression for {binding.Item} did not evaluate to a number.");

            var error = channel.SetParameter(binding.Item.Replace($"{channel.Name}.", ""), value);
            if (!string.IsNullOrEmpty(error))
                throw new ApplicationException($"error setting {binding.Item}: {error}");
        }
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

        _tapStreamer.StartStreaming(_trialDataPath.Replace(".json", ".wav"));
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
        switch (request.Command)
        {
            case "Initialize":
                _settings = request.GetPayload<TappingConfiguration>();
                InitializeMeasurement();
                if (_dataPath != null && _dataPath.StartsWith("error"))
                {  
                    return TcpMessage.Ok(_dataPath);
                }
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

        try
        {
            if (_audioEnabled)
            {
                _pacer.Process(data, channels, _audioDspLog.CurrentBlockNumber);

                if (_stopAudio)
                {
                    Gate.RampDown(data);
                    _audioEnabled = false;
                }

                if (_pacer.IsComplete)
                {
                    _audioEnabled = false;
                }
            }

            _audioDspLog.AddBlock();
        }
        catch (Exception ex)
        {
            _runEnded = true;
            HandleError($"Audio processing error: {ex.Message}", ex.StackTrace);
        }
    }
}
