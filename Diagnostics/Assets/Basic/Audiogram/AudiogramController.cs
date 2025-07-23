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

using Audiograms;
using KLib;
using KLib.Signals;
using KLib.Signals.Enumerations;
using KLib.Signals.Waveforms;
using TMPro.EditorUtilities;


public class AudiogramController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private TextAsset _defaultInstructions;
    [SerializeField] private Text _finishText;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private GameObject _workPanel;
    [SerializeField] private QuestionBox _questionBox;
    [SerializeField] private Image _buttonImage;
    [SerializeField] private Slider _progressBar;

    [SerializeField] private InputAction _responseAction;

    private bool _isRemote;

    private bool _stopMeasurement = false;
    private bool _localAbort = false;

    private AudiogramMeasurementSettings _settings = new AudiogramMeasurementSettings();

    private string _dataPath;
    private string _mySceneName = "Audiogram";

    private InputAction _stopMeasurementAction;

    ProcedureData _data = new ProcedureData();
    SignalManager _signalManager;

    private bool _doSimulation = false;

    float airBoneGap = 15; // assumed ABG
    float interauralAtten = 35; // measured by JPW in New York

    ANSI_dBHL dBHL_table;

    TrackData currentTrack;
    float currentResponseTime;
    float currentReversal;
    bool _buttonPressed;
    bool soundDetected;
    float _volumeWaitTime = 0.5f;

    private AudioSource _audioSource;

    enum SceneState { Idle, Instructions, Testing, Done };
    SceneState _sceneState = SceneState.Idle;

    private string _stateFile;
    private MeasurementState _state;
    private StimulusCondition _currentStimulusCondition = null;

    private string _configName;

    private void Awake()
    {
        _stopMeasurementAction = _actions.FindAction("Abort");
        _stopMeasurementAction.Enable();
        _stopMeasurementAction.performed += OnAbortAction;

        _responseAction.performed += OnSubjectResponse;
        _responseAction.Disable();

        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        HTS_Server.SetCurrentScene(_mySceneName, this);

        _title.text = "";

#if HACKING
        Application.targetFrameRate = 60;
        GameManager.SetSubject("Scratch/_shit");
        _configName = "Hello";
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
            var fn = FileLocations.ConfigFile("Audiogram", _configName);
            _settings = FileIO.XmlDeserialize<BasicMeasurementConfiguration>(fn) as AudiogramMeasurementSettings;
            InitializeMeasurement();
            Begin();
        }
    }

    void InitializeMeasurement()
    {
        _title.text = _settings.Title;

        InitDataFile();

        _stateFile = Path.Combine(FileLocations.SubjectFolder, $"{_mySceneName}.bin");

        dBHL_table = ANSI_dBHL.GetTable();
        SetReactionTimeLimits();

        AudiogramData extantData = AudiogramData.Load();
        if (_settings.Merge && extantData != null)
        {
            _data.audiogramData = extantData;
            _data.audiogramData.Append(_settings.TestFrequencies);
        }
        else
        {
            _data.audiogramData.Initialize(_settings.TestFrequencies);
        }
        _state = new MeasurementState(_settings.TestFrequencies, _settings.TestEar);
        _progressBar.maxValue = _state.NumStimulusConditions;
        _progressBar.value = 0;

        InitializeStimulusGeneration();

        HTS_Server.SendMessage(_mySceneName, $"File:{Path.GetFileName(_dataPath)}");
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
        Debug.Log($"data path = {_dataPath}");
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

            _sceneState = SceneState.Instructions;
            HTS_Server.SendMessage(_mySceneName, "Status:Instructions");
            _instructionPanel.InstructionsFinished = StartMeasurement;
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

        if (File.Exists(_stateFile))
        {
            _questionBox.gameObject.SetActive(true);
            _questionBox.PoseQuestion("Continue previous session?", OnQuestionResponse);
        }
        else
        {
            _sceneState = SceneState.Testing;

            _instructionPanel.gameObject.SetActive(false);
            _workPanel.SetActive(true);
            AdvanceMeasurement();
        }
    }

    private void OnQuestionResponse(bool yes)
    {
        _questionBox.gameObject.SetActive(false);

        if (yes)
        {
            ProcedureData d = RestoreState(false);
            _progressBar.value = _state.NumCompleted;
            AdvanceMeasurement();
        }
        else
        {
            File.Delete(_stateFile);
            StartMeasurement();
        }
    }

    void OnAbortAction(InputAction.CallbackContext context)
    {
        _stopMeasurement = true;
        _stopMeasurementAction.Disable();

        _workPanel.SetActive(false);
        _instructionPanel.gameObject.SetActive(false);
        _quitPanel.SetActive(true);
    }

    public void OnQuitConfirmButtonClick()
    {
        _localAbort = true;
        _stopMeasurement = true;

        _stopMeasurementAction.Disable();
        _stopMeasurement = false;

        EndRun(abort: true);
    }

    public void OnQuitCancelButtonClick()
    {
        _quitPanel.SetActive(false);
        _stopMeasurementAction.Enable();

        if (_sceneState == SceneState.Instructions)
        {
            _instructionPanel.InstructionsFinished = StartMeasurement;
            ShowInstructions(
                instructions: _settings.InstructionMarkdown,
                fontSize: _settings.InstructionFontSize);
        }
        else if (_sceneState == SceneState.Testing)
        {
            _stopMeasurement = false;
            DoCurrentStimulusCondition();  
        }
    }

    private void ShowInstructions(string instructions, int fontSize)
    {
        _workPanel.SetActive(false);
        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.ShowInstructions(new Turandot.Instructions() { Text = instructions, FontSize = fontSize });
    }

    private void EndRun(bool abort)
    {
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(false);

        File.AppendAllText(_dataPath, FileIO.JSONSerializeToString(_data));
        _data.audiogramData.Save();

        // Delete state file
        if (File.Exists(_stateFile) && _state.IsCompleted)
        {
            File.Delete(_stateFile);
        }

        // Exit
        _sceneState = SceneState.Done;

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
                _configName = "remote";
                _settings = FileIO.XmlDeserializeFromString<BasicMeasurementConfiguration>(data) as AudiogramMeasurementSettings;
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
    
    private void SetReactionTimeLimits()
    {
        //float subjMeanResponseTime_s = SubjectManager.Instance.GetMetric("ResponseTime_s", _settings.MaxValidResponseTime);

        //float nominalCenter = 0.5f * (_settings.MinValidResponseTime + _settings.MaxValidResponseTime);
        //if (subjMeanResponseTime_s > nominalCenter)
        //{
        //    float dt = subjMeanResponseTime_s - nominalCenter;
        //    _settings.MaxValidResponseTime += dt;
        //    _settings.MinISI += dt;
        //    _settings.MaxISI += dt;
        //}
    }

    

    private void AdvanceMeasurement()
    {
        _progressBar.value = _state.NumCompleted;
        HTS_Server.SendMessage(_mySceneName, $"Progress:{_state.PercentComplete}");

        _currentStimulusCondition = _state.GetNext();

        if (_currentStimulusCondition == null)
        {
            EndRun(abort: false);
            return;
        }

        HTS_Server.SendMessage(_mySceneName, $"Status:{_currentStimulusCondition.Laterality} ear, {_currentStimulusCondition.Frequency} Hz");

        if (_currentStimulusCondition.NewFrequency)
        {
            _instructionPanel.InstructionsFinished = DoCurrentStimulusCondition;

            ShowInstructions(
                "- Nice work!\n" +
                "- Let's try some more.\n" +
                "- The pitch will be different, but your job is the same.",
                _settings.InstructionFontSize);
        }
        else if (_currentStimulusCondition.NewEar)
        {
            _instructionPanel.InstructionsFinished = DoCurrentStimulusCondition;
            ShowInstructions("- Great!\n- Let's try the same thing with your right ear", _settings.InstructionFontSize);
        }
        else
        {
            DoCurrentStimulusCondition();
        }
    }

    private void DoCurrentStimulusCondition()
    {
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(true);
        StartCoroutine(TestOneEar(_currentStimulusCondition.Laterality, _currentStimulusCondition.Frequency));
    }

    private void OnSubjectResponse(InputAction.CallbackContext context)
    {
        _buttonPressed = true;
        StartCoroutine(SimulateButtonPress());
    }

    private bool IsMaskedThresholdRequired(float leftThreshold, float rightThreshold)
    {
        return false;
        //return _testLaterality == Laterality.Diotic && 
        //    Mathf.Abs(leftThreshold - rightThreshold) + airBoneGap >= interauralAtten &&
        //    !float.IsNaN(leftThreshold) && !float.IsInfinity(leftThreshold) &&
        //    !float.IsNaN(rightThreshold) && !float.IsInfinity(rightThreshold);
    }

    IEnumerator TestOneEar(Laterality ear, float freq)
    {
        float startHL = _settings.Abridged ? 30 : 50;

        _responseAction.Enable();

        yield return new WaitForSeconds(_doSimulation ? 0.2f : UnityEngine.Random.Range(_settings.MinISI, _settings.MaxISI));

        _signalManager["Signal"].Laterality = ear;
        var fm = (_signalManager["Signal"].waveform as FM);

        fm.Carrier_Hz = freq;
        fm.ModFreq_Hz = freq * _settings.ModDepth / 100f;
        fm.ModFreq_Hz = _settings.ModRate;

        _signalManager.Initialize();
        _signalManager.StartPaused();

        float maxSPL = _signalManager["Signal"].GetMaxLevel();
        float maxHL = dBHL_table.SPL_To_HL(freq, maxSPL);
        startHL = Mathf.Min(startHL, maxHL);

        _signalManager.Unpause();

        if (_currentStimulusCondition.NumTries == 1)
        {
            Debug.Log("Repeat track");
        }

        currentTrack = new TrackData(ear, freq, 50);
        yield return StartCoroutine(DoMonauralTrack(freq, startHL, maxHL));

        _responseAction.Disable();

        currentTrack.Trim();
        _data.tracks.Add(currentTrack);

        if (_currentStimulusCondition.NumTries==0 && float.IsPositiveInfinity(currentTrack.thresholdHL))
        {
            _currentStimulusCondition.NumTries++;
            _instructionPanel.InstructionsFinished = DoCurrentStimulusCondition;

            ShowInstructions(
                "- Okay, remember you should push the button as soon as you hear a sound",
                _settings.InstructionFontSize);

            yield break;
        }

        if (_stopMeasurement)
        {
            yield break;
        }

        if (float.IsNaN(currentTrack.thresholdHL))
        {
            Debug.Log("Computing alternate threshold...");
            currentTrack.thresholdHL = GetAlternateThreshold();
            currentTrack.alternateComputation = true;
        }

        _data.audiogramData.Set(
            _currentStimulusCondition.Laterality,
            freq,
            currentTrack.thresholdHL,
            currentTrack.thresholdHL + dBHL_table.HL_To_SPL(freq));

        _state.SetCompleted(_currentStimulusCondition, currentTrack.thresholdHL);
        SaveState();

        AdvanceMeasurement();
    }

    /// <summary>
    /// Compute threshold for case where two repeats do not yield consistent reversals.
    /// </summary>
    /// <returns>The alternate threshold.</returns>
    private float GetAlternateThreshold()
    {
        // Combine reversals from current track...
        List<float> reversals = new List<float>(currentTrack.reversals);
        // ...and the previous one
        reversals.AddRange(_data.tracks[_data.tracks.Count - 2].reversals);

        // Sort-Reverse puts the biggest ones first
        reversals.Sort();
        reversals.Reverse();
        // Compute mean of N biggest (here N = #reversals/2)
        return KMath.Mean(reversals.GetRange(0, Mathf.RoundToInt(reversals.Count / 2)).ToArray());
    }

    IEnumerator DoMonauralTrack(float freq, float startLevel, float maxLevel)
    {
        float[] reversalLevels = new float[6];
        for (int k = 0; k < reversalLevels.Length; k++)
            reversalLevels[k] = float.NaN;

        float curLevel = startLevel;

        _volumeWaitTime = 2.0f;

        // Start, go up until we get a response
        soundDetected = false;
        float delta = _settings.Abridged ? 5 : 15;
        while (!soundDetected && curLevel <= Mathf.Min(startLevel + 45, maxLevel))
        {
            yield return StartCoroutine(PlayStimulusAndGetResponse(freq, curLevel));
            currentTrack.Add(curLevel, float.NaN, currentResponseTime, soundDetected);
            if (_stopMeasurement) yield break;

            if (!soundDetected)
            {
                if (curLevel == maxLevel)
                    break;

                curLevel = Mathf.Min(curLevel + delta, maxLevel);
            }
        }

        // Couldn't get a response
        if (!soundDetected)
        {
            currentTrack.thresholdHL = float.PositiveInfinity;
            yield break;
        }

        if (_settings.Abridged)
        {
            curLevel = 10;
        }
        else
        {
            curLevel -= 10;
        }

        // Acquire 3 reversals
        for (int k = 0; k < 3; k++)
        {
            yield return StartCoroutine(DoReversal(freq, curLevel, maxLevel));
            if (_stopMeasurement) yield break;

            if (float.IsNaN(currentReversal))
            {
                yield break;
            }
            reversalLevels[k] = currentReversal;
            curLevel = currentReversal;

            int index = LookForNRepeats(reversalLevels, 2);
            if (index >= 0)
            {
                currentTrack.thresholdHL = reversalLevels[index];
                yield break;
            }
        }

        // If no winner (and there's not, if we got this far), acquire two more reversals
        for (int k = 0; k < 2; k++)
        {
            yield return StartCoroutine(DoReversal(freq, curLevel, maxLevel));
            if (_stopMeasurement) yield break;

            if (float.IsNaN(currentReversal))
            {
                yield break;
            }
            reversalLevels[k + 3] = (int)currentReversal;
            curLevel = currentReversal;

            int index = LookForNRepeats(reversalLevels, 3);
            if (index >= 0)
            {
                currentTrack.thresholdHL = reversalLevels[index];
                yield break;
            }
        }
    }

    int LookForNRepeats(float[] array, int ntarg)
    {
        for (int k = 0; k < array.Length - ntarg + 1; k++)
        {
            if (float.IsNaN(array[k]))
                break;

            int n = 1;
            int testVal = (int)array[k];

            for (int j = k + 1; j < array.Length; j++)
            {
                if (float.IsNaN(array[j]))
                    break;

                if ((int)array[j] == testVal)
                {
                    if (++n == ntarg)
                    {
                        return k;
                    }
                }
            }
        }

        return -1;
    }

    IEnumerator DoReversal(float freq, float level, float maxLevel)
    {
        currentReversal = float.NaN;

        while (soundDetected)
        {
            yield return StartCoroutine(PlayStimulusAndGetResponse(freq,level));
            currentTrack.Add(level, float.NaN, currentResponseTime, soundDetected);

            if (soundDetected )
            {
                level -= 10;
            }

            if (_stopMeasurement) yield break;
        }
        while (!soundDetected)
        {
            level += 5;
            if (level > maxLevel)
            {
                yield break;
            }
            yield return StartCoroutine(PlayStimulusAndGetResponse(freq,level));
            currentTrack.Add(level, float.NaN, currentResponseTime, soundDetected);

            if (_stopMeasurement) yield break;
        }
        currentReversal = level;
        currentTrack.AddReversal(currentReversal);
    }

    IEnumerator FindMaskedThreshold(Ear ear, float freq, float originalThreshold)
    {
        //commonUI.ShowPrompt("Press the radio as soon as you hear a sound");

        //float maskerLevel = originalThreshold - 10;
        //_signalManager["Signal"].Destination = ear == Ear.Left ? Laterality.Left : Laterality.Right;
        //_signalManager.Initialize();
        //float maxSPL = _signalManager["Signal"].GetMaxLevel();

        //noise.filter.CF = freq;
        //noiseGen.SigMan["Masker"].Destination = ear == Ear.Right ? Laterality.Left : Laterality.Right;
        //noiseGen.SigMan["Masker"].level.Units = LevelUnits.dB_SPL;
        //noiseGen.SigMan.MaxLevelMargin = _signalManager.MaxLevelMargin;
        //noiseGen.SigMan.Initialize();

        //_settings.MaxMaskerSPL = noiseGen.SigMan["Masker"].GetMaxLevel();

        //volumeControl.SetAttenuation(0);

        //_stopMeasurement = false;

        //while (true)
        //{
        //    currentTrack = new TrackData(ear, freq, 50);
        //    currentTrack.maskerLevel = maskerLevel;

        //    noiseGen.SigMan["Masker"].level.Value = maskerLevel;
        //    noiseGen.Play();

        //    yield return new WaitForSeconds(Random.Range(_settings.minISI_s, _settings.maxISI_s));

        //    yield return StartCoroutine(DoMonauralTrack(Mathf.Min(originalThreshold + 20, maxSPL), maxSPL));

        //    noiseGen.Stop();
        //    while (noiseGen.IsPlaying)
        //    {
        //        yield return new WaitForSeconds(0.1f);
        //    }

        //    currentTrack.Trim();
        //    _data.tracks.Add(currentTrack);

        //    if (_stopMeasurement || currentTrack.thresholdHL <= originalThreshold)
        //    {
        //        yield break;
        //    }
        //    else
        //    {
        //        maskerLevel += (currentTrack.thresholdHL - originalThreshold);
        //        originalThreshold = currentTrack.thresholdHL;
        //        if (maskerLevel > _settings.MaxMaskerSPL || originalThreshold > maxSPL)
        //        {
        //            currentTrack.thresholdHL = float.NaN;
        //            yield break;
        //        }
        //    }
        //}
        yield break;
    }

    IEnumerator PlayStimulusAndGetResponse(float freq, float level)
    {
        if (_doSimulation)
        {
            yield return StartCoroutine(Simulate(level));
            yield break;
        }

        // 500-ms steps
        float waitTime = 0.5f * Mathf.Round(UnityEngine.Random.Range(_settings.MinISI, _settings.MaxISI) * 2);

        _signalManager["Signal"].level.Value = level + dBHL_table.HL_To_SPL(freq);

        //float vol_dB = noiseGen.IsPlaying ? 0 : volumeControl.SetAttenuation(_signalManager.MinAtten());
        //_signalManager.SetMasterVolume(vol_dB);

        yield return new WaitForSeconds(_volumeWaitTime); // the volume seems to take a finite time to get switched. (Duh.)
        _volumeWaitTime = 0.5f;

        _audioSource.clip = _signalManager.CreateClip();

        _buttonPressed = false;
        soundDetected = false;
        currentResponseTime = -1;

        Debug.Log("playing stimulus");
        _audioSource.Play();

        float t = 0;
        while (t < waitTime)
        {
            yield return null;

            if (currentResponseTime < 0 && _buttonPressed)
                currentResponseTime = t;

            t += Time.deltaTime;

            if (_stopMeasurement) yield break;
        }
        soundDetected = currentResponseTime >= _settings.MinValidResponseTime && currentResponseTime <= _settings.MaxValidResponseTime;
    }

    IEnumerator Simulate(float level)
    {
        currentResponseTime = (level > 40) ? _settings.MaxValidResponseTime / 2 : 0;
        soundDetected = currentResponseTime >= _settings.MinValidResponseTime && currentResponseTime <= _settings.MaxValidResponseTime;
        if (soundDetected)
        {
            yield return StartCoroutine(SimulateButtonPress());
        }
        yield break;
    }

    void InitializeStimulusGeneration()
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
                Active = true,
                Duration_ms = _settings.ToneDuration,
                Ramp_ms = _settings.Ramp
            }
        };

        int npts = (int)Mathf.Ceil(AudioSettings.outputSampleRate * _settings.ToneDuration / 1000);
        if (_settings.NumPips > 1)
        {
            signalChannel.gate.Period_ms = _settings.IPI_ms;
            npts = (int)Mathf.Ceil(AudioSettings.outputSampleRate * 0.001f * ((_settings.NumPips - 1) * _settings.IPI_ms + _settings.ToneDuration));
        }

        _signalManager.AddChannel(signalChannel);
        _signalManager.Initialize(audioConfig.sampleRate, npts);

        // Initialize masker channel
        //noise.filter.shape = FilterShape.Band_pass;
        //noise.filter.bandwidthMethod = BandwidthMethod.Octaves;
        //noise.filter.BW = 1f / 3f;

        //noiseGen.InitializeAudio(transducer);
        //noiseGen.SigMan.AddChannel("Masker", noise);
        //noiseGen.SigMan["Masker"].Destination = Laterality.Diotic;
        //noiseGen.SigMan["Masker"].level.Units = LevelUnits.dB_SPL;
    }

    public void OnButtonClick()
    {
        StartCoroutine(SimulateButtonPress());
        _buttonPressed = true;
    }

    private void SaveState()
    {
        FileIO.CreateBinarySerialization(_stateFile);
        FileIO.SerializeToBinary(_state);
        FileIO.SerializeToBinary(_data);
        FileIO.CloseBinarySerialization();
    }

    private ProcedureData RestoreState(bool discard)
    {
        ProcedureData d = null;
        MeasurementState s;

        FileIO.OpenBinarySerialization(_stateFile);
        try
        {
            s = FileIO.DeserializeFromBinary<MeasurementState>();
            d = FileIO.DeserializeFromBinary<ProcedureData>();

            if (!discard)
            {
                _state = s;
                _data = d;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log($"error restoring state: {ex.Message}");
        }
        FileIO.CloseBinarySerialization();
        return d;
    }

    IEnumerator SimulateButtonPress()
    {
        var color = _buttonImage.color;
        _buttonImage.color = Color.yellow;
        yield return new WaitForSeconds(0.25f);
        _buttonImage.color = color;
    }

    
}
