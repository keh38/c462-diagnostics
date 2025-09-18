using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using KLib;
using KLib.Signals.Waveforms;
using KLib.Signals;
using LDL;

using BasicMeasurements;

public class LDLHapticsController : MonoBehaviour, IRemoteControllable
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

    private LDLSliderPanel _sliderPanel;

    private bool _isRemote;

    private bool _localAbort = false;

    private LDLMeasurementSettings _settings = new LDLMeasurementSettings();

    private string _configName;
    private string _dataPath;
    private string _mySceneName = "LDL";

    private string _stateFile;
    private MeasurementState _state;

    private List<TestCondition> _curGroup = new List<TestCondition>();
    private LoudnessDiscomfortData _data = new LoudnessDiscomfortData();

    private InputAction _abortAction;

    private bool _doSimulation = false;

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += OnAbortAction;

        _sliderPanel = _workPanel.GetComponent<LDLSliderPanel>();
        _sliderPanel.LockInPressed += OnLockIn;
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
            var fn = FileLocations.ConfigFile("LDL", _configName);
            _settings = FileIO.XmlDeserialize<BasicMeasurementConfiguration>(fn) as LDLMeasurementSettings;
            InitializeMeasurement();
            Begin();
        }
    }

    void InitializeMeasurement()
    {
        _title.text = _settings.Title;

        InitDataFile();

        _stateFile = Path.Combine(FileLocations.SubjectFolder, $"{_mySceneName}.bin");

        // Need to delete the existing LDL, otherwise it will be loaded and used to constrain the 
        // max output level, making it impossible ever to exceed the original LDL.
        if (_settings.Merge && File.Exists(FileLocations.LDLPath))
        {
            _data.LDLgram = Audiograms.AudiogramData.Load(FileLocations.LDLPath);
            File.Delete(FileLocations.LDLPath);
        }

        CreatePlan();

        _progressBar.maxValue = _state.NumConditions;
        _progressBar.value = 0;

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
            measurementType = "LDL",
            configName = _configName,
            subjectID = GameManager.Subject
        };

        string json = FileIO.JSONStringAdd("", "info", KLib.FileIO.JSONSerializeToString(header));
        json = KLib.FileIO.JSONStringAdd(json, "params", KLib.FileIO.JSONSerializeToString(_settings));
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
        _instructionPanel.gameObject.SetActive(false);

        if (File.Exists(_stateFile))
        {
            Debug.Log("LDL: Previous state exists. Asking whether to resume");
            HTS_Server.SendMessage(_mySceneName, "Status:Asking to resume");

            _questionBox.gameObject.SetActive(true);
            _questionBox.PoseQuestion("Continue previous session?", OnQuestionResponse);
        }
        else
        {
            _instructionPanel.gameObject.SetActive(false);

            _workPanel.SetActive(true);
            InitializeSliderPanel();

            HTS_Server.SendMessage(_mySceneName, "Status:Running measurement");
            DoNextGroup();
        }
    }

    private void OnQuestionResponse(bool yes)
    {
        _questionBox.gameObject.SetActive(false);

        if (yes)
        {
            Debug.Log("LDL: Resuming previous");
            HTS_Server.SendMessage(_mySceneName, "Status:Resuming previous");

            _state = RestoreState();
            _progressBar.maxValue = _state.NumConditions;
            _progressBar.value = _state.NumCompleted;

            _workPanel.SetActive(true);
            InitializeSliderPanel();

            HTS_Server.SendMessage(_mySceneName, "Status:Running measurement");
            DoNextGroup();
        }
        else
        {
            Debug.Log("LDL: Starting new measurement");
            HTS_Server.SendMessage(_mySceneName, "Status:Staring new measurement");

            File.Delete(_stateFile);
            StartMeasurement();
        }
    }

    void OnAbortAction(InputAction.CallbackContext context)
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
        DoNextGroup();
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

        FinishData();

        if (File.Exists(_stateFile) && _state.IsComplete)
        {
            File.Delete(_stateFile);
        }

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
        SceneManager.LoadScene("Home");
    }

    void IRemoteControllable.ProcessRPC(string command, string data)
    {
        switch (command)
        {
            case "Initialize":
                _settings = FileIO.XmlDeserializeFromString<BasicMeasurementConfiguration>(data) as LDLMeasurementSettings;
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
                EndRun(abort: true);
                break;
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    private void InitializeSliderPanel()
    {
        var ch = new Channel()
        {
            Modality = KLib.Signals.Enumerations.Modality.Audio,
            Laterality = Laterality.Diotic,
            waveform = new FM()
            {
                Carrier_Hz = 500,
                Depth_Hz = 500 * _settings.ModDepth_pct / 100f,
                ModFreq_Hz = 5,
                Phase_cycles = 0
            },
            level = new Level()
            {
                Units = _settings.LevelUnits == LevelUnits.dB_SPL ? LevelUnits.dB_SPL_noLDL : _settings.LevelUnits,
                Value = 75f
            },
            gate = new Gate()
            {
                Active = true,
                Delay_ms = 0,
                Duration_ms = _settings.ToneDuration,
                Period_ms = _settings.ISI_ms
            }
        };

        _sliderPanel.Initialize(_settings, ch);
    }

    private void CreatePlan()
    {
        _state = new MeasurementState();

        if (_settings.LevelUnits == LevelUnits.dB_SL)
        {
            Audiograms.AudiogramData audiogram = Audiograms.AudiogramData.Load();
            Audiograms.Audiogram agramLeft = audiogram.Get(Audiograms.Ear.Left);
            Audiograms.Audiogram agramRight = audiogram.Get(Audiograms.Ear.Right);


            List<float> agramFreqs = new List<float>(audiogram.Get_Frequency_Hz());

            for (int k = 0; k < _settings.TestFrequencies.Length; k++)
            {
                int ifreq = agramFreqs.FindIndex(o => o == _settings.TestFrequencies[k]);

                if (!float.IsNaN(agramLeft.Threshold_dBSPL[ifreq]) && !float.IsInfinity(agramLeft.Threshold_dBSPL[ifreq]) && _settings.TestEar != Audiograms.TestEar.Right)
                {
                    _state.testConditions.Add(new TestCondition(Laterality.Left, _settings.TestFrequencies[k]));
                }
                if (!float.IsNaN(agramRight.Threshold_dBSPL[ifreq]) && !float.IsInfinity(agramRight.Threshold_dBSPL[ifreq]) && _settings.TestEar != Audiograms.TestEar.Left)
                {
                    _state.testConditions.Add(new TestCondition(Laterality.Right, _settings.TestFrequencies[k]));
                }
            }
        }
        else
        {
            for (int k = 0; k < _settings.TestFrequencies.Length; k++)
            {
                if (_settings.TestEar != Audiograms.TestEar.Right) _state.testConditions.Add(new TestCondition(Laterality.Left, _settings.TestFrequencies[k]));
                if (_settings.TestEar != Audiograms.TestEar.Left) _state.testConditions.Add(new TestCondition(Laterality.Right, _settings.TestFrequencies[k]));
            }
        }

        _state.CreateRandomTestOrder(_settings.NumRepeats);
    }

    private void DoNextGroup()
    {
        StartCoroutine(NextGroup());
    }

    private IEnumerator NextGroup()
    {
        _sliderPanel.HideLockInButton();

#if !UNITY_EDITOR
        HardwareInterface.VolumeManager.SetMasterVolume(1, VolumeManager.VolumeUnit.Scalar);
#endif

        if (_curGroup.Count > 0 && !_doSimulation)
        {
            yield return StartCoroutine(_sliderPanel.ShuffleSliderPositions());
        }

        _progressBar.gameObject.SetActive(false);
        _curGroup.Clear();

        int numToDo = Mathf.Min(_state.testOrder.Count, _sliderPanel.NumSliders);
        for (int k = 0; k < numToDo; k++)
        {
            int index = _state.testOrder[k];
            _curGroup.Add(_state.testConditions[index]);
        }

        _sliderPanel.ResetSliders(_curGroup);

        if (_doSimulation)
        {
            DoSimulation();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void DoSimulation()
    {
        _sliderPanel.SimulateMove();
        OnLockIn();
    }

    public void OnLockIn()
    {
        StartCoroutine(LockIn());
    }

    private IEnumerator LockIn()
    {
        _progressBar.gameObject.SetActive(true);

        var sliderSettings = _sliderPanel.GetSliderSettings();
        for (int k = 0; k < _curGroup.Count; k++)
        {
            _data.sliderSettings.Add(sliderSettings[k]);
            _curGroup[k].discomfortLevel.Add(sliderSettings[k].isMaxed ? float.NaN : sliderSettings[k].end);
            _state.testOrder.RemoveAt(0);
        }
        SaveState();

        yield return new WaitForSeconds(0.25f);

        _progressBar.value = _state.NumCompleted;
        HTS_Server.SendMessage(_mySceneName, $"Progress:{_state.PercentCompleted}");
        _sliderPanel.HideLockInButton();

        yield return new WaitForSeconds(0.25f);

        if (_state.testOrder.Count > 0)
        {
            DoNextGroup();
        }
        else
        {
            EndRun(abort: false);
        }
    }

    void FinishData()
    {
        Audiograms.AudiogramData audiograms = Audiograms.AudiogramData.Load();

        // 1. Create "LDL Audiogram"
        if (_settings.Merge && _data.LDLgram != null)
        {
            _data.LDLgram.Append(_settings.TestFrequencies);
        }
        else
        {
            _data.LDLgram = new Audiograms.AudiogramData();
            if (_settings.LevelUnits == LevelUnits.dB_SL)
                _data.LDLgram.Initialize(audiograms.Get_Frequency_Hz());
            else
                _data.LDLgram.Initialize(_settings.TestFrequencies);
        }

        foreach (TestCondition tc in _state.testConditions)
        {
            float sum = 0;
            float n = 0;
            foreach (float level in tc.discomfortLevel)
            {
                if (!float.IsNaN(level))
                {
                    sum += level;
                    ++n;
                }
            }

            Audiograms.Ear ear = tc.ear == Laterality.Left ? Audiograms.Ear.Left : Audiograms.Ear.Right;
            if (_settings.LevelUnits == LevelUnits.dB_SL)
            {
                float thresh_SPL = audiograms.Get(ear).GetThreshold(tc.Freq_Hz);

                float LDL_SL = (n > 1) ? (sum / n) : float.NaN;
                _data.LDLgram.Set(ear, tc.Freq_Hz, LDL_SL, LDL_SL + thresh_SPL);
            }
            else
            {
                float LDL_SPL = (n > 1) ? (sum / n) : float.NaN;
                _data.LDLgram.Set(ear, tc.Freq_Hz, float.NaN, LDL_SPL);
            }
        }
        _data.LDLgram.Save(FileLocations.LDLPath);
        File.AppendAllText(_dataPath, FileIO.JSONSerializeToString(_data));
    }

    private void SaveState()
    {
        FileIO.CreateBinarySerialization(_stateFile);
        FileIO.SerializeToBinary(_state);
        FileIO.CloseBinarySerialization();
    }

    private MeasurementState RestoreState()
    {
        MeasurementState s = null;

        FileIO.OpenBinarySerialization(_stateFile);
        try
        {
            s = FileIO.DeserializeFromBinary<MeasurementState>();
        }
        catch (System.Exception ex)
        {
            Debug.Log($"Error deserializing LDL state: {ex.Message}");
        }
        FileIO.CloseBinarySerialization();

        return s;
    }
}
