using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_METRO && !UNITY_EDITOR
using LegacySystem.IO;
#else
using System.IO;
#endif

using ExtensionMethods;
using KLib;
using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;
using SpeechReception;

#if KDEBUG
using DateTime = KDebug.Settings;
#endif

/// <summary>
/// Procedure: 
/// 1. Help/instructions (with skip option)
/// 2. Three practice tests, in random order
/// 3. Parse "settings file" to determine which real tests to do
/// </summary>
public class SpeechReceptionTester : MonoBehaviour 
{
/*    public DiagnosticsUI commonUI;
    public UISprite EarSprite;
    public UISprite MicSprite;
    public UITweener MicTweener;
    public UISlider progressBar;

    public UIButton OKButton;
    public UIButton RedoButton;
    public UIButton RepeatButton;
    public UIButton ContinueButton;
    public UIButton RecordButton;
    public UIButton StopButton;

    public UILabel prompt;

    public AudioSource audioPlay;
    public AudioSource audioRecord;
    public AudioSource audioAlert;

    public AudioClip recordStartClip;
    public AudioClip recordEndClip;

    public SpeechMasker masker;

    public ClosedSetController closedSetController;
    public MatrixTestController _matrixTestController;
    public GameObject _topFiller;

    private SpeechReception.ListDescription _srItems;
    private ListProperties _srList;
    private SpeechReception.Test _srTest;
    private SpeechReception.Data _srData;
    private Data.Response _tentativeResponse;
    private string _dataPath;
    private int _qnum = -1;
    private int _responseAttempt;
    private bool _responseAccepted;

    private enum RecordingState {Waiting, Start, Recording, TimedOut, Stop, StopAndRedo, StopAndContinue, Validating};
    private RecordingState _recordingState;

    private UILabel _OKButtonLabel;
    private TweenPosition _recordButtonTween;
    private TweenPosition _stopButtonTween;
    private Color _buttonColor;

    private static float sMaxRecordTime_sec = 10;

    private bool _reviewResponses = false;
    private static readonly int maxNumRecordAttempts = 3;

    //private static readonly float Output_dBHL = 70;

    private float _volumeAtten;
    private VolumeManager _volumeManager;

    private int _numListsCompleted;

    private KEventDelegate _onFinished;

    private Instructions _instructions;

    private string _testXmlFile;

    private bool _recordButtonPressed;
    private bool _stopButtonPressed;
    private bool _rerecordButtonPressed;
    private bool _itsGoodButtonPressed;

    private int _numPracticeLists;
    private int _numSinceLastBreak;

    private bool _useClosedSet = false;
    private bool _useMatrixTest = false;
    private ClosedSetData _srClosedSetData = new ClosedSetData();
    private MatrixTestData _srMatrixTestData = new MatrixTestData();

    private string _transducer;
    private UserCustomizations _customizations;

    private float _pupillometryBaseline = 2;

    void Awake()
    {
        _matrixTestController = GameObject.Find("Matrix Test").GetComponent<MatrixTestController>();

        _topFiller = GameObject.Find("Filler/Top");

        _recordButtonTween = RecordButton.gameObject.GetComponentInChildren<UITweener>() as TweenPosition;
        _recordButtonTween.from = RecordButton.transform.localPosition;
        _recordButtonTween.to += _recordButtonTween.from;

        _stopButtonTween = StopButton.gameObject.GetComponentInChildren<UITweener>() as TweenPosition;
        _stopButtonTween.from = StopButton.transform.localPosition;
        _stopButtonTween.to += _stopButtonTween.from;

        _buttonColor = GameObject.Find("Record Button/Background").GetComponentInChildren<UISprite>().color;

        _OKButtonLabel = OKButton.gameObject.GetComponentInChildren<UILabel>();
    }

    void Start() 
    {
        NGUITools.SetActive(EarSprite.gameObject, false);
        NGUITools.SetActive(MicSprite.gameObject, false);
        NGUITools.SetActive(prompt.gameObject, false);
        NGUITools.SetActive(RepeatButton.gameObject, false);
        NGUITools.SetActive(progressBar.gameObject, false);
        NGUITools.SetActive(OKButton.gameObject, false);
        NGUITools.SetActive(RedoButton.gameObject, false);
        NGUITools.SetActive(RecordButton.gameObject, false);
        NGUITools.SetActive(StopButton.gameObject, false);
        NGUITools.SetActive(ContinueButton.gameObject, false);

        commonUI.SetParentObject(this.gameObject);
        commonUI.SetHelpBalloonPosition(Vector3.zero);

        closedSetController.OnSelectionMade = OnClosedSetResponse;
        _matrixTestController.OnSelectionMade = OnMatrixTestResponse;

        if (IPC.Instance.Use && !IPC.Instance.Connect())
        {
            throw new Exception("Error connecting to IPC interface.");
        }

        _volumeManager = new VolumeManager();
        Debug.Log(_volumeManager);
    }

    public KEventDelegate OnFinished
    {
        set { _onFinished = value;}
    }

    public void RunTest(SpeechReception.Test srt, bool responsePlayback)
    {
        _transducer = SubjectManager.Instance.Transducer;
        _customizations = UserCustomizations.Initialize(DataFileLocations.SubjectCustomSpeechPath, SubjectManager.CurrentSubject);

        _srTest = srt;

        if (_srTest.NumToReview == 0)
        {
            Vector3 delta = _recordButtonTween.to - _recordButtonTween.from;
            _recordButtonTween.from = new Vector3(0, 110);
            _recordButtonTween.to = _recordButtonTween.from + delta;
            _recordButtonTween.enabled = true;
        }

        _reviewResponses = responsePlayback && _srTest.NumToReview > 0;
        _numListsCompleted = 0;
        _numSinceLastBreak = 0;

#if KDEBUG
            if (KDebug.Settings.active && KDebug.Settings.data == KDebug.Data.InAndOut)
            {
                StartCoroutine(InAndOut());
                return;
            }
#endif
        StartCoroutine(StartNextList());
    }

    private float SetLevel(float level, SpeechReception.LevelUnits units, Laterality laterality)
    {
        float dB_wavfile_fullscale = float.NaN;

        float ref_dB = _srTest.GetReference(_transducer, units);
        //float ref_dB = _srTest.GetReference(units);

        dB_wavfile_fullscale = ref_dB;
        Debug.Log("dB_wavfile_fullscale" + " = " + dB_wavfile_fullscale);

        float atten = level - dB_wavfile_fullscale;
        Debug.Log("atten" + " = " + atten);
        atten = Math.Min(0f, atten);

        audioPlay.volume = Mathf.Pow(10, atten / 20);
        audioAlert.volume = Mathf.Pow(10, (atten - 10) / 20);
        audioPlay.panStereo = laterality.ToBalance();

        audioPlay.volume = Mathf.Pow(10, atten / 20);
        audioAlert.volume = Mathf.Pow(10, (atten - 10) / 20);
        atten = 0;

        _volumeManager.SetMasterVolume(atten, VolumeManager.VolumeUnit.Decibel);

        return atten;
    }

    public void Return()
    {
        _onFinished();
    }

    public IEnumerator StartNextList()
    {
        commonUI.ShowProgressBar(true);
        commonUI.HideHelpButton();
        commonUI.HideHelpBox();

        // Pop next test off the stack
        Debug.Log(_srTest.Lists[0].file);
        _srList = _srTest.Lists[0];
        if (_srList.laterality == Laterality.Unspecified)
        {
            _srList.laterality = SubjectManager.Instance.Laterality;
        }

        //var c = _customizations.Get(_srTest.TestType);
        //if (_srTest.Lists[0].applyCustom && c != null)
        //{
        //    _srList.level = c.level;
        //    _srList.SNRs = c.snr;
        //}

        _useClosedSet = _srList.closedSet != null && _srList.closedSet.active;
        _useMatrixTest = _srList.closedSet == null && _srList.matrixTest != null && _srList.matrixTest.active;
        if (_useClosedSet)
        {
            closedSetController.Initialize(_srList.closedSet, _srList.GetClosedSetResponses());
        }
        else if (_useMatrixTest)
        {
            NGUITools.SetActive(_topFiller, false);
            _matrixTestController.Initialize();
            _srList.matrixTest.Initialize();
        }

        _srList.SetSequence();

        _volumeAtten = SetLevel(_srList.level, _srList.units, _srList.laterality);

        if (IPC.Instance.Use)
        {
            IPC.Instance.StartRecording(SubjectManager.CurrentSubject + "-" + _srTest.Lists[0].file);
            IPC.Instance.SendCommand("State", "0");

            yield return new WaitForSeconds(1f);
        }

        _srTest.Lists.RemoveAt(0);

        // Initialize summary data object
        if (_useClosedSet)
        {
            _srClosedSetData = new ClosedSetData(
                _numListsCompleted < _srTest.NumPracticeLists,
                _srList.sequence.numBlocks,
                _srList.sequence.ItemsPerBlock,
                _srList.closedSet.performanceCriteria);

            _srClosedSetData.date = DateTime.Now.ToString();
            _srClosedSetData.test = _srTest.TestType + "-" + _srList.Title;
            _srClosedSetData.laterality = _srList.laterality;
            _srClosedSetData.runNumber = SubjectManager.Instance.PeekDiagnosticsRun();
        }
        else if (_useMatrixTest)
        {
            _srMatrixTestData = new MatrixTestData(_srList.matrixTest.stimLevel, _srList.matrixTest.maskerLevel, _srList.matrixTest.mode);

            _srMatrixTestData.date = DateTime.Now.ToString();
            _srMatrixTestData.test = _srTest.TestType + "-" + _srList.Title;
            _srMatrixTestData.laterality = _srList.laterality;
            _srMatrixTestData.runNumber = SubjectManager.Instance.PeekDiagnosticsRun();
        }
        else
        {
            _srData = new SpeechReception.Data(_numListsCompleted < _srTest.NumPracticeLists);
            _srData.date = DateTime.Now.ToString();
            _srData.test = _srTest.TestType + "-" + _srList.Title;
            _srData.Fs = 22050;
            _srData.runNumber = SubjectManager.Instance.PeekDiagnosticsRun();
            _srData.laterality = _srList.laterality;
            _dataPath = DiagnosticsManager.Instance.StartTest(_srData, "Speech", _srData.test);
        }

        _qnum = 0;
        if (_srList.UseMasker)
        {
            float maskerLevel = _srList.level - _srList.sentences[_qnum].SNR;
            if (_useMatrixTest) maskerLevel = _srList.matrixTest.maskerLevel;
            yield return StartCoroutine(masker.Initialize(
                _srList.masker,
                maskerLevel,
                _transducer,
                _srList.units,
                _srList.laterality));
        }

        // Display message, if appropriate
        var instructionsPath = _srTest.Instructions == null ? null : _srTest.Instructions.Find(_numListsCompleted);

        if (!string.IsNullOrEmpty(instructionsPath))
        {
            ShowInstructions(instructionsPath);
        }
        else if (_numListsCompleted > 0)
        {
            commonUI.FormatHelpBox("Great!\nLet's try some more.");
            commonUI.ShowNextButton("Press Next to continue", StartNextSentence);
        }
        else
        {
            StartNextSentence();
        }
    }

    public void StartNextSentence()
    {
        commonUI.HideHelpBox();

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
#endif

    private IEnumerator DoSentence()
    {
        NGUITools.SetActive(EarSprite.gameObject, true);
        NGUITools.SetActive(prompt.gameObject, true);
        prompt.text = "Listen";

        string wavfile = _srList.sentences[_qnum].wavfile;

        Debug.Log(_qnum + ": " + wavfile);

        WWW www = new WWW("file:///" + FileIO.CombinePaths(DataFileLocations.SpeechWavFolder, _srTest.TestType, wavfile));
        while (!www.isDone)
            yield return null;

        if (_useMatrixTest)
        {
            _volumeAtten = SetLevel(_srList.matrixTest.StimLevel, _srList.units, _srList.laterality);
        }

        if (IPC.Instance.Use) IPC.Instance.SendCommand("Sentence", _qnum.ToString());

        // Pre-sentence baseline
        if (PupillometerSettings.Restore().Use)
        {
            yield return new WaitForSeconds(_pupillometryBaseline);
        }

        if (_srList.UseMasker)
        {
            if (_useMatrixTest)
            {
                masker.SetLevel(_srList.matrixTest.maskerLevel);
            }
            else
            {
                masker.SetLevel(_srList.level - _srList.sentences[_qnum].SNR);
            }
            if (IPC.Instance.Use) IPC.Instance.SendCommand("MaskerOn", _qnum.ToString());
            masker.Play();
        }

        float delay_s = 0;
        if (_srTest.MaxDelay_s > 0)
        {
            delay_s = Expressions.UniformRandomNumber(_srTest.MinDelay_s, _srTest.MaxDelay_s);
            yield return new WaitForSeconds(delay_s);
        }

        // Sentence
        _volumeManager.SetMasterVolume(_volumeAtten, VolumeManager.VolumeUnit.Decibel);

        if (IPC.Instance.Use) IPC.Instance.SendCommand("SentenceStart", _qnum.ToString());
        audioPlay.clip = www.GetAudioClip(false, false, AudioType.WAV);
        audioPlay.Play();
        yield return new WaitForSeconds(audioPlay.clip.length);
        if (IPC.Instance.Use) IPC.Instance.SendCommand("SentenceEnd", _qnum.ToString());

        // Post-sentence baseline
        float wait_s = 0;
        if (PupillometerSettings.Restore().Use)
        {
            wait_s = 3f; // _pupillometryBaseline;
        }
        else if (_srList.UseMasker)
        {
            wait_s = _srTest.SentenceDuration_s - delay_s - audioPlay.clip.length;
        }

        if (wait_s > 0)
        {
            yield return new WaitForSeconds(wait_s);
        }

        if (_srList.UseMasker)
        {
            masker.Stop();
            if (IPC.Instance.Use) IPC.Instance.SendCommand("MaskerOff", _qnum.ToString());
        }

        if (PupillometerSettings.Restore().Use)
        {
            yield return new WaitForSeconds(_pupillometryBaseline);
        }

        NGUITools.SetActive(EarSprite.gameObject, false);

        if (_useClosedSet)
        {
            StartCoroutine(AcquireClosedSetResponse());
        }
        else if (_useMatrixTest)
        {
            StartCoroutine(AcquireMatrixTestResponse());
        }
        else
        {
            if (_srTest.debug)
            {
                StartCoroutine(SimulateAcquireAudioResponse());
            }
            else
            {
                StartCoroutine(AcquireAudioResponse());
            }
        }
    }

    IEnumerator AcquireAudioResponse()
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
            yield return StartCoroutine(WaitForRecordingStart());
        }

        if (IPC.Instance.Use) IPC.Instance.SendCommand("Response", _qnum.ToString());

        StartCoroutine(RecordResponse());

        while (!_responseAccepted)
        {
            yield return null;
        }

        FileIO.AppendTextFile(_dataPath, FileIO.JSONSerializeToString(_tentativeResponse));

        if (_tentativeResponse.volumeChanged)
        {
            yield return StartCoroutine(ShowVolumeWarning());
        }
        commonUI.IncrementProgressBar();
        ResponseAcquired();
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

    IEnumerator AcquireClosedSetResponse()
    {
        prompt.text = "What did you hear?";
        closedSetController.Show();

        yield break;
    }

    IEnumerator AcquireMatrixTestResponse()
    {
        prompt.text = "";
        commonUI.EnableHomeButton(false);

        _matrixTestController.Show();

        yield break;
    }

    private void OnClosedSetResponse(string value)
    {
        bool volumeChanged = _volumeManager.GetMasterVolume(VolumeManager.VolumeUnit.Decibel) != _volumeAtten;
        Debug.Log("Response = " + value);

        _srClosedSetData.AddResponse(_srList.sentences[_qnum].whole, value, _srList.sentences[_qnum].SNR, volumeChanged);

        prompt.text = "";

        if (_srList.ShowFeedback)
        {
            bool correct = _srList.sentences[_qnum].whole.Equals(value);
            closedSetController.ShowFeedback(correct, _srList.sentences[_qnum].whole);

            if (correct)
            {
                StartCoroutine(ProcessClosedSetResponse(volumeChanged));
            }
        }
        else
        {
            StartCoroutine(ProcessClosedSetResponse(volumeChanged));
        }
    }

    private void OnMatrixTestResponse(string value)
    {
        bool volumeChanged = _volumeManager.GetMasterVolume(VolumeManager.VolumeUnit.Decibel) != _volumeAtten;
        Debug.Log("Response = " + value);

        int nc = _srMatrixTestData.AddResponse(_srList.sentences[_qnum].whole, value, _srList.matrixTest.StimLevel, _srList.matrixTest.maskerLevel, volumeChanged);
        _srList.matrixTest.UpdateSNR(nc);

        prompt.text = "";
        commonUI.EnableHomeButton(true);

        StartCoroutine(ProcessMatrixTestResponse(volumeChanged));
    }

    IEnumerator ProcessClosedSetResponse(bool volumeWarning)
    {
        yield return new WaitForSeconds(_srList.closedSet.feedback == ClosedSet.FeedbackType.Subject ? 2.0f : 0.5f);

        closedSetController.Hide();

        if (volumeWarning)
        {
            yield return StartCoroutine(ShowVolumeWarning());
        }
        commonUI.IncrementProgressBar();
        ResponseAcquired();
    }

    IEnumerator ProcessMatrixTestResponse(bool volumeWarning)
    {
        yield return new WaitForSeconds(0.5f);

        _matrixTestController.Hide();

        if (volumeWarning)
        {
            yield return StartCoroutine(ShowVolumeWarning());
        }
        commonUI.IncrementProgressBar();
        ResponseAcquired();
    }

    public void ListenAgain()
    {
        StartCoroutine(DoListenAgain());
    }

    IEnumerator DoListenAgain()
    {
        if (_srList.UseMasker)
        {
            masker.SetLevel(_srList.level - _srList.sentences[_qnum].SNR);
            masker.Play();
        }

        float delay_s = 0;
        if (_srTest.MaxDelay_s > 0)
        {
            delay_s = Expressions.UniformRandomNumber(_srTest.MinDelay_s, _srTest.MaxDelay_s);
            yield return new WaitForSeconds(delay_s);
        }

        // Sentence
        _volumeManager.SetMasterVolume(_volumeAtten, VolumeManager.VolumeUnit.Decibel);

        audioPlay.Play();
        yield return new WaitForSeconds(audioPlay.clip.length);

        // Post-sentence baseline
        float wait_s = 0;
        if (_srList.UseMasker)
        {
            wait_s = _srTest.SentenceDuration_s - delay_s - audioPlay.clip.length;
        }

        if (wait_s > 0)
        {
            yield return new WaitForSeconds(wait_s);
        }

        if (_srList.UseMasker)
        {
            masker.Stop();
        }
    }

    public void ContinueClosedSet()
    {
        StartCoroutine(DelayAndContinue());
    }

    IEnumerator DelayAndContinue()
    {
        closedSetController.Hide();

        yield return new WaitForSeconds(0.5f);

        commonUI.IncrementProgressBar();
        ResponseAcquired();
    }

    IEnumerator WaitForRecordingStart()
    {
        _recordingState = RecordingState.Waiting;

        NGUITools.SetActive(prompt.gameObject, true);
        prompt.text = "What did you hear?";

        RecordButton.transform.localPosition = _recordButtonTween.from;
        StopButton.transform.localPosition = _stopButtonTween.from;

        NGUITools.SetActive(RecordButton.gameObject, true);
        _recordButtonPressed = false;
        KLib.Unity.SetButtonState(RecordButton, true, _buttonColor);

        if (ReviewThisOne())
        {
            NGUITools.SetActive(StopButton.gameObject, true);
            _stopButtonPressed = false;
            KLib.Unity.SetButtonState(StopButton, true, _buttonColor);
            KLib.Unity.DisableButton(StopButton);
        }
        else
        {
            NGUITools.SetActive(StopButton.gameObject, false);
        }

        while (_recordingState != RecordingState.Start)
        {
            yield return null;
        }

        if (ReviewThisOne())
        {
            KLib.Unity.SetButtonState(RecordButton, false);
            KLib.Unity.SetButtonState(StopButton, true, _buttonColor);
        }
        else
        {
            NGUITools.SetActive(RecordButton.gameObject, false);
        }
    }

    IEnumerator RecordResponse()
    {
        commonUI.ShowPrompt("");
        NGUITools.SetActive(RepeatButton.gameObject, false);
        NGUITools.SetActive(ContinueButton.gameObject, false);

        bool reviewThisOne = ReviewThisOne();
        NGUITools.SetActive(OKButton.gameObject, !reviewThisOne);
        NGUITools.SetActive(RedoButton.gameObject, !reviewThisOne);
        if (!reviewThisOne)
        {
            _itsGoodButtonPressed = false;
            _rerecordButtonPressed = false;
        }


        _OKButtonLabel.text = (ReviewThisOne()) ? "It's good!" : "Done";

        NGUITools.SetActive(MicSprite.gameObject, true);
        NGUITools.SetActive(prompt.gameObject, true);
        prompt.text = "What did you hear?";
        NGUITools.SetActive(progressBar.gameObject, true);
        MicTweener.enabled = true;

        _recordingState = RecordingState.Recording;

        audioRecord.clip = Microphone.Start(null, false, (int) (sMaxRecordTime_sec), _srData.Fs);

        audioAlert.clip = recordStartClip;
        audioAlert.Play();

        float progressBarSamples = (float)(audioRecord.clip.samples);

        progressBar.value = 1.0f;
        while (Microphone.IsRecording(null) && _recordingState==RecordingState.Recording)
        {
            yield return new WaitForSeconds(0.1f);
            progressBar.value = Microphone.IsRecording(null) ? Mathf.Max(0f, 1f - (float)(Microphone.GetPosition(null) / progressBarSamples)) : 0;
        }

        if (_recordingState == RecordingState.Recording)
        {
            _recordingState = RecordingState.TimedOut;
        } 
        if (_recordingState == RecordingState.Stop || _recordingState == RecordingState.StopAndContinue)
        {
            yield return new WaitForSeconds(0.5f);
        }

        int endPosition = Microphone.GetPosition(null);
        Microphone.End(null);

        KLib.Unity.SetButtonState(StopButton, false);

        // Trim response, if needed
        if (endPosition > 0 && endPosition < audioRecord.clip.samples)
        {
            float[] data = new float[endPosition];
            audioRecord.clip.GetData(data, 0);
            audioRecord.clip = AudioClip.Create("clip", data.Length, 1, _srData.Fs, false, false);
            audioRecord.clip.SetData(data, 0);
        }

        audioAlert.clip = recordEndClip;
        audioAlert.Play();

        //progressBar.value = 0;
        MicTweener.enabled = false;

        bool volumeChanged = _volumeManager.GetMasterVolume(VolumeManager.VolumeUnit.Decibel) != _volumeAtten;
        string respPath = SaveResponseClip(_srData.runNumber, _srData.test, _responseAttempt);

        _tentativeResponse = new SpeechReception.Data.Response(_srList.sentences[_qnum].whole, _srList.sentences[_qnum].words, _srList.sentences[_qnum].SNR, volumeChanged, respPath);

        if (ReviewThisOne())
        {
            _recordingState = (_responseAttempt < maxNumRecordAttempts) ? RecordingState.Validating : RecordingState.StopAndContinue;
        }

        StartCoroutine(DisposeOfResponse());
    }

    private IEnumerator DisposeOfResponse()
    {
        switch (_recordingState)
        {
            case RecordingState.Validating:
                NGUITools.SetActive(prompt.gameObject, true);
                prompt.text = "Check your response";
                
                audioRecord.Play();
                while (audioRecord.isPlaying)
                {
                    progressBar.value = 1f - (float)(audioRecord.time) / audioRecord.clip.length;
                    yield return new WaitForSeconds(0.1f);
                }
                progressBar.value = 0;
                
                NGUITools.SetActive(MicSprite.gameObject, false);
                NGUITools.SetActive(prompt.gameObject, false);
                NGUITools.SetActive(progressBar.gameObject, false);
                NGUITools.SetActive(RecordButton.gameObject, false);
                NGUITools.SetActive(StopButton.gameObject, false);
                
                commonUI.ShowPrompt("Was your response recorded OK?");
                NGUITools.SetActive(OKButton.gameObject, true);
                NGUITools.SetActive(RedoButton.gameObject, true);
                _itsGoodButtonPressed = false;
                _rerecordButtonPressed = false;
                break;

            case RecordingState.StopAndContinue:
                yield return new WaitForSeconds(0.5f);
                NGUITools.SetActive(MicSprite.gameObject, false);
                NGUITools.SetActive(prompt.gameObject, false);
                NGUITools.SetActive(progressBar.gameObject, false);
                NGUITools.SetActive(RecordButton.gameObject, false);
                NGUITools.SetActive(StopButton.gameObject, false);
                _responseAccepted = true;
                break;

            case RecordingState.StopAndRedo:
                yield return new WaitForSeconds(0.25f);
                ++_responseAttempt;
                StartCoroutine(RecordResponse());
                break;

            case RecordingState.TimedOut:
                NGUITools.SetActive(prompt.gameObject, false);
                NGUITools.SetActive(progressBar.gameObject, false);
                NGUITools.SetActive(OKButton.gameObject, false);
                NGUITools.SetActive(RedoButton.gameObject, false);
                NGUITools.SetActive(RepeatButton.gameObject, true);
                NGUITools.SetActive(ContinueButton.gameObject, true);
                _rerecordButtonPressed = false;
                _itsGoodButtonPressed = false;
                break;
        }
    }

    public void ResponseAcquired()
    {
        // Clear our work from the screen
        commonUI.ShowPrompt("");
        prompt.text = "";
        NGUITools.SetActive(MicSprite.gameObject, false);
        NGUITools.SetActive(OKButton.gameObject, false);
        NGUITools.SetActive(RedoButton.gameObject, false);
        NGUITools.SetActive(RepeatButton.gameObject, false);
        NGUITools.SetActive(ContinueButton.gameObject, false);

        _numSinceLastBreak++;

        // Figure out where to go next
        if (++_qnum < _srList.sentences.Count && !(_useClosedSet && _srClosedSetData.PassedPerformanceCriteria))
        {
            if (!_useClosedSet && _reviewResponses && _qnum==_srTest.NumToReview)
            {
                _reviewResponses = false;
                Vector3 delta = _recordButtonTween.to - _recordButtonTween.from;
                _recordButtonTween.from = new Vector3(0, 110);
                _recordButtonTween.to = _recordButtonTween.from + delta;
                _recordButtonTween.enabled = true;

                StartCoroutine(ShowEndReviewInstructions());
            }
            else
            {
                // go to next sentence
                if (_srTest.GiveBreakEvery > 0 && _numSinceLastBreak >= _srTest.GiveBreakEvery)
                {
                    _numSinceLastBreak = 0;
                    commonUI.FormatHelpBox("Great!\nTake a short break if you need one.");
                    commonUI.ShowNextButton("Press Next to continue", StartNextSentence);
                }
                else
                {
                    StartNextSentence();
                }
                //commonUI.ShowNextButton("Press Next to continue", StartNextSentence);
            }
        }
        else
        {
            if (IPC.Instance.Use) IPC.Instance.StopRecording();

            // save summary data
            if (_useClosedSet)
            {
                _srClosedSetData.Finish();
                DiagnosticsManager.Instance.CompleteTestButNoAdvance(_srClosedSetData, "Speech", _srClosedSetData.test);
            }
            else if (_useMatrixTest)
            {
                DiagnosticsManager.Instance.CompleteTestButNoAdvance(_srMatrixTestData, "Speech", _srMatrixTestData.test);
            }
            else
            {
                DiagnosticsManager.Instance.EndTest(_dataPath);
                _dataPath = null;
            }
            _numListsCompleted++;

            if (_srList.listIndex >= 0)
            {
                string historyFile = FileIO.CombinePaths(DataFileLocations.SubjectMetaFolder, _srList.TestType + "_History.xml");
                if (File.Exists(historyFile))
                {
                    var history = FileIO.XmlDeserialize<ListHistory>(historyFile);
                    history.LastCompleted = _srList.listIndex;
                    FileIO.XmlSerialize(history, historyFile);
                }
            }

            // tests remaining?
            if (_srTest.Lists.Count > 0)
            {
                StartCoroutine(StartNextList());
            }
            else
            {
                commonUI.ShowHelpBox("Excellent!");
                commonUI.ShowNextButton("Press Next to start the next task", Return);
            }
        }
    }
    
    public void AbortTest()
    {
        // save summary data
        if (!_useClosedSet)
        {
            DiagnosticsManager.Instance.EndTest(_dataPath);
        }
        else
        {
            _srClosedSetData.Finish();
            DiagnosticsManager.Instance.CompleteTestButNoAdvance(_srClosedSetData, "Speech", _srClosedSetData.test);
        }
    }

    private string SaveResponseClip(int runNum, string test, int attemptNum)
    {
        SpeechReception.Response r = new SpeechReception.Response();
        r.test = _srData.test;
        r.sentenceNum = _qnum;
        r.sentence = _srList.sentences[_qnum].whole;
        r.Fs = _srData.Fs;
        r.attemptNum = attemptNum;
        r.date = System.DateTime.Now.ToString();
        r.numChannels = audioRecord.clip.channels;


        string responsePath = SubjectManager.CurrentSubject + "-SpeechFile-Run" + runNum + "-" + test + "-" + _qnum;
        if (attemptNum > 1)
        {
            responsePath += "-" + attemptNum;
        }
        responsePath = System.IO.Path.Combine(DataFileLocations.DataFolder, responsePath + ".json");

        string json = KLib.FileIO.JSONStringAdd("", "info", KLib.FileIO.JSONSerializeToString(r));
        KLib.FileIO.WriteTextFile(responsePath, json);
        DataFileManager.UploadDataFile(responsePath);

        float[] data = new float[audioRecord.clip.samples * audioRecord.clip.channels];
        audioRecord.clip.GetData(data, 0);

        responsePath = responsePath.Replace(".json", ".wav");
        KLib.Wave.WaveFile.Write(data, (uint) r.Fs, 16, responsePath);
        //string uploadFolder = System.IO.Path.Combine(DataFileLocations.ProjectFolder, "Upload");
        //FileIO.Copy(responsePath, System.IO.Path.Combine(uploadFolder, System.IO.Path.GetFileName(responsePath)));
        DataFileManager.UploadDataFile(responsePath);

        return System.IO.Path.GetFileName(responsePath);
    }

    private bool ReviewThisOne()
    {
        return (_reviewResponses && _qnum < _srTest.NumToReview);
    }

    public void OnOKButtonClick()
    {
        if (_itsGoodButtonPressed)
        {
            return;
        }
        _itsGoodButtonPressed = true;

        if (_recordingState == RecordingState.Validating)
        {
            _responseAccepted = true;
        }
        else if (_recordingState == RecordingState.TimedOut)
        {
            _responseAccepted = true;
        }
        else
        {
            _recordingState = RecordingState.StopAndContinue;
        }
    }
    
    public void OnRecordButtonClick()
    {
        if (_recordButtonPressed)
        {
            // avoid double clicks
            return;
        }

        _recordButtonPressed = true;
        _recordButtonTween.ResetToBeginning();
        _recordButtonTween.PlayForward();

        StartCoroutine(WaitForRecordButtonTween());

        //recordingState = RecordingState.Start;
    }

    private IEnumerator WaitForRecordButtonTween()
    {
        yield return new WaitForSeconds(_recordButtonTween.duration);
        _recordingState = RecordingState.Start;
    }

    public void OnRecordButtonTweenFinished()
    {
        _recordingState = RecordingState.Start;
    }

    public void OnStopButtonClick()
    {
        if (_stopButtonPressed)
        {
            return;
        }
        _stopButtonPressed = true;

        _stopButtonTween.ResetToBeginning();
        //stopButtonTween.PlayForward();
        
        KLib.Unity.SetButtonState(StopButton, false);
        _recordingState = RecordingState.Stop;
    }
    
    public void OnRedoButtonClick()
    {
        if (_rerecordButtonPressed)
        {
            return;
        }
        _rerecordButtonPressed = true;

        if (_recordingState == RecordingState.Validating)
        {
            NGUITools.SetActive(RepeatButton.gameObject, false);
            NGUITools.SetActive(RedoButton.gameObject, false);
            NGUITools.SetActive(ContinueButton.gameObject, false);

            StopButton.transform.localPosition = _stopButtonTween.from;
        
            NGUITools.SetActive(RecordButton.gameObject, true);
            NGUITools.SetActive(StopButton.gameObject, true);
            _recordButtonPressed = false;
            _stopButtonPressed = false;
            KLib.Unity.SetButtonState(StopButton, true, _buttonColor);

            ++_responseAttempt;
            StartCoroutine(RecordResponse());
        }
        else if (_recordingState == RecordingState.TimedOut)
        {
            ++_responseAttempt;
            StartCoroutine(RecordResponse());
        }
        else
        {
            prompt.text = "Wait...";
            _recordingState = RecordingState.StopAndRedo;
        }
    }

    private void ShowInstructions(string instructionFile)
    {
        commonUI.SetHelpBalloonSize(1400, 700);
        commonUI.OnPageChangeNotifier = ShowInstructionPage;

        _instructions = FileIO.XmlDeserialize<Instructions>(DataFileLocations.ConfigFile(instructionFile));

        ShowInstructionPage(0);
    }

    private void ShowInstructionPage(int pageNum)
    {
        if (pageNum < _instructions.pages.Count)
        {
            commonUI.ShowHelpPage(
                _instructions.pages[pageNum],
                pageNum,
                (pageNum < _instructions.pages.Count - 1 ? "Next" : "Begin")
            );
        }
        else
        {
            StartNextSentence();
        }
    }

    private IEnumerator ShowEndReviewInstructions()
    {
        yield return commonUI.AnimateHelpText(
            "OK, it seems like the recording is working.\n" +
            "We'll stop asking you to check it every time..." +
            ""
            );
        yield return commonUI.AnimateHelpText(
            "...but you will always have the option to re-record your response." +
            "", "continue", StartNextSentence
            );
    }
    private IEnumerator ShowVolumeWarning()
    {
        yield return commonUI.AnimateHelpText(
            "Please don't try to change the volume.\n" +
            "It may seem too low, but we set it where it will give the most meaningful results.\n" +
            "It's OK, just do your best!" +
            ""
            );
    }


#if KDEBUG
    private IEnumerator InAndOut()
    {
        commonUI.FormatHelpBox(_testXmlFile + "\nSimulation done.\n");
        yield return new WaitForSeconds(1f);
        Return();
    }
#endif
*/
}
