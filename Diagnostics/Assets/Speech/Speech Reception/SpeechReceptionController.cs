using KLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using ExtensionMethods;
using SpeechReception;
using UnityEngine.Audio;
using UnityEngine.Networking;
using TMPro.EditorUtilities;
using System.Linq;
using System.Runtime.Serialization;

public class SpeechReceptionController : MonoBehaviour, IRemoteControllable
{
    [Header("UI Elements")]
    [SerializeField] private Camera _camera;
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private TextAsset _defaultInstructions;
    [SerializeField] private Text _finishText;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private QuestionBox _questionBox;
    [SerializeField] private GameObject _workPanel;

    [Header("Work Panel Elements")]
    [SerializeField] private Slider _progressBar;
    [SerializeField] private GameObject _fixationPoint;
    [SerializeField] private TMPro.TMP_Text _prompt;

    [Header("Audio")]
    [SerializeField] private SpeechMasker _masker;

    [Header("Controls")]
    [SerializeField] private RecordPanel _recordPanel;
    public ClosedSetController closedSetController;
    public MatrixTestController _matrixTestController;

    private bool _isRemote;

    private bool _localAbort = false;
    private bool _abortRequested = false;
    private bool _abortProcessed = false;

    private SpeechTest _settings = null;

    private int _runNumber;
    private string _dataPath;
    private string _mySceneName = "SpeechReception";
    private string _configName;

    private InputAction _abortAction;

    private TestPlan _plan;

    private AudioSource _audioPlay;
    private AudioSource _audioAlert;

    private ListProperties _srList;
    private Data _srData;
    private Data.Response _provisionalResponse;

    private VolumeManager _volumeManager;

    private int _numPracticeLists;
    private int _numSinceLastBreak;

    private ClosedSetData _srClosedSetData = new ClosedSetData();
    private MatrixTestData _srMatrixTestData = new MatrixTestData();

    private TestLog _log = new TestLog();

    private enum State
    {
        Idle,
        Instructions,
        PlayingSentence,
        AwaitingResponse
    }
    private State _state = State.Idle;

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += OnAbortAction;

        CreateAudioSources();

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
        _workPanel.SetActive(false);
        
        _recordPanel.StatusUpdate = OnRecordStatusChanged;
        _recordPanel.Initialize(22050, _audioAlert);
        _recordPanel.gameObject.SetActive(false);

        _volumeManager = new VolumeManager();

#if HACKING
        Application.targetFrameRate = 60;
        GameManager.SetSubject("Scratch/_shit");
        _configName = "QuickSIN_Test1";
        GameManager.Transducer = "Freefield";
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

    private void CreateAudioSources()
    {
        _audioPlay = gameObject.AddComponent<AudioSource>();
        _audioPlay.bypassEffects = true;
        _audioPlay.bypassListenerEffects = true;
        _audioPlay.bypassReverbZones = true;
        _audioPlay.loop = false;
        _audioPlay.spatialBlend = 0;

        _audioAlert = gameObject.AddComponent<AudioSource>();
        _audioAlert.bypassEffects = true;
        _audioAlert.bypassListenerEffects = true;
        _audioAlert.bypassReverbZones = true;
        _audioAlert.loop = false;
        _audioAlert.spatialBlend = 0;
    }

    void InitializeMeasurement()
    {
        CreatePlan();

        _progressBar.maxValue = _plan.totalNumSentences;
        _recordPanel.AudioCuesOnly = _settings.AudioCuesOnly;

        CreateDataFileName(_settings.TestName, "SyncLog");
    }

