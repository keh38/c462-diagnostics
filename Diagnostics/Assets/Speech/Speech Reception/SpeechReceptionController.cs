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

public class SpeechReceptionController : MonoBehaviour, IRemoteControllable
{
    [Header("UI Elements")]
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

    private bool _stopMeasurement = false;
    private bool _localAbort = false;

    private SpeechTest _settings = null;

    private string _dataPath;
    private string _mySceneName = "SpeechReception";
    private string _configName;

    private InputAction _abortAction;

    private TestPlan _plan;

    private AudioSource _audioPlay;
    private AudioSource _audioAlert;

    private ListDescription _srItems;
    private ListProperties _srList;
    private Data _srData;
    private Data.Response _tentativeResponse;
    private int _qnum = -1;

    private float _volumeAtten;
    private VolumeManager _volumeManager;

    private int _numListsCompleted;

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

        _recordPanel.AudioCuesOnly = _settings.AudioCuesOnly;

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
            if (testEar == SpeechReception.TestEar.SubjectDefault)
            {
                string subjectDefault = GameManager.Metrics["TestEar"];

                if (string.IsNullOrEmpty(subjectDefault))
                    throw new Exception("Subject default test ear not set");
                else if (subjectDefault == "Left")
                    testEar = SpeechReception.TestEar.Left;
                else if (subjectDefault == "Right")
                    testEar = SpeechReception.TestEar.Right;
                else if (subjectDefault == "Binaural")
                    testEar = SpeechReception.TestEar.Binaural;
                else
                    throw new Exception("Subject default test ear not set properly");

            }

