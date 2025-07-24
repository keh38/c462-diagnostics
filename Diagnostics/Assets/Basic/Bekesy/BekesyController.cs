using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//using Audiograms;
using Bekesy;
using KLib;
using KLib.Signals;
using KLib.Signals.Waveforms;
using UnityEngine.EventSystems;

public class BekesyController : MonoBehaviour, IRemoteControllable
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
    [SerializeField] private TMPro.TMP_Text _prompt;
    [SerializeField] private Button _button;

    private bool _isRemote;

    private bool _stopMeasurement = false;
    private bool _localAbort = false;

    private BekesyMeasurementSettings _settings = new BekesyMeasurementSettings();

    private Data _data;
    private TrackData _currentTrack;
    private bool _trackActive;

    private bool _buttonDown = false;
    private int _direction;
    private int _lastDirection;
    private float _maxLevel;
    private float _currentLevel;
    private float _deltaTime;
    private bool _levelOverRange;
    private int _numReversals;

    private string _dataPath;
    private string _mySceneName = "Bekesy";
    private string _configName;

    private SignalManager _signalManager;
    private Action<float> _levelSetter;

    private InputAction _abortAction;
    
    Audiograms.ANSI_dBHL dBHL_table;

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += OnAbortAction;
    }

    private void Start()
    {
        HTS_Server.SetCurrentScene(_mySceneName, this);

        _title.text = "";
        dBHL_table = Audiograms.ANSI_dBHL.GetTable();

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
            var fn = FileLocations.ConfigFile("Bekesy", _configName);
            _settings = FileIO.XmlDeserialize<BasicMeasurementConfiguration>(fn) as BekesyMeasurementSettings;
            InitializeMeasurement();
            Begin();
        }
    }

    void InitializeMeasurement()
    {
        _trackActive = false;

        CreatePlan();
        InitializeStimulusGeneration();
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
            measurementType = "Audiogram",
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
        _data = new Data();
        _data.audiogram.Initialize(_settings.TestFrequencies);

        foreach (var f in _settings.TestFrequencies)
        {
            if (_settings.TestEar != Audiograms.TestEar.Right)
            {
                _data.tracks.Add(new Bekesy.TrackData(KLib.Signals.Laterality.Left, f));
            }
            if (_settings.TestEar != Audiograms.TestEar.Left)
            {
                _data.tracks.Add(new Bekesy.TrackData(KLib.Signals.Laterality.Right, f));
            }
        }
    }

    private void InitializeStimulusGeneration()
    {
        var audioConfig = AudioSettings.GetConfiguration();
        _signalManager = new SignalManager();
        _signalManager.AdapterMap = HardwareInterface.AdapterMap;

        var signalChannel = new Channel()
        {
            Name = "Signal",
            Modality = KLib.Signals.Enumerations.Modality.Audio,
            Laterality = Laterality.Diotic,
            waveform = new FM(),
            level = new Level()
            {
                Units = LevelUnits.dB_SPL
            },
            gate = new Gate()
            {
                Active = !_settings.Continuous,
                Duration_ms = _settings.ToneDuration,
                Ramp_ms = _settings.Ramp,
                Period_ms = _settings.IPI_ms
            }
        };

        _signalManager.AddChannel(signalChannel);
        _signalManager.Initialize(audioConfig.sampleRate, audioConfig.dspBufferSize);
        _deltaTime = (float)audioConfig.dspBufferSize / audioConfig.sampleRate;

        _levelSetter = signalChannel.GetParamSetter("Level");
    }

    private void Begin()
    {
        _localAbort = false;
        _stopMeasurement = false;

        if (!string.IsNullOrEmpty(_settings.InstructionMarkdown))
        {
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
        HTS_Server.SendMessage(_mySceneName, "Status:Starting measurement");

        _instructionPanel.gameObject.SetActive(false);

        _workPanel.SetActive(true);
        _progressBar.maxValue = _data.tracks.Count;
        _button.gameObject.SetActive(false);
        _prompt.gameObject.SetActive(false);

        _currentTrack = null;

        StartNextStimulusCondition();
    }

    private void StartNextStimulusCondition()
    {
        var lastTrack = _currentTrack;
        _currentTrack = _data.GetNext();
        if (_currentTrack != null)
        {
            _progressBar.value = _data.NumCompleted + 1;
            HTS_Server.SendMessage(_mySceneName, $"Progress:{_data.PercentCompleted}");

            if (lastTrack == null)
            {
                StartCoroutine(StartTrack());
            }
            else if (lastTrack.ear != _currentTrack.ear)
            {
                _instructionPanel.InstructionsFinished = ResumeFromInstructions;
                ShowInstructions("- Great!\n- Let's try the same thing with your right ear", _settings.InstructionFontSize);
            }
            else if (lastTrack.Freq_Hz != _currentTrack.Freq_Hz)
            { 
                _instructionPanel.InstructionsFinished = ResumeFromInstructions;

                ShowInstructions(
                    "- Nice work!\n" +
                    "- Let's try some more.\n" +
                    "- The pitch will be different, but your job is the same.",
                    _settings.InstructionFontSize);
            }
            else
            {
                // don't know how we could possibly get here, but just in case
                StartCoroutine(StartTrack());
            }
        }
        else
        {
            EndRun(abort: false);
        }
    }

    private void ResumeFromInstructions()
    {
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.gameObject.SetActive(true);
        StartCoroutine(StartTrack());
    }

    private IEnumerator StartTrack()
    {
        HTS_Server.SendMessage(_mySceneName, $"Status:{_currentTrack.ear} ear, {_currentTrack.Freq_Hz} Hz");

        var fm = (_signalManager["Signal"].waveform as FM);

        fm.Carrier_Hz = _currentTrack.Freq_Hz;
        fm.ModFreq_Hz = _currentTrack.Freq_Hz * _settings.ModDepth / 100f;
        fm.ModFreq_Hz = _settings.ModRate;

        _currentLevel = _settings.StartLevel + dBHL_table.HL_To_SPL(_currentTrack.Freq_Hz);
        _levelSetter(_currentLevel);

        _signalManager.Initialize();
        _signalManager.StartPaused();

        _maxLevel = _signalManager["Signal"].GetMaxLevel();
        _levelOverRange = false;
        _direction = 0;
        _lastDirection = 0;
        _numReversals = 0;

        yield return new WaitForSeconds(0.5f);
        _prompt.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);

        _prompt.gameObject.SetActive(false);
        _button.gameObject.SetActive(true);

        _signalManager.Unpause();
        _trackActive = true;
    }

    private IEnumerator EndTrack(bool overrange)
    {
        _button.gameObject.SetActive(false);
        _currentTrack.Complete();

        float threshold = _currentTrack.ComputeThreshold();
        if (overrange)
        {
            threshold = float.PositiveInfinity;
        }
        _data.audiogram.Set(
            _currentTrack.ear,
            _currentTrack.Freq_Hz,
            dBHL_table.SPL_To_HL(_currentTrack.Freq_Hz, threshold),
            threshold);

        StartNextStimulusCondition();
        yield break;
    }

    void OnAbortAction(InputAction.CallbackContext context)
    {
        _abortAction.Disable();

        _workPanel.SetActive(false);
        _instructionPanel.gameObject.SetActive(false);
        _quitPanel.SetActive(true);
    }

    public void OnButtonDown(BaseEventData data)
    {
        _buttonDown = true;
        _direction = -1;
    }

    public void OnButtonUp(BaseEventData data)
    {
        _buttonDown = false;
        _direction = 1;
    }

    void Update()
    {
        if (_stopMeasurement)
        {
            _abortAction.Disable();
            _stopMeasurement = false;
            EndRun(abort: true);
        }
        else if (_trackActive)
        {
            if (_numReversals >= _settings.NumReversals || _levelOverRange)
            {
                _trackActive = false;
                _signalManager.Pause();
                StartCoroutine(EndTrack(_levelOverRange));
            }
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
        _workPanel.gameObject.SetActive(false);
        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = StartMeasurement;
        _instructionPanel.ShowInstructions(new Turandot.Instructions() { Text = instructions, FontSize = fontSize });
    }

    private void EndRun(bool abort)
    {
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(false);

        File.AppendAllText(_dataPath, FileIO.JSONSerializeToString(_data));
        //_data.audiogramData.Save();

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
                _settings = FileIO.XmlDeserializeFromString<BasicMeasurementConfiguration>(data) as BekesyMeasurementSettings;
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

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_trackActive && !_levelOverRange)
        {
            float delta = _direction * _settings.AttenuationRate * _deltaTime;
            _currentLevel += delta;

            bool reversal = _direction != _lastDirection && _lastDirection != 0;
            if (reversal)
            {
                _numReversals++;
            }
            _lastDirection = _direction;

            _currentTrack.log.Add(AudioSettings.dspTime, _currentLevel, _direction, reversal ? 1 : 0);

            if (_currentLevel > _maxLevel)
            {
                _levelOverRange = true;
            }
            else
            {
                _levelSetter(_currentLevel);

                _signalManager.Synthesize(data);
            }
        }
    }
}