    string CreateDataFileName(string testName, string listName)
    {
        var fileStemStart = $"{GameManager.Subject}-Speech";
        string fileStem = "";
        while (true)
        {
            _runNumber = GameManager.GetNextRunNumber("Speech");
            fileStem = $"{fileStemStart}-Run{_runNumber:000}";
            var existingFiles = Directory.GetFiles(FileLocations.SubjectFolder, fileStem + "*.json");
            if (existingFiles==null || existingFiles.Length == 0)
            {
                break;
            }
        }

        if (!string.IsNullOrEmpty(listName))
        {
            listName = $"-{listName}";
        }
        _dataPath = Path.Combine(FileLocations.SubjectFolder, $"{fileStem}-{testName}{listName}.json");
        HTS_Server.SendMessage(_mySceneName, $"File:{Path.GetFileName(_dataPath)}");

        return _dataPath;
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
            _settings.TestEars = new List<TestEar>() { TestEar.Binaural };
        }

        int[] iorder = new int[_settings.TestEars.Count];
        if (_settings.EarOrder == "Random")
        {
            iorder = KMath.Permute(_settings.TestEars.Count);
        }
        else
        {
            for (int k = 0; k < iorder.Length; k++) iorder[k] = k;
        }

        foreach (int k in iorder)
        {
            var testEar = _settings.TestEars[k];
            if (testEar == TestEar.SubjectDefault)
            {
                string subjectDefault = GameManager.Metrics["TestEar"];

                if (string.IsNullOrEmpty(subjectDefault))
                    throw new Exception("Subject default test ear not set");
                else if (subjectDefault == "Left")
                    testEar = TestEar.Left;
                else if (subjectDefault == "Right")
                    testEar = TestEar.Right;
                else if (subjectDefault == "Binaural")
                    testEar = TestEar.Binaural;
                else
                    throw new Exception("Subject default test ear not set properly");

            }

            var t = _settings.Clone();
            t.Initialize(testEar, customizations.Get(_settings.TestSource));
            _plan.tests.Add(t);
            _plan.totalNumSentences += t.NumSentences;
        }
    }

    private void Begin()
    {
        _localAbort = false;
        _abortRequested = false;

        _numSinceLastBreak = 0;

        SetIllumination();

        if (_plan.IsRunInProgress())
        {
            Debug.Log($"{_mySceneName}: Previous state exists. Asking whether to resume");
            HTS_Server.SendMessage(_mySceneName, "Status:Asking to resume");

            _questionBox.gameObject.SetActive(true);
            _questionBox.PoseQuestion("Continue previous session?", OnQuestionResponse);
        }
        else
        {
            StartCoroutine(StartNextList());
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

    private void OnQuestionResponse(bool yes)
    {
        _questionBox.gameObject.SetActive(false);

        if (yes)
        {
            Debug.Log($"{_mySceneName}: Resuming previous");
            HTS_Server.SendMessage(_mySceneName, "Status:Resuming previous");

            _plan = TestPlan.RestoreSavedState();
            _srList = _plan.GetCurrentList();

            // Must start new data file for time logging purposes.
            // Data files will need to be merged post hoc.
            _srData = InitializeOpenSetData();
            _dataPath = CreateDataFileName(_settings.TestName, _srList.Title);

            _log.Clear();
            _log.Add($"List started: {_srList.Title}");
           HTS_Server.SendMessage(_mySceneName, $"Progress:{_plan.PercentComplete}");
           
            if (_settings.UseMicrophone)
            {
                _recordPanel.gameObject.SetActive(true);
            }

            SetLevel(_srList.Level, _srList.Units, _srList.TestEar);

            StartNextSentence();
        }
        else
        {
            Debug.Log($"{_mySceneName}: starting test over");
            HTS_Server.SendMessage(_mySceneName, "Status:Starting test over");

            StartCoroutine(StartNextList());
        }
    }

    private void SetLevel(float level, SpeechReception.LevelUnits units, SpeechReception.TestEar testEar)
    {
        float dB_wavfile_fullscale = float.NaN;

        float ref_dB = _settings.GetReference(GameManager.Transducer, units);

        dB_wavfile_fullscale = ref_dB;
        Debug.Log("dB_wavfile_fullscale" + " = " + dB_wavfile_fullscale);

        float atten = level - dB_wavfile_fullscale;
        Debug.Log("atten" + " = " + atten);
        atten = Math.Min(0f, atten);

        _audioPlay.panStereo = _srList.TestEar.ToBalance();

        _audioPlay.volume = Mathf.Pow(10, atten / 20);
        _audioAlert.volume = Mathf.Pow(10, (atten - 10) / 20);
    }

    public IEnumerator StartNextList()
    {
        yield return null;

        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(true);

        // Pop next test off the stack
        _srList = _plan.GetNextList();
        _srList.ApplySequence();
        _plan.Save();

        CreateDataFileName(_settings.TestName, _srList.Title);

        if (_settings.TestType == TestType.ClosedSet)
        {
            //            closedSetController.Initialize(_srList.closedSet, _srList.GetClosedSetResponses());
        }
        else if (_settings.TestType == TestType.Matrix)
        {
            //_matrixTestController.Initialize();
            //_srList.matrixTest.Initialize();
        }
        else if (_settings.UseMicrophone)
        {
            _recordPanel.gameObject.SetActive(true);
        }

        SetLevel(_srList.Level, _srList.Units, _srList.TestEar);

        _log.Clear();
        _log.Add($"List started: {_srList.Title}");
        Debug.Log($"List started: {_srList.Title}");
        HTS_Server.SendMessage(_mySceneName, $"Status: Starting list '{_srList.Title}'");

        _settings.Lists.RemoveAt(0);

        // Initialize summary data object
        if (_settings.TestType == TestType.ClosedSet)
        {
            _srClosedSetData = new ClosedSetData(
                _plan.currentListIndex < _settings.NumPracticeLists,
                _srList.Sequence.NumBlocks,
                _srList.Sequence.ItemsPerBlock,
                _srList.ClosedSet.PerformanceCriteria);

            _srClosedSetData.date = DateTime.Now.ToString();
            _srClosedSetData.test = _settings.TestType + "-" + _srList.Title;
            _srClosedSetData.testEar = _srList.TestEar;
            _srClosedSetData.runNumber = _runNumber;
        }
        else if (_settings.TestType == TestType.Matrix)
        {
            _srMatrixTestData = new MatrixTestData(_srList.MatrixTest.StimLevel, _srList.MatrixTest.MaskerLevel, _srList.MatrixTest.Mode);

            _srMatrixTestData.date = DateTime.Now.ToString();
            _srMatrixTestData.test = _settings.TestType + "-" + _srList.Title;
            _srMatrixTestData.testEar = _srList.TestEar;
            _srMatrixTestData.runNumber = _runNumber;
        }
        else
        {
            _srData = InitializeOpenSetData();
        }

        if (_settings.TestType != TestType.QuickSIN && _srList.UseMasker)
        {
            float maskerLevel = _srList.Level - _srList.sentences[_plan.currentSentenceIndex].SNR;
            if (_settings.TestType == TestType.Matrix) maskerLevel = _srList.MatrixTest.MaskerLevel;
            //yield return StartCoroutine(masker.Initialize(
            //    _srList.masker,
            //    maskerLevel,
            //    GameManager.Transducer,
            //    _srList.Units,
            //    _srList.laterality));
        }

        // Display instructions if needed
        var instructions = GetInstructions(_plan.currentListIndex);
        if (!string.IsNullOrEmpty(instructions))
        {
            ShowInstructions(instructions);
        }
        else if (_plan.currentListIndex > 0)
        {
            ShowInstructions("-Great!\n-Let's try some more.");
        }
        else
        {
            StartNextSentence();
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

    private Data InitializeOpenSetData()
    {
        var openSetData = new Data(_plan.currentListIndex < _settings.NumPracticeLists)
        {
            date = DateTime.Now.ToString(),
            test = $"{_settings.TestType}-{_srList.Title}",
            Fs = _recordPanel.Fs,
            runNumber = _runNumber,
            testEar = _srList.TestEar
        };
        return openSetData;
    }

    public void StartNextSentence()
    {
        _instructionPanel.gameObject.SetActive(false);
        _state = State.PlayingSentence;

#if KDEBUG
        if (KDebug.Settings.active && KDebug.Settings.data == KDebug.Data.Simulate)
        {
            commonUI.FormatHelpBox(_srList.sentences[_qnum].whole);
            commonUI.IncrementProgressBar();
            AutoRespond();
            StartCoroutine(AutoAdvance(ResponseAcquired));
        }
#else
        StartCoroutine(DoSentence());
#endif
    }

    private IEnumerator DoSentence()
    {
        _workPanel.SetActive(true);
        _fixationPoint.SetActive(true);
        _prompt.gameObject.SetActive(true);
        _prompt.text = "Listen...";

        string wavfile = _srList.sentences[_plan.currentSentenceIndex].wavfile;

        //Debug.Log(_qnum + ": " + wavfile);

        string filePath = Path.Combine(FileLocations.SpeechWavFolder, _settings.TestSource, wavfile);

        // On Windows, you often need "file:///" prefix for local files when using UnityWebRequest,
        // although using System.Uri is more robust across platforms.
        string uriPath = new System.Uri(filePath).AbsoluteUri;

        // Use UnityWebRequestMultimedia.GetAudioClip to create the request
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(uriPath, AudioType.WAV)) // Specify the AudioType
        {
            // Send the request and wait for it to complete
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading audio clip: " + uwr.error);
            }
            else
            {
                // Get the AudioClip content from the download handler
                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                _audioPlay.clip = clip;
            }
        }

        if (_settings.TestType == TestType.Matrix)
        {
            SetLevel(_srList.MatrixTest.StimLevel, _srList.Units, _srList.TestEar);
        }

        _log.Add($"Sentence:{_plan.currentSentenceIndex}");
        HTS_Server.SendMessage(_mySceneName, $"Status: Sentence {_plan.currentSentenceIndex}...");

        if (_settings.TestType != TestType.QuickSIN && _srList.UseMasker)
        {
            if (_settings.TestType == TestType.Matrix)
            {
                _masker.SetLevel(_srList.MatrixTest.MaskerLevel);
            }
            else
            {
                _masker.SetLevel(_srList.Level - _srList.sentences[_plan.currentSentenceIndex].SNR);
            }
            _log.Add($"MaskerOff:{_plan.currentSentenceIndex}");
            _masker.Play();
        }

        float delay_s = 0;
        if (_settings.MaxDelay_s > 0)
        {
            delay_s = Expressions.UniformRandomNumber(_settings.MinDelay_s, _settings.MaxDelay_s);
            yield return new WaitForSeconds(delay_s);
        }

        // Sentence
        _volumeManager.SetMasterVolume(0, VolumeManager.VolumeUnit.Decibel);

        _log.Add($"SentenceStart:{_plan.currentSentenceIndex}");

        _audioPlay.Play();
        
        yield return new WaitForSeconds(_audioPlay.clip.length);

        _log.Add($"SentenceEnd:{_plan.currentSentenceIndex}");

        // Post-sentence baseline
        float wait_s = 0;
        if (_srList.UseMasker)
        {
            wait_s = _settings.SentenceDuration_s - delay_s - _audioPlay.clip.length;
        }

        if (wait_s > 0)
        {
            yield return new WaitForSeconds(wait_s);
        }

        if (_srList.UseMasker)
        {
            _masker.Stop();
            _log.Add($"MaskerOff:{_plan.currentSentenceIndex}");
        }

        _fixationPoint.SetActive(false);

        if (_abortRequested)
        {
            ProcessAbortRequest();
            yield break;
        }

        _state = State.AwaitingResponse;

        //if (_useClosedSet)
        //{
        //    StartCoroutine(AcquireClosedSetResponse());
        //}
        //else if (_useMatrixTest)
        //{
        //    StartCoroutine(AcquireMatrixTestResponse());
        //}
        //else
        {
            var reviewThisOne = _settings.ReviewResponses && _plan.numSentencesDone < _settings.NumToReview;
            _recordPanel.AcquireAudioResponse(reviewThisOne);
        }
    }

    private string SaveResponseClip()
    {
        Response r = new Response();
        r.test = _srData.test;
        r.sentenceNum = _plan.currentSentenceIndex;
        r.sentence = _srList.sentences[_plan.currentSentenceIndex].whole;
        r.Fs = _srData.Fs;
        r.attemptNum = _recordPanel.NumAttempts;
        r.date = DateTime.Now.ToString();

        string responsePath = _dataPath.Replace(".json", $"-{_plan.currentSentenceIndex}");
        if (_recordPanel.NumAttempts > 1)
        {
            responsePath += "-" + _recordPanel.NumAttempts;
        }

        string json = FileIO.JSONStringAdd("", "info", FileIO.JSONSerializeToString(r));
        File.WriteAllText($"{responsePath}.json", json);

        KLib.Wave.WaveFile.Write(_recordPanel.Data, (uint)r.Fs, 16, $"{responsePath}.wav");

        return responsePath;
    }

    private void SaveData()
    {
        // save summary data
        if (_settings.TestType == TestType.ClosedSet)
        {
            _srClosedSetData.Finish();
            //                DiagnosticsManager.Instance.CompleteTestButNoAdvance(_srClosedSetData, "Speech", _srClosedSetData.test);
        }
        else if (_settings.TestType == TestType.Matrix)
        {
            //                DiagnosticsManager.Instance.CompleteTestButNoAdvance(_srMatrixTestData, "Speech", _srMatrixTestData.test);
        }
        else
        {
            SaveOpenSetData();
        }

    }

    private void SaveOpenSetData()
    {
        var header = new BasicMeasurementFileHeader()
        {
            measurementType = _mySceneName,
            configName = _configName,
            subjectID = GameManager.Subject
        };

        string json = FileIO.JSONStringAdd("", "info", KLib.FileIO.JSONSerializeToString(header));
        json = FileIO.JSONStringAdd(json, "data", KLib.FileIO.JSONSerializeToString(_srData));
        json = FileIO.JSONStringAdd(json, "log", KLib.FileIO.JSONSerializeToString(_log.Trim()));
        json += Environment.NewLine;

        File.WriteAllText(_dataPath, json);

        HTS_Server.SendMessage(_mySceneName, $"ReceiveData:{Path.GetFileName(_dataPath)}:{File.ReadAllText(_dataPath)}");
    }

    public void ResponseAcquired()
    {
        _prompt.text = "";
        _recordPanel.Hide();

        _numSinceLastBreak++;

        // Figure out where to go next
        _plan.IncrementSentence();

        if (_abortRequested)
        {
            ProcessAbortRequest();
            return;
        }

        _progressBar.value = _plan.numSentencesDone;
        HTS_Server.SendMessage(_mySceneName, $"Progress:{_plan.PercentComplete}");

        if (_plan.currentSentenceIndex < _srList.sentences.Count && !(_settings.TestType == TestType.ClosedSet && _srClosedSetData.PassedPerformanceCriteria))
        {
            if (_settings.TestType != TestType.ClosedSet && _settings.ReviewResponses && _plan.numSentencesDone == _settings.NumToReview)
            {
                ShowInstructions(
                    "-OK, it seems like the recording is working.\n" +
                    "-We'll stop asking you to check it every time...\n" +
                    "-...but you will always have the option to re-record your response." 
                    );
            }
            else if (_settings.UseMicrophone && _provisionalResponse.volumeChanged)
            {
                HTS_Server.SendMessage(_mySceneName, $"Status: Volume changed!");
                ShowInstructions(
                    "-Please don't try to change the volume.\n" +
                    "-It may seem too low, but we set it where it will give the most meaningful results.\n" +
                    "-It's OK, just do your best!"
                    );
            }
            else if (_settings.GiveBreakEvery > 0 && _numSinceLastBreak >= _settings.GiveBreakEvery)
            {
                HTS_Server.SendMessage(_mySceneName, $"Status: Offering a break");
                _numSinceLastBreak = 0;
                ShowInstructions("-Great!\n-Take a short break if you need one.");
            }
            else
            {
                StartNextSentence();
            }
        }
        else
        {
            SaveData();

            if (_srList.listIndex >= 0)
            {
                string historyFile = Path.Combine(FileLocations.SubjectMetaFolder, _settings.TestSource + "_History.xml");
                if (File.Exists(historyFile))
                {
                    var history = FileIO.XmlDeserialize<ListHistory>(historyFile);
                    history.LastCompleted = _srList.listIndex;
                    FileIO.XmlSerialize(history, historyFile);
                }
            }

            _plan.IncrementList();

            // tests remaining?
            if (!_plan.IsFinished)
            {
                StartCoroutine(StartNextList());
            }
            else
            {
                _plan.Finish();
                EndRun(abort: false);
            }
        }
    }

    public void ListenAgain()
    {
        StartCoroutine(DoListenAgain());
    }

    IEnumerator DoListenAgain()
    {
        //if (_srList.UseMasker)
        //{
        //    masker.SetLevel(_srList.level - _srList.sentences[_qnum].SNR);
        //    masker.Play();
        //}

        //float delay_s = 0;
        //if (_srTest.MaxDelay_s > 0)
        //{
        //    delay_s = Expressions.UniformRandomNumber(_srTest.MinDelay_s, _srTest.MaxDelay_s);
        //    yield return new WaitForSeconds(delay_s);
        //}

        //// Sentence
        //_volumeManager.SetMasterVolume(_volumeAtten, VolumeManager.VolumeUnit.Decibel);

        //audioPlay.Play();
        //yield return new WaitForSeconds(audioPlay.clip.length);

        //// Post-sentence baseline
        //float wait_s = 0;
        //if (_srList.UseMasker)
        //{
        //    wait_s = _srTest.SentenceDuration_s - delay_s - audioPlay.clip.length;
        //}

        //if (wait_s > 0)
        //{
        //    yield return new WaitForSeconds(wait_s);
        //}

        //if (_srList.UseMasker)
        //{
        //    masker.Stop();
        //}
        yield return null;
    }

    void OnRecordStatusChanged(string status)
    {
        switch (status)
        {
            case "RecordingStarted":
                _log.Add($"RecordingStarted:{_plan.currentSentenceIndex}");
                break;

            case "ResponseAcquired":
                var respPath = SaveResponseClip();
                bool volumeChanged = _volumeManager.GetMasterVolume(VolumeManager.VolumeUnit.Decibel) != 0;
                _provisionalResponse = new Data.Response(
                    _srList.sentences[_plan.currentSentenceIndex].whole,
                    _srList.sentences[_plan.currentSentenceIndex].words,
                    _srList.sentences[_plan.currentSentenceIndex].SNR,
                    volumeChanged,
                    Path.GetFileName(respPath));
                break;

            case "ResponseAccepted":
                string jsonPath = Path.Combine(FileLocations.SubjectFolder, $"{_provisionalResponse.file}.json");
                HTS_Server.SendMessage(_mySceneName, $"ReceiveData:{Path.GetFileName(jsonPath)}:{File.ReadAllText(jsonPath)}");

                string wavPath = jsonPath.Replace(".json", ".wav");
                HTS_Server.SendBufferedFile(wavPath);

                _srData.responses.Add(_provisionalResponse);
                ResponseAcquired();
                break;
        }
    }

    void OnAbortAction(InputAction.CallbackContext context)
    {
        _abortAction.Disable();

        _abortProcessed = false;
        _abortRequested = true;

        if (_state == State.PlayingSentence)
        {
            Debug.Log("Abort requested");
            HTS_Server.SendMessage(_mySceneName, $"Status: Abort requested");
        }
        else
        {
            ProcessAbortRequest();
        }
    }

    void ProcessAbortRequest()
    {
        if (_abortProcessed) return;

        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(false);
        _recordPanel.Abort();

        if (!_isRemote)
        {
            _quitPanel.SetActive(true);
        }
        else
        {
            EndRun(abort: true);
            _abortProcessed = true;
        }
    }

    public void OnQuitConfirmButtonClick()
    {
        _quitPanel.SetActive(false);
        EndRun(abort: true);
        _abortProcessed = true;
    }

    public void OnQuitCancelButtonClick()
    {
        _abortRequested = false;
        _quitPanel.SetActive(false);
        _abortAction.Enable();

        if (_state == State.Instructions)
            _instructionPanel.gameObject.SetActive(true);
        else
            StartNextSentence();
    }

    private void ShowInstructions(string instructions)
    {
        _state = State.Instructions;
        HTS_Server.SendMessage(_mySceneName, $"Status: Showing instructions");

        _workPanel.gameObject.SetActive(false);
        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = StartNextSentence;
        _instructionPanel.ShowInstructions(
            new Turandot.Instructions() 
            { 
                Text = instructions, 
                FontSize = _settings.InstructionFontSize,
                VerticalAlignment = Turandot.Instructions.VerticalTextAlignment.Middle,
                LineSpacing = 2
            });
    }

    private void EndRun(bool abort)
    {
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.SetActive(false);

        if (abort)
        {
            SaveData();
        }

        string status = abort ? "Measurement aborted" : "Measurement finished";
        HTS_Server.SendMessage(_mySceneName, $"Finished:{status}");

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
        _instructionPanel.gameObject.SetActive(false);
        _workPanel.gameObject.SetActive(false);
        _recordPanel.gameObject.SetActive(false);

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
                OnAbortAction(new InputAction.CallbackContext());
                break;
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

#if KDEBUG
    private IEnumerator AutoAdvance(KEventDelegate onAdvance)
    {
        yield return new WaitForSeconds(KDebug.Settings.advanceDelay_s);
        onAdvance();
    }
    private void AutoRespond()
    {
        if (_useClosedSet)
        {
            _srClosedSetData.AddResponse(_srList.sentences[_qnum].whole, "hood", _srList.sentences[_qnum].SNR, false);
        }
        else if (_useMatrixTest)
        {
            string value = _matrixTestController.Simulate(_srList.sentences[_qnum].words, _srList.matrixTest.SNR);
            int nc = _srMatrixTestData.AddResponse(_srList.sentences[_qnum].whole, value, _srList.matrixTest.StimLevel, _srList.matrixTest.maskerLevel, false);
            _srList.matrixTest.UpdateSNR(nc);
        }
        else
        {
            var response = new SpeechReception.Data.Response(_srList.sentences[_qnum].whole, _srList.sentences[_qnum].words, _srList.sentences[_qnum].SNR, false, "");
            FileIO.AppendTextFile(_dataPath, FileIO.JSONSerializeToString(response));
        }
        _responseAccepted = true;
    }

    IEnumerator SimulateAcquireAudioResponse()
    {
        _responseAttempt = 1;
        _responseAccepted = false;

        if (_srTest.AudioCues)
        {
            yield return new WaitForSeconds(0.5f);
            _recordingState = RecordingState.Start;
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (IPC.Instance.Use) IPC.Instance.SendCommand("Response", _qnum.ToString());

        yield return new WaitForSeconds(1.0f);
        FileIO.AppendTextFile(_dataPath, FileIO.JSONSerializeToString(_tentativeResponse));

        commonUI.IncrementProgressBar();
        ResponseAcquired();
    }

#endif

}
