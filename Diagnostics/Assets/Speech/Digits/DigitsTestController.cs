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
using System.Numerics;
using Digits;
using NUnit.Framework.Interfaces;
using static UnityEngine.GraphicsBuffer;
using System.Runtime.Serialization;

public class DigitsTestController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private Camera _camera;
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private Text _finishText;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private QuestionBox _questionBox;
    [SerializeField] private GameObject _workPanel;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private DigitDisplay _digitDisplay;
    [SerializeField] private GameObject _fixationImage;
    [SerializeField] private GameObject _speakerPrefab;
    [SerializeField] private GameObject _prompt;

    private bool _isRemote;

    private bool _stopMeasurement = false;
    private bool _localAbort = false;

    private DigitsTestSettings _settings = new DigitsTestSettings();
    private List<Digits.TestSpec> _plan = new List<Digits.TestSpec>();
    private Digits.TestData _data;
    private Digits.TestStatus _status = new Digits.TestStatus();

    private static readonly int[] validDigits = new int[] { 1, 2, 3, 4, 5, 6, 8, 9 };
    private static readonly int _numDigitsToSpeak = 4;

    private DigitSpeaker _distractor1;
    private DigitSpeaker _distractor2;
    private DigitSpeaker _targetSpeaker;

    private int _numRampTrials = 0;
    private bool _doSimulation = false;
    private float _maxSPL;

    private string _dataPath;
    private string _mySceneName = "Digits";
    private string _configName;

    private InputAction _abortAction;

    private DigitsTestLog _log = new DigitsTestLog();

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
        _prompt.SetActive(false);

        _workPanel.SetActive(false);

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
            _settings = FileIO.XmlDeserialize<BasicMeasurementConfiguration>(fn) as DigitsTestSettings;
            InitializeMeasurement();
            Begin();
        }
    }

    void SetIllumination()
    {
        if (_settings.ApplyIllumination)
        {
            _camera.backgroundColor = GameManager.BackgroundColor;
            if (HardwareInterface.LED.IsInitialized)
            {
                HardwareInterface.LED.SetColorFromString(GameManager.LEDColorString);
                HTS_Server.SendMessage("ChangedLEDColors", GameManager.LEDColorString);
            }
        }
    }

    void InitializeMeasurement()
    {
        InitDataFile();
        CreatePlan();
        InitializeSpeakers();

        _progressBar.maxValue = _plan.Count;
        _progressBar.value = 0;

        HTS_Server.SendMessage(_mySceneName, $"File:{Path.GetFileName(_dataPath.Replace(".json", ""))}");
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
    }

    private void SaveData()
    {
        var header = new BasicMeasurementFileHeader()
        {
            measurementType = _mySceneName,
            configName = _configName,
            subjectID = GameManager.Subject
        };

        string json = FileIO.JSONStringAdd("", "info", KLib.FileIO.JSONSerializeToString(header));
        json = FileIO.JSONStringAdd(json, "params", KLib.FileIO.JSONSerializeToString(_settings));
        json = FileIO.JSONStringAdd(json, "data", KLib.FileIO.JSONSerializeToString(_data));
        json = FileIO.JSONStringAdd(json, "log", KLib.FileIO.JSONSerializeToString(_log.Trim()));
        json += Environment.NewLine;

        var filepath = _dataPath.Replace(".json", $"-{_data.name}.json");
        File.WriteAllText(filepath, json);

        HTS_Server.SendMessage(_mySceneName, $"ReceiveData:{Path.GetFileName(filepath)}:{File.ReadAllText(filepath)}");
    }

    private void CreatePlan()
    {
        if (_settings.NumPracticeTrials > 0)
        {
            _plan.Add(new Digits.TestSpec(
                type: Digits.TestSpec.TestType.Practice,
                SNR: float.PositiveInfinity,
                ITD: 0,
                numBlocks: 1,
                numTrialsPerBlock: _settings.NumPracticeTrials,
                criterion: 0.9f,
                curBlock: 0
            ));
            _plan.Add(new Digits.TestSpec(
                type: Digits.TestSpec.TestType.Practice,
                SNR: 20,
                ITD: 0,
                numBlocks: 1,
                numTrialsPerBlock: _settings.NumPracticeTrials,
                criterion: 0.9f,
                curBlock: 0
            ));
            _plan.Add(new Digits.TestSpec(
                type: Digits.TestSpec.TestType.Practice,
                SNR: 9,
                ITD: 0,
                numBlocks: 1,
                numTrialsPerBlock: _settings.NumPracticeTrials,
                criterion: 0,
                curBlock: 0
            ));
            _plan.Add(new Digits.TestSpec(
                type: Digits.TestSpec.TestType.Practice,
                SNR: 3,
                ITD: 0,
                numBlocks: 1,
                numTrialsPerBlock: _settings.NumPracticeTrials,
                criterion: 0,
                curBlock: 0
            ));
        }

        if (_settings.BlockOrder == BlockOrder.Sequential)
        {
            foreach (float snr in _settings.SNR)
            {
                _plan.Add(new Digits.TestSpec(
                    type: Digits.TestSpec.TestType.Test,
                    SNR: snr,
                    ITD: 0,
                    numBlocks: _settings.NumTestBlocksPerCondition,
                    numTrialsPerBlock: _settings.NumTestTrialsPerBlock,
                    criterion: 0,
                    curBlock: 0
                ));
            }
        }
        else
        {
            for (int k = 0; k < _settings.NumTestBlocksPerCondition; k++)
            {
                float[] SNRs = Expressions.Evaluate(_settings.SNR);
                if (_settings.BlockOrder != BlockOrder.Sequential)
                {
                    SNRs = KMath.Permute(SNRs);
                }
                foreach (float snr in SNRs)
                {
                    _plan.Add(new Digits.TestSpec(
                        type: Digits.TestSpec.TestType.Test,
                        SNR: snr,
                        ITD: 0,
                        numBlocks: 1,
                        numTrialsPerBlock: _settings.NumTestTrialsPerBlock,
                        criterion: 0,
                        curBlock: k
                    ));
                }
            }
        }
    }

    private void InitializeSpeakers()
    {
        var gobj = GameManager.Instantiate(_speakerPrefab);
        _distractor1 = gobj.GetComponent<DigitSpeaker>();
        _distractor1.speakerID = DigitSpeaker.SpeakerID.F01;

        gobj = GameManager.Instantiate(_speakerPrefab);
        _distractor2 = gobj.GetComponent<DigitSpeaker>();
        _distractor2.speakerID = DigitSpeaker.SpeakerID.M01;

        gobj = GameManager.Instantiate(_speakerPrefab);
        _targetSpeaker = gobj.GetComponent<DigitSpeaker>();
        _targetSpeaker.speakerID = DigitSpeaker.SpeakerID.M02;

        _maxSPL = DigitSpeaker.GetMaxLevel(GameManager.Transducer, "dBSPL");
    }

    private void Begin()
    {
        _localAbort = false;
        _stopMeasurement = false;

        SetIllumination();

        HTS_Server.SendMessage(_mySceneName, "Status:Instructions");
        ShowInstructions(Instructions.Intro);
    }

    private void StartMeasurement()
    {
        _title.gameObject.SetActive(false);
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(true);

        _status.dataFileNum++;
        _data = new Digits.TestData(_plan[_status.testNum], _status.dataFileNum);
        _status.blockNum = 0;

        HTS_Server.SendMessage(_mySceneName, $"Status:{_data.name}");

        _log.Clear();

        StartBlock();
    }

    private void StartBlock()
    {
        StartCoroutine(CoStartBlock());
    }

    private IEnumerator CoStartBlock()
    {
        yield return null;

        _data.NewBlock();
        _log.Add(Time.timeSinceLevelLoad, Time.realtimeSinceStartup, "Block");
        _status.trialNum = 0;

        _numRampTrials = 0;
        if (_settings.RampStep > 0 && _plan[_status.testNum].SNR < _settings.RampStart)
        {
            float endRamp = Mathf.Max(_plan[_status.testNum].SNR, _settings.RampEnd);
            _numRampTrials = Mathf.FloorToInt((float)(_settings.RampStart - endRamp) / (float)_settings.RampStep);
        }

        NextTrial();
    }

    public void NextTrial()
    {
        float SNR = _plan[_status.testNum].SNR;
        float ITD = _plan[_status.testNum].ITD;

        if (_plan[_status.testNum].type == Digits.TestSpec.TestType.Test && _status.trialNum < _numRampTrials)
        {
            SNR = _settings.RampStart - _settings.RampStep * _status.trialNum;
        }
        StartCoroutine(DoTrial(SNR, ITD));
    }

    IEnumerator DoTrial(float SNR, float ITD)
    {
        // Set the "listen" display
        if (!_doSimulation)
        {
            _fixationImage.SetActive(true);
        }
        _digitDisplay.Hide();

        float signalAtten = _settings.SpeakerLevel - _maxSPL;
        float maskerAtten = signalAtten - SNR;

        // Initialize speakers (select random digits, set levels)
        List<List<int>> availableDigits = new List<List<int>>();
        for (int k = 0; k < _numDigitsToSpeak; k++)
        {
            availableDigits.Add(new List<int>(validDigits));
        }

        _targetSpeaker.Randomize(availableDigits);
        _targetSpeaker.atten_dB = signalAtten;


        if (float.IsInfinity(SNR))
        {
            _distractor1.Clear();
            _distractor2.Clear();
        }
        else
        {
            _distractor1.Randomize(availableDigits);
            _distractor2.Randomize(availableDigits);
            _distractor2.atten_dB = _distractor1.atten_dB = maskerAtten;

            int sign = 2 * UnityEngine.Random.Range(0, 1) - 1;
            _distractor1.itd_us = sign * ITD;
            _distractor2.itd_us = (1 - sign) * ITD;
        }

        if (_doSimulation)
        {
            OnEnterCallback();
            yield break;
        }

        //Debug.Log("SNR = " + SNR + "; ITD = " + ITD + "; M02 Atten = " + _targetSpeaker.atten_dB + "; F01/M01 Atten = " + _distractor1.atten_dB);

        // Wait briefly
        _log.Add(Time.timeSinceLevelLoad, Time.realtimeSinceStartup, "Trial");

        yield return new WaitForSeconds(UnityEngine.Random.Range(_settings.MinDelay, _settings.MaxDelay));

        // Start speakers
        for (int k = 0; k < _numDigitsToSpeak; k++)
        {
            _log.Add(Time.timeSinceLevelLoad, Time.realtimeSinceStartup, $"Digit{k+1}");

            if (!float.IsInfinity(SNR))
            {
                _distractor1.SpeakDigit(k);
                _distractor2.SpeakDigit(k);
            }
            _targetSpeaker.SpeakDigit(k);

            // Wait for speaking to finish
            while (_distractor1.IsSpeaking || _distractor2.IsSpeaking || _targetSpeaker.IsSpeaking)
            {
                yield return null;
            }
        }

        // Prompt for response
        _log.Add(Time.timeSinceLevelLoad, Time.realtimeSinceStartup, "StartResponse");

        _fixationImage.SetActive(false);
        _digitDisplay.Show();
        _prompt.SetActive(true);
    }

    private IEnumerator EndTrial()
    {
        if (!_doSimulation)
        {
            yield return new WaitForSeconds(0.5f);
        }

        _prompt.SetActive(false);
        _digitDisplay.DisableLockInButton();

        // Add data
        Digits.Trial trialData = new Digits.Trial();
        trialData.F01 = _distractor1.SpokenDigits;
        trialData.M01 = _distractor2.SpokenDigits;
        trialData.M02 = _targetSpeaker.SpokenDigits;
        trialData.Response = (!_doSimulation) ? _digitDisplay.Value : AutoRunResponse();

        _data.AddTrial(trialData);

        // If practice, show feedback
        if (_plan[_status.testNum].type == Digits.TestSpec.TestType.Practice)
        {
            if (!_doSimulation)
            {
                yield return StartCoroutine(_digitDisplay.Feedback(_targetSpeaker.SpokenDigits));
            }
        }

        // Whither next?
        if (++_status.trialNum == _plan[_status.testNum].numTrialsPerBlock)
        {
            EndBlock();
        }
        else
        {
            if (!_doSimulation)
            {
                yield return new WaitForSeconds(0.75f);
            }
            NextTrial();
        }
    }

    private void EndBlock()
    {
        if (++_status.blockNum == _plan[_status.testNum].numBlocks)
        {
            EndTest();
        }
        else
        {
            // only reach this if we're running the test: practices are all single blocks

            if (PerformConsistencyTest())
            {
                EndTest();
            }
            //else if (KMath.IsMultipleOf(_status.blockNum, 3))
            //{
            //    _digitDisplay.Hide();
            //    //commonUI.FormatHelpBox("Nice work!\nTake a breather for a second.\nWe are going to do that again. ");
            //    //commonUI.ShowNextButton("Press Next when you're ready to start", StartBlock);
            //}
            else
            {
                StartBlock();
            }
        }
    }

    private void EndTest()
    {
        SaveData();

        _digitDisplay.Hide();

        // Success criterion?
        if (_data.FractionCorrect() < _plan[_status.testNum].criterion)
        {
            ShowInstructions(Instructions.MorePractice);
        }
        else
        {
            _status.testNum++;

            _progressBar.value = _status.testNum;
            int pctComplete = _status.testNum * 100 / _plan.Count;
            HTS_Server.SendMessage(_mySceneName, $"Progress:{pctComplete}");

            if (_status.testNum == 1)
            {
                ShowInstructions(Instructions.FinishedPractice1);
            }
            else if (_status.testNum == 2 && _plan.Count > _status.testNum && _plan[_status.testNum].type == Digits.TestSpec.TestType.Practice)
            {
                ShowInstructions(Instructions.FinishedPractice2);
            }
            else if (_status.testNum == 3 && _plan.Count > _status.testNum && _plan[_status.testNum].type == Digits.TestSpec.TestType.Practice)
            {
                ShowInstructions(Instructions.FinishedPractice3);
            }
            else if (_status.testNum < _plan.Count && _plan[_status.testNum - 1].type == Digits.TestSpec.TestType.Practice && _plan[_status.testNum].type == Digits.TestSpec.TestType.Test)
            {
                ShowInstructions(Instructions.StartTest);
            }
            else if (_status.testNum < _plan.Count)
            {
                ShowInstructions(Instructions.MoreTesting);
            }
            else
            {
                EndRun(abort: false);
            }
        }
    }

    private bool PerformConsistencyTest()
    {
        bool pass = false;
        if (_data.type != Digits.TestSpec.TestType.Test.ToString())
        {
            return false;
        }

        float[] numCorrect = new float[_data.blocks.Count];
        int totalCorrect = 0;
        int numTested = 0;

        for (int k = 0; k < _data.blocks.Count; k++)
        {
            Debug.Log("Block " + k + ": " + _data.blocks[k].numDigitsCorrect + " correct");
            numCorrect[k] = _data.blocks[k].numDigitsCorrect;
            totalCorrect += _data.blocks[k].numDigitsCorrect;
            numTested += _data.blocks[k].numDigitsTested;
        }

        float pctCorrect = (float)totalCorrect / (float)numTested;
        Debug.Log("CV = " + KMath.CoeffVar(numCorrect).ToString("F3"));
        Debug.Log(totalCorrect + " correct out of " + numTested + " = " + Mathf.RoundToInt(100 * pctCorrect).ToString() + "%");

        if (numTested < 520)
        {
            return false;
        }

        if (pctCorrect < 0.12)
        {
            Debug.Log("Quitting after " + _data.blocks.Count + " blocks dues to poor performance.");
            return true;
        }

        if (KMath.CoeffVar(numCorrect) <= 0.2f)
        {
            return true;
        }

        return pass;
    }

    private int[] AutoRunResponse()
    {
        int[] response = new int[4];
        float pc = (_data.type == Digits.TestSpec.TestType.Practice.ToString()) ? 1 : 0.5f;
        for (int k = 0; k < 4; k++)
        {
            response[k] = (UnityEngine.Random.Range(0f, 1f) <= pc) ? _targetSpeaker.SpokenDigits[k] : _distractor1.SpokenDigits[k];
        }
        return response;
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
        _log.Add(Time.timeSinceLevelLoad, Time.realtimeSinceStartup, "KeyPress");
    }

    private void OnEnterCallback()
    {
        StartCoroutine(EndTrial());
    }

    private void ShowInstructions(string instructions)
    {
        _workPanel.gameObject.SetActive(false);
        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = StartMeasurement;
        _instructionPanel.ShowInstructions(
            new Turandot.Instructions() 
            { 
                Text = instructions, 
                FontSize = _settings.InstructionFontSize,
                VerticalAlignment = Turandot.Instructions.VerticalTextAlignment.Middle,
            });
    }

    private void EndRun(bool abort)
    {
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(false);

        string _status = abort ? "Measurement aborted" : "Measurement finished";
        HTS_Server.SendMessage(_mySceneName, $"Finished:{_status}");

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
