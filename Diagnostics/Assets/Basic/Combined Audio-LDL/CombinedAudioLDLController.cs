using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using KLibU;
using KLibU.Net;
using KLib.Signals;

using C462.Shared;
using C462.Shared.Protocol.DTOs;

using Audiograms;

using BasicMeasurements;
using CombinedAudioLDL;

using MeasurementState = CombinedAudioLDL.MeasurementState;

public class CombinedAudioLDLController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private TextAsset _defaultInstructions;
    [SerializeField] private Text _finishText;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private GameObject _workPanel;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private QuestionBox _questionBox;
    [SerializeField] private CombinedLevelSlider _levelSlider;
    [SerializeField] private SliderWheel _sliderWheel;

    private bool _isRemote;
    private bool _localAbort = false;
    private bool _audioEnabled = false;
    private bool _stopAudio = false;

    private string _configName;
    private string _dataPath;
    private string _mySceneName = "Combined Audio-LDL";

    private string _stateFile;
    private MeasurementState _state;
    private AudioLDLData _data = new AudioLDLData();
    private CombinedAudioLDLSettings _settings = new CombinedAudioLDLSettings();

    private InputAction _abortAction;

    private SignalManager _signalManager = new SignalManager();
    private float _sliderRange;

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += HandleAbortAction;

        Application.logMessageReceived += HandleException;
    }

    public void AdvanceButtonClick()
    {
        _sliderWheel.Advance();
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
            var fn = SharedFileLocations.GetConfigFile("Combined", _configName);
            _settings = Files.XmlDeserialize<BasicMeasurementConfiguration>(fn) as CombinedAudioLDLSettings;
            InitializeMeasurement();
            Begin();
        }
    }

    void InitializeMeasurement()
    {
        _title.text = _settings.Title;

        InitDataFile();

        _stateFile = Path.Combine(SharedFileLocations.HtsSubjectFolder, $"{_mySceneName}.bin");

        CreatePlan();
        InitializeStimulusGeneration();

        var maxLevel = GetOverallMaxLevel();
        _sliderRange = maxLevel;

        _progressBar.maxValue = _state.NumConditions;
        _progressBar.value = 0;
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
            measurementType = "CombinedAudioLDL",
            configName = _configName,
            subjectID = GameManager.Subject
        };

        string json = Files.JSONStringAdd("", "info", KLibU.Files.JSONSerializeToString(header));
        json = Files.JSONStringAdd(json, "params", KLibU.Files.JSONSerializeToString(_settings));
        json += Environment.NewLine;

        File.WriteAllText(_dataPath, json);
    }

    private void Begin()
    {
        _localAbort = false;

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

            return;
        }

        StartMeasurement();
    }

    private void StartMeasurement()
    {
        _instructionPanel.gameObject.SetActive(false);
        _instructionPanel.gameObject.SetActive(false);

        if (File.Exists(_stateFile))
        {
            Debug.Log("CombinedAudioLDL: Previous state exists. Asking whether to resume");
            HTS_Server.SendRequest(_mySceneName, "Status:Asking to resume");

            _questionBox.gameObject.SetActive(true);
            _questionBox.PoseQuestion("Continue previous session?", OnQuestionResponse);
        }
        else
        {
            _instructionPanel.gameObject.SetActive(false);

            _workPanel.SetActive(true);
            _sliderWheel.Initialize(_state.NumConditions);

            HTS_Server.SendRequest(_mySceneName, "Status:Running measurement");
            DoNextStimulusAsync();
        }
    }

    private float GetOverallMaxLevel()
    {
        float leftMax = GetMaxLevel("Left");
        float rightMax = GetMaxLevel("Right");
        return Mathf.Max(leftMax, rightMax);
    }

    private float GetMaxLevel(string ear)
    {
        float earMax = float.NegativeInfinity;
        var cal = CalibrationFactory.Load(LevelUnits.dB_SPL_noLDL, SessionContext.Signal, ear);
        for (int k = 0; k < _settings.TestFrequencies.Length; k++)
        {
            float max = cal.GetReference(_settings.TestFrequencies[k]);
            earMax = Mathf.Max(earMax, max);
            //Debug.Log($"Max level for {ear} ear at {_settings.TestFrequencies[k]} Hz: {max} dB SPL");
        }
        return earMax;
    }

    private void OnQuestionResponse(bool yes)
    {
        _questionBox.gameObject.SetActive(false);

        if (yes)
        {
            Debug.Log("CombinedAudioLDL: Resuming previous");
            HTS_Server.SendRequest(_mySceneName, "Status:Resuming previous");

            _state = RestoreState();
            _progressBar.maxValue = _state.NumConditions;
            _progressBar.value = _state.NumCompleted;

            _workPanel.SetActive(true);
            _sliderWheel.Initialize(_state.NumConditions);

            HTS_Server.SendRequest(_mySceneName, "Status:Running measurement");
            DoNextStimulusAsync();
        }
        else
        {
            Debug.Log("CombinedAudioLDL: Starting new measurement");
            HTS_Server.SendRequest(_mySceneName, "Status:Starting new measurement");

            File.Delete(_stateFile);
            StartMeasurement();
        }
    }

    void HandleAbortAction(InputAction.CallbackContext context)
    {
        _abortAction.Disable();

        _workPanel.SetActive(false);
        _instructionPanel.gameObject.SetActive(false);
        _quitPanel.SetActive(true);
    }

    public void OnQuitConfirmButtonClick()
    {
        _localAbort = true;
        _abortAction.Disable();
        EndRun(abort: true);
    }

    public void OnQuitCancelButtonClick()
    {
        _quitPanel.SetActive(false);
        _workPanel.SetActive(true);
        _abortAction.Enable();
        DoNextStimulusAsync();
    }

    private void ShowInstructions(string instructions, int fontSize)
    {
        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = StartMeasurement;
        _instructionPanel.ShowInstructions(new Turandot.Instructions() { Text = instructions, FontSize = fontSize });
    }

    private void EndRun(bool abort)
    {
        _progressBar.gameObject.SetActive(false);
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(false);

        if (_state.testConditions.FindAll(test => test.completed).Count > 0)
        {
            FinishData();

            if (File.Exists(_stateFile) && _state.IsComplete)
            {
                File.Delete(_stateFile);
            }

            string status = abort ? "Measurement aborted" : "Measurement finished";

            HTS_Server.SendDataFile(_mySceneName, _dataPath);
            HTS_Server.SendDataFile(_mySceneName, SharedFileLocations.AudiogramPath);
            HTS_Server.SendDataFile(_mySceneName, SharedFileLocations.LDLPath);
            HTS_Server.SendRequest(_mySceneName, $"Finished:{status}");
        }
        else
        {
            HTS_Server.SendRequest(_mySceneName, "Finished:No data collected");
            if (File.Exists(_stateFile))
            {
                File.Delete(_stateFile);
            }
        }

        if (_localAbort)
        {
            SceneManager.LoadScene("Home");
            return;
        }

        if (!_isRemote)
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
        SceneManager.LoadScene("Home");
    }

    TcpMessage IRemoteControllable.ProcessRPC(TcpMessage request)
    {
        switch (request.Command)
        {
            case "Initialize":
                _settings = request.GetPayload<CombinedAudioLDLSettings>();
                InitializeMeasurement();
                return TcpMessage.Ok(Path.GetFileName(_dataPath));
            case "Begin":
                StartCoroutine(BeginNextFrame());
                return TcpMessage.Ok();
            case "Abort":
                EndRun(abort: true);
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

    private void InitializeStimulusGeneration()
    {
        Waveform wf = null;
        if (_settings.Bandwidth == 0)
        {
            var fm = new FM();
            fm.Carrier_Hz = 500;
            fm.Depth_Hz = 500 * _settings.ModDepth_pct / 100f;
            wf = fm;
        }
        else
        {
            wf = new Noise()
            {
                Filter = new FilterSpec()
                {
                    Shape = FilterShape.Band_pass,
                    CF = 500,
                    BW = _settings.Bandwidth,
                    BandMode = BandMode.Octaves
                }
            };
        }

        var ch = new Channel()
        {
            Modality = Modality.Audio,
            Laterality = Laterality.Diotic,
            Waveform = wf,
            Level = new Level()
            {
                Units = LevelUnits.dB_SPL_noLDL,
                Value = 75f
            },
            Gate = new Gate()
            {
                Active = true,
                Delay_ms = 0,
                Width_ms = _settings.ToneDuration,
                Period_ms = _settings.ISI_ms
            }
        };

        _signalManager.Channels.Add(ch);

        var config = AudioSettings.GetConfiguration();
        _signalManager.Initialize(config.sampleRate, config.dspBufferSize, SessionContext.Signal);
    }

    private void CreatePlan()
    {
        _state = new MeasurementState();

        for (int k = 0; k < _settings.TestFrequencies.Length; k++)
        {
            if (_settings.TestEar != TestEar.Right) _state.testConditions.Add(new TestCondition(Laterality.Left, _settings.TestFrequencies[k]));
            if (_settings.TestEar != TestEar.Left) _state.testConditions.Add(new TestCondition(Laterality.Right, _settings.TestFrequencies[k]));
        }

        _state.CreateRandomTestOrder(_settings.NumRepeats);
    }

    private void DoNextStimulusAsync()
    {
        StartCoroutine(DoNextStimulus());
    }

    private IEnumerator DoNextStimulus()
    {

#if !UNITY_EDITOR
        HardwareInterface.VolumeManager.SetMasterVolume(1, KLib.VolumeManager.VolumeUnit.Scalar);
#endif

        float maxLevel = UpdateStimulus(_state.testConditions[_state.testIndex]);
        //Debug.Log($"Max level for current stimulus: {maxLevel} dB SPL");

        yield return _sliderWheel.Advance();

        _levelSlider = _sliderWheel.GetActiveSlider();
        _levelSlider.MeasurementLocked = HandleMeasurementLocked;
        _levelSlider.ParamSetter = _signalManager.Channels[0].Level.GetParamSetter("Level");

        _levelSlider.Initialize(_settings.MinExcursion, _settings.NumReversals);

        _levelSlider.Activate(maxLevel - _sliderRange, maxLevel);
        _signalManager.Activate();
        _stopAudio = false;
        _audioEnabled = true;

        yield return null;
    }

    private float UpdateStimulus(TestCondition testCondition)
    {
        if (_settings.Bandwidth == 0)
        {
            var fm = _signalManager.Channels[0].Waveform as FM;
            fm.Carrier_Hz = testCondition.Freq_Hz;
            fm.Depth_Hz = testCondition.Freq_Hz * _settings.ModDepth_pct / 100f;
        }
        else
        {
            var noise = _signalManager.Channels[0].Waveform as Noise;
            noise.Filter.CF = testCondition.Freq_Hz;
        }
        _signalManager.Channels[0].Laterality = testCondition.ear;
        _signalManager.Channels[0].Level.Value = 0;

        var config = AudioSettings.GetConfiguration();
        _signalManager.Initialize(config.sampleRate, config.dspBufferSize, SessionContext.Signal);

        return _signalManager.Channels[0].GetMaxLevel(SessionContext.Signal);
    }

    private void HandleMeasurementLocked(SliderMeasurement measurement, float value)
    {
        switch (measurement)
        {
            case SliderMeasurement.Threshold:
                _state.testConditions[_state.testIndex].threshold = value;
                SendThreshold(_state.testConditions[_state.testIndex]);
                if (value == float.PositiveInfinity)
                {
                    _state.testConditions[_state.testIndex].LDL = float.PositiveInfinity;
                    StartCoroutine(EndSlider());
                    return;
                }
                break;
            case SliderMeasurement.LDL:
                _state.testConditions[_state.testIndex].LDL = value;
                SendLDL(_state.testConditions[_state.testIndex]);
                StartCoroutine(EndSlider());
                break;
        }
    }

    private void SendThreshold(TestCondition testCondition)
    {
        float thresholdHL = ANSI_dBHL.SPL_To_HL(testCondition.Freq_Hz, testCondition.threshold, SessionContext.Signal.Transducer);
        thresholdHL = Mathf.Round(thresholdHL / 5f) * 5f;

        AudiogramPointPayload payload = new AudiogramPointPayload()
        {
            Type = AudiogramType.Threshold,
            Ear = testCondition.ear == Laterality.Left ? AudiogramTestEar.Left : AudiogramTestEar.Right,
            Frequency_Hz = testCondition.Freq_Hz,
            Threshold_dBHL = thresholdHL,
            Threshold_dBSPL = testCondition.threshold
        };

        HTS_Server.SendRequest("AudiogramPoint", _mySceneName, payload);
    }

    private void SendLDL(TestCondition testCondition)
    {
        AudiogramPointPayload payload = new AudiogramPointPayload()
        {
            Type = AudiogramType.LDL,
            Ear = testCondition.ear == Laterality.Left ? AudiogramTestEar.Left : AudiogramTestEar.Right,
            Frequency_Hz = testCondition.Freq_Hz,
            Threshold_dBHL = testCondition.LDL - testCondition.threshold,
            Threshold_dBSPL = testCondition.threshold
        };

        HTS_Server.SendRequest("AudiogramPoint", _mySceneName, payload);
    }

    private IEnumerator EndSlider()
    {
        _signalManager.Pause();
        _stopAudio = true;

        if (_settings.LogSliderTracks)
        {
            _state.testConditions[_state.testIndex].log = _levelSlider.Log.Finish();
        }
        _state.testConditions[_state.testIndex].completed = true;
        bool finished = _state.Advance();
        Debug.Log($"Completed test {_state.testIndex} of {_state.testOrder.Count}, finished = {finished}");

        SaveState();

        yield return new WaitForSeconds(0.25f);

        _progressBar.value = _state.NumCompleted;
        HTS_Server.SendRequest(_mySceneName, $"Progress:{_state.PercentCompleted}");

        yield return new WaitForSeconds(0.25f);

        if (!finished)
        {
            DoNextStimulusAsync();
            yield break;
        }

        EndRun(abort: false);
    }

    void FinishData()
    {
        _data.testConditions = _state.testConditions;

        _data.audiogram = new AudiogramData();
        _data.audiogram.Initialize(_settings.TestFrequencies);  

        _data.LDLgram = new AudiogramData();
        _data.LDLgram.Initialize(_settings.TestFrequencies);

        foreach (TestCondition tc in _state.testConditions.FindAll(test => test.completed))
        {
            var ear = tc.ear == Laterality.Left ? AudiogramTestEar.Left : AudiogramTestEar.Right;
            float thresholdHL = ANSI_dBHL.SPL_To_HL(tc.Freq_Hz, tc.threshold, SessionContext.Signal.Transducer);
            thresholdHL = Mathf.Round(thresholdHL / 5f) * 5f;
            float thresholdSPL = ANSI_dBHL.HL_To_SPL(tc.Freq_Hz, thresholdHL, SessionContext.Signal.Transducer);
            _data.audiogram.Set(ear, tc.Freq_Hz, thresholdHL, thresholdSPL);

            float ldlSL = (tc.LDL != float.PositiveInfinity) ? tc.LDL - thresholdSPL : float.PositiveInfinity;
            _data.LDLgram.Set(ear, tc.Freq_Hz, ldlSL, tc.LDL);
        }
        _data.audiogram.Save(SharedFileLocations.AudiogramPath);
        SessionContext.SetAudiogram(SharedFileLocations.AudiogramPath);

        _data.LDLgram.Save(SharedFileLocations.LDLPath);
        SessionContext.SetLDL(SharedFileLocations.LDLPath);

       KLib.Utilities.AppendToJsonFile(_dataPath, Files.JSONSerializeToString(_data));
    }

    private void SaveState()
    {
        Files.JSONSerialize(_state, _stateFile);
    }

    private MeasurementState RestoreState()
    {
        MeasurementState s = null;

        try
        {
            s = Files.JSONDeserialize<MeasurementState>(_stateFile);
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Error deserializing LDL state: {ex.Message}");
        }

        return s;
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

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_audioEnabled)
        {
            _signalManager.Synthesize(data);

            if (_stopAudio)
            {
                Gate.RampDown(data);
                _audioEnabled = false;
            }
        }
    }

}