            var t = _settings.Clone();
            t.Initialize(testEar, customizations.Get(_settings.TestSource));
            _plan.tests.Add(t);
        }
    }

    private void Begin()
    {
        _localAbort = false;
        _stopMeasurement = false;

        _numListsCompleted = 0;
        _numSinceLastBreak = 0;

        StartCoroutine(StartNextList());
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

        _audioPlay.panStereo = _srList.TestEar.ToBalance();

        _audioPlay.volume = Mathf.Pow(10, atten / 20);
        _audioAlert.volume = Mathf.Pow(10, (atten - 10) / 20);
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
        _srList.ApplySequence();

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
        if (_settings.TestType != TestType.QuickSIN && _srList.UseMasker)
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

        // Display instructions if needed
        var instructions = GetInstructions(_plan.currentListIndex);
        if (false && !string.IsNullOrEmpty(instructions))
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

    public void StartNextSentence()
    {
        _instructionPanel.gameObject.SetActive(false);

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

        string wavfile = _srList.sentences[_qnum].wavfile;

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
            _volumeAtten = SetLevel(_srList.MatrixTest.StimLevel, _srList.Units, _srList.TestEar);
        }

        _log.Add($"Sentence:{_qnum}");
        HTS_Server.SendMessage(_mySceneName, $"Status: Sentence {_qnum}...");

        if (_settings.TestType != TestType.QuickSIN && _srList.UseMasker)
        {
            if (_settings.TestType == TestType.Matrix)
            {
                _masker.SetLevel(_srList.MatrixTest.MaskerLevel);
            }
            else
            {
                _masker.SetLevel(_srList.Level - _srList.sentences[_qnum].SNR);
            }
            _log.Add($"MaskerOff:{_qnum}");
            _masker.Play();
        }

        float delay_s = 0;
        if (_settings.MaxDelay_s > 0)
        {
            Debug.Log("wtf");
            delay_s = Expressions.UniformRandomNumber(_settings.MinDelay_s, _settings.MaxDelay_s);
            yield return new WaitForSeconds(delay_s);
        }

        // Sentence
        _volumeManager.SetMasterVolume(_volumeAtten, VolumeManager.VolumeUnit.Decibel);

        _log.Add($"SentenceStart:{_qnum}");

        //_audioPlay.clip = www.GetAudioClip(false, false, AudioType.WAV);
        _audioPlay.Play();
        
        yield return new WaitForSeconds(_audioPlay.clip.length);

        _log.Add($"SentenceEnd:{_qnum}");

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
            _log.Add($"MaskerOff:{_qnum}");
        }

        _fixationPoint.SetActive(false);

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
            var reviewThisOne = _settings.ReviewResponses && _qnum < _settings.NumToReview;
            _recordPanel.AcquireAudioResponse(reviewThisOne);
        }
    }

    public void ResponseAcquired()
    {
        //// Clear our work from the screen
        //commonUI.ShowPrompt("");
        //prompt.text = "";
        //NGUITools.SetActive(MicSprite.gameObject, false);
        //NGUITools.SetActive(OKButton.gameObject, false);
        //NGUITools.SetActive(RedoButton.gameObject, false);
        //NGUITools.SetActive(RepeatButton.gameObject, false);
        //NGUITools.SetActive(ContinueButton.gameObject, false);

        //_numSinceLastBreak++;

        //// Figure out where to go next
        //if (++_qnum < _srList.sentences.Count && !(_useClosedSet && _srClosedSetData.PassedPerformanceCriteria))
        //{
        //    if (!_useClosedSet && _reviewResponses && _qnum == _srTest.NumToReview)
        //    {
        //        _reviewResponses = false;
        //        Vector3 delta = _recordButtonTween.to - _recordButtonTween.from;
        //        _recordButtonTween.from = new Vector3(0, 110);
        //        _recordButtonTween.to = _recordButtonTween.from + delta;
        //        _recordButtonTween.enabled = true;

        //        StartCoroutine(ShowEndReviewInstructions());
        //    }
        //    else
        //    {
        //        // go to next sentence
        //        if (_srTest.GiveBreakEvery > 0 && _numSinceLastBreak >= _srTest.GiveBreakEvery)
        //        {
        //            _numSinceLastBreak = 0;
        //            commonUI.FormatHelpBox("Great!\nTake a short break if you need one.");
        //            commonUI.ShowNextButton("Press Next to continue", StartNextSentence);
        //        }
        //        else
        //        {
        //            StartNextSentence();
        //        }
        //        //commonUI.ShowNextButton("Press Next to continue", StartNextSentence);
        //    }
        //}
        //else
        //{
        //    if (IPC.Instance.Use) IPC.Instance.StopRecording();

        //    // save summary data
        //    if (_useClosedSet)
        //    {
        //        _srClosedSetData.Finish();
        //        DiagnosticsManager.Instance.CompleteTestButNoAdvance(_srClosedSetData, "Speech", _srClosedSetData.test);
        //    }
        //    else if (_useMatrixTest)
        //    {
        //        DiagnosticsManager.Instance.CompleteTestButNoAdvance(_srMatrixTestData, "Speech", _srMatrixTestData.test);
        //    }
        //    else
        //    {
        //        DiagnosticsManager.Instance.EndTest(_dataPath);
        //        _dataPath = null;
        //    }
        //    _numListsCompleted++;

        //    if (_srList.listIndex >= 0)
        //    {
        //        string historyFile = FileIO.CombinePaths(DataFileLocations.SubjectMetaFolder, _srList.TestType + "_History.xml");
        //        if (File.Exists(historyFile))
        //        {
        //            var history = FileIO.XmlDeserialize<ListHistory>(historyFile);
        //            history.LastCompleted = _srList.listIndex;
        //            FileIO.XmlSerialize(history, historyFile);
        //        }
        //    }

        //    // tests remaining?
        //    if (_srTest.Lists.Count > 0)
        //    {
        //        StartCoroutine(StartNextList());
        //    }
        //    else
        //    {
        //        commonUI.ShowHelpBox("Excellent!");
        //        commonUI.ShowNextButton("Press Next to start the next task", Return);
        //    }
        //}
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
                _log.Add($"RecordingStarted:{_qnum}");
                break;
            case "ResponseAccepted":
                break;
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

    private void ShowInstructions(string instructions)
    {
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
