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

public class LDLController : MonoBehaviour, IRemoteControllable
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

    private bool _stopMeasurement = false;
    private bool _localAbort = false;

    private LDLMeasurementSettings _settings = new LDLMeasurementSettings();

    private string _configName;
    private string _dataPath;
    private string _mySceneName = "LDL";

    private string _stateFile;
    private MeasurementState _state;

    private List<int> _curGroup = new List<int>();
    private LoudnessDiscomfortData _data = new LoudnessDiscomfortData();

    private InputAction _abortAction;

    private bool _doSimulation = false;

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += OnAbortAction;

        _sliderPanel = _workPanel.GetComponent<LDLSliderPanel>();
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
        InitDataFile();

        _stateFile = Path.Combine(FileLocations.SubjectFolder, $"{_mySceneName}.bin");

        //AudiogramData extantData = AudiogramData.Load();
        //if (_settings.Merge && extantData != null)
        //{
        //    _data.audiogramData = extantData;
        //    _data.audiogramData.Append(_settings.TestFrequencies);
        //}
        //else
        //{
        //    _data.audiogramData.Initialize(_settings.TestFrequencies);
        //}

        CreatePlan();

        _progressBar.maxValue = _state.testConditions.Count;
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
        _localAbort = true;
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
        _instructionPanel.gameObject.SetActive(false);

        if (File.Exists(_stateFile))
        {
            _questionBox.gameObject.SetActive(true);
            _questionBox.PoseQuestion("Continue previous session?", OnQuestionResponse);
        }
        else
        {
            //_sceneState = SceneState.Testing;

            _instructionPanel.gameObject.SetActive(false);

            _workPanel.SetActive(true);
            InitializeSliderPanel();

            DoNextGroup();
        }
    }

    private void OnQuestionResponse(bool yes)
    {
        _questionBox.gameObject.SetActive(false);

        if (yes)
        {
            _state = RestoreState();
            _progressBar.maxValue = _state.NumConditions;
            _progressBar.value = _state.NumCompleted;
            DoNextGroup();
        }
        else
        {
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
                _stopMeasurement = true;
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
                Depth_Hz = 500 * _settings.ModDepth_pct,
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

        for (int k = 0; k < _settings.NumRepeats; k++)
        {
            _state.testOrder.AddRange(KMath.Permute(_state.testConditions.Count));
        }
    }

    private void DoNextGroup()
    {
        _progressBar.gameObject.SetActive(false);
        StartCoroutine(NextGroup());
    }

    private IEnumerator NextGroup()
    {
        _sliderPanel.HideLockInButton();

        HardwareInterface.VolumeManager.SetMasterVolume(1, VolumeManager.VolumeUnit.Scalar);

        yield return StartCoroutine(_sliderPanel.ShuffleSliderPositions());
        yield break;

        if (_curGroup.Count > 0 && !_doSimulation)
        {
            yield return StartCoroutine(_sliderPanel.ShuffleSliderPositions());
        }

        _curGroup.Clear();

        int numToDo = Mathf.Min(_state.testOrder.Count, _sliderPanel.NumSliders);
        for (int k = 0; k < numToDo; k++)
        {
            int index = _state.testOrder[k];
            _curGroup.Add(index);

            SetOneSlider(k, _state.testConditions[index]);
        }

        for (int k = numToDo; k < _sliderPanel.NumSliders; k++)
        {
            _sliderPanel.HideSlider(k);
        }

        _sliderPanel.ResetFirstMove();
        //commonUI.ShowPrompt("Move sliders until sound is uncomfortable");

        if (_doSimulation)
        {
            Simulation();
        }
    }

    private void Simulation()
    {
        for (int k = 0; k < _sliderPanel.NumSliders; k++)
        {
            _sliderPanel.SimulateMove(k);
        }

        OnLockIn();
    }

    private void SetOneSlider(int sliderNum, TestCondition test)
    {
        SliderSettings s = new SliderSettings();
        s.var = "Level";
        s.ear = test.ear;
        s.Freq_Hz = test.Freq_Hz;

        if (test.discomfortLevel.Count == 0 || float.IsNaN(test.discomfortLevel[test.discomfortLevel.Count - 1]))
        {
            s.min = _settings.MinLevel;
            s.max = float.PositiveInfinity;
            s.start = s.min + UnityEngine.Random.Range(0f, 15f);
        }
        else
        {
            float lastLDL = test.discomfortLevel[test.discomfortLevel.Count - 1];
            s.min = lastLDL - UnityEngine.Random.Range(30f, 60f);
            s.max = lastLDL + UnityEngine.Random.Range(10f, 40f);
            s.start = s.min + UnityEngine.Random.Range(0f, 10f);
        }

        //(_sliderPanel[sliderNum].Channel.waveform as FM).Carrier_Hz = s.Freq_Hz;
        //_sliderPanel[sliderNum].Channel.Laterality = s.ear;
        //s.max = Mathf.Min(s.max, _sliderPanel[sliderNum].Channel.GetMaxLevel());

        //_sliderPanel.sliders[sliderNum].ApplySettings(s);
    }

    public void OnLockIn()
    {
        StartCoroutine(LockIn());
    }

    private IEnumerator LockIn()
    {
        //commonUI.IncrementProgressBar(_curGroup.Count);
        //commonUI.ShowPrompt("");
        //lockinButton.enabled = false;
        _sliderPanel.LockSliders(true);

        for (int k = 0; k < _curGroup.Count; k++)
        {
            //SliderSettings ss = _sliderPanel[k].Settings as SliderSettings;
            //_data.sliderSettings.Add(ss);
            //_state.testConditions[_curGroup[k]].discomfortLevel.Add(ss.isMaxed ? float.NaN : ss.end);
            //_state.testOrder.RemoveAt(0);
        }
        SaveState();

        yield return new WaitForSeconds(0.25f);

        //NGUITools.SetActive(lockinButton.gameObject, false);
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
        //_data.LDLgram.Save(DataFileLocations.LDLPath);

        //// 2. Save the data
        //_data.testConditions = new List<TestCondition>(_state.testConditions);
        //DiagnosticsManager.Instance.CompleteTest(_data, "LDLSliders");

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
