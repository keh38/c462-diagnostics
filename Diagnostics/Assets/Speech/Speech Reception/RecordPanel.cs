using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class RecordPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMPro.TMP_Text _prompt;
    [SerializeField] private RecordButton _recordButton;
    [SerializeField] private RecordButton _stopButton;
    [SerializeField] private Slider _powerBar;

    [Header("Audio")]
    [SerializeField] private AudioClip _recordStartClip;
    [SerializeField] private AudioClip _recordEndClip;

    private AudioSource _audioAlert;
    private AudioSource _audioRecord;

    private int _Fs;

    private bool _reviewThisOne;

    private int _responseAttempt;
    private bool _responseAccepted;

    private static float _maxRecordTime_sec = 10;
    private static readonly int _maxNumRecordAttempts = 3;

    private enum RecordingState { Waiting, Recording, TimedOut, Stop, StopAndRedo, StopAndContinue, Validating };
    private RecordingState _recordingState;

    public bool AudioCuesOnly { get; set; }

    public delegate void StatusUpdateDelegate(string status);
    public StatusUpdateDelegate StatusUpdate;
    private void OnStatusUpdate(string status)
    {
        StatusUpdate?.Invoke(status);
    }

    void Start()
    {
        Hide();
        CreateAudioSource();
    }

    private void CreateAudioSource()
    {
        _audioRecord = gameObject.AddComponent<AudioSource>();
        _audioRecord.bypassEffects = true;
        _audioRecord.bypassListenerEffects = true;
        _audioRecord.bypassReverbZones = true;
        _audioRecord.loop = false;
        _audioRecord.spatialBlend = 0;
    }

    public void Initialize(int Fs, AudioSource audioSource)
    {
        _Fs = Fs;
        _audioAlert = audioSource;
    }

    public void Hide()
    {
        _recordButton.gameObject.SetActive(false);
        _stopButton.gameObject.SetActive(false);
        _powerBar.gameObject.SetActive(false);
    }

    public void AcquireAudioResponse(bool review)
    {
        _reviewThisOne = review;

        _responseAttempt = 1;
        _responseAccepted = false;

        if (AudioCuesOnly)
        {
            StartCoroutine(RecordResponse());
        }
        else
        {
            _prompt.text = "What did you hear?";
            
            _recordButton.gameObject.SetActive(true);
            _stopButton.gameObject.SetActive(true);

            _recordButton.SetInteractable(true);
            _stopButton.SetInteractable(false);
        }
    }

    public void OnRecordButtonClick()
    {
        _recordButton.SetInteractable(false);
        _stopButton.SetInteractable(true);

        StartCoroutine(RecordResponse());
    }

    public void OnStopButtonClick()
    {
        _recordingState = RecordingState.Stop;
        _stopButton.SetInteractable(false);
    }

    IEnumerator RecordResponse()
    {
        if (AudioCuesOnly)
        {
            yield return new WaitForSeconds(0.5f);
        }

        _prompt.text = "What did you hear?";
        OnStatusUpdate("RecordStart");

        //NGUITools.SetActive(RepeatButton.gameObject, false);
        //NGUITools.SetActive(ContinueButton.gameObject, false);

        //bool reviewThisOne = ReviewThisOne();
        //NGUITools.SetActive(OKButton.gameObject, !reviewThisOne);
        //NGUITools.SetActive(RedoButton.gameObject, !reviewThisOne);
        //if (!reviewThisOne)
        //{
        //    _itsGoodButtonPressed = false;
        //    _rerecordButtonPressed = false;
        //}


        //_OKButtonLabel.text = (ReviewThisOne()) ? "It's good!" : "Done";

        _recordingState = RecordingState.Recording;

        _audioRecord.clip = Microphone.Start(null, false, (int)(_maxRecordTime_sec), _Fs);

        _audioAlert.clip = _recordStartClip;
        _audioAlert.Play();

        _powerBar.gameObject.SetActive(true);
        float progressBarSamples = (float)(_audioRecord.clip.samples);

        _powerBar.value = 1.0f;
        while (Microphone.IsRecording(null) && _recordingState == RecordingState.Recording)
        {
            yield return new WaitForSeconds(0.1f);
            _powerBar.value = Microphone.IsRecording(null) ? Mathf.Max(0f, 1f - (float)(Microphone.GetPosition(null) / progressBarSamples)) : 0;
        }

        if (_recordingState == RecordingState.Recording)
        {
            _recordingState = RecordingState.TimedOut;
        }
        else if (_recordingState == RecordingState.Stop || _recordingState == RecordingState.StopAndContinue)
        {
            yield return new WaitForSeconds(0.5f);
        }

        int endPosition = Microphone.GetPosition(null);
        Microphone.End(null);

        _stopButton.SetInteractable(false);


        // Trim response, if needed
        if (endPosition > 0 && endPosition < _audioRecord.clip.samples)
        {
            float[] data = new float[endPosition];
            _audioRecord.clip.GetData(data, 0);
            _audioRecord.clip = AudioClip.Create("clip", data.Length, 1, _Fs, false, false);
            _audioRecord.clip.SetData(data, 0);
        }

        _audioAlert.clip = _recordEndClip;
        _audioAlert.Play();

        _powerBar.value = 0f;

        //bool volumeChanged = _volumeManager.GetMasterVolume(VolumeManager.VolumeUnit.Decibel) != _volumeAtten;
        //string respPath = SaveResponseClip(_srData.runNumber, _srData.test, _responseAttempt);

        //_tentativeResponse = new SpeechReception.Data.Response(_srList.sentences[_qnum].whole, _srList.sentences[_qnum].words, _srList.sentences[_qnum].SNR, volumeChanged, respPath);

        if (_reviewThisOne)
        {
            _recordingState = (_responseAttempt < _maxNumRecordAttempts) ? RecordingState.Validating : RecordingState.StopAndContinue;
        }

        StartCoroutine(DisposeOfResponse());
    }

    private IEnumerator DisposeOfResponse()
    {
        switch (_recordingState)
        {
            case RecordingState.Validating:
                _prompt.text = "Check your response";
                
                _audioRecord.Play();
                while (_audioRecord.isPlaying)
                {
                    _powerBar.value = 1f - (float)(_audioRecord.time) / _audioRecord.clip.length;
                    yield return new WaitForSeconds(0.1f);
                }
                _powerBar.value = 0;
                _powerBar.gameObject.SetActive(false);
                //NGUITools.SetActive(prompt.gameObject, false);
                //NGUITools.SetActive(RecordButton.gameObject, false);
                //NGUITools.SetActive(StopButton.gameObject, false);

                _prompt.text = "Was your response recorded OK?";
                //NGUITools.SetActive(OKButton.gameObject, true);
                //NGUITools.SetActive(RedoButton.gameObject, true);
                //_itsGoodButtonPressed = false;
                //_rerecordButtonPressed = false;
                break;

            case RecordingState.StopAndContinue:
                yield return new WaitForSeconds(0.5f);
                //NGUITools.SetActive(MicSprite.gameObject, false);
                //NGUITools.SetActive(prompt.gameObject, false);
                //NGUITools.SetActive(progressBar.gameObject, false);
                //NGUITools.SetActive(RecordButton.gameObject, false);
                //NGUITools.SetActive(StopButton.gameObject, false);
                _responseAccepted = true;
                break;

            case RecordingState.StopAndRedo:
                yield return new WaitForSeconds(0.25f);
                ++_responseAttempt;
                StartCoroutine(RecordResponse());
                break;

            case RecordingState.TimedOut:
                //NGUITools.SetActive(prompt.gameObject, false);
                //NGUITools.SetActive(progressBar.gameObject, false);
                //NGUITools.SetActive(OKButton.gameObject, false);
                //NGUITools.SetActive(RedoButton.gameObject, false);
                //NGUITools.SetActive(RepeatButton.gameObject, true);
                //NGUITools.SetActive(ContinueButton.gameObject, true);
                //_rerecordButtonPressed = false;
                //_itsGoodButtonPressed = false;
                break;
        }
    }

    public void OnOKButtonClick()
    {
        //if (_itsGoodButtonPressed)
        //{
        //    return;
        //}
        //_itsGoodButtonPressed = true;

        //if (_recordingState == RecordingState.Validating)
        //{
        //    _responseAccepted = true;
        //}
        //else if (_recordingState == RecordingState.TimedOut)
        //{
        //    _responseAccepted = true;
        //}
        //else
        //{
        //    _recordingState = RecordingState.StopAndContinue;
        //}
    }

    public void OnRedoButtonClick()
    {
        //if (_rerecordButtonPressed)
        //{
        //    return;
        //}
        //_rerecordButtonPressed = true;

        //if (_recordingState == RecordingState.Validating)
        //{
        //    NGUITools.SetActive(RepeatButton.gameObject, false);
        //    NGUITools.SetActive(RedoButton.gameObject, false);
        //    NGUITools.SetActive(ContinueButton.gameObject, false);

        //    StopButton.transform.localPosition = _stopButtonTween.from;

        //    NGUITools.SetActive(RecordButton.gameObject, true);
        //    NGUITools.SetActive(StopButton.gameObject, true);
        //    _recordButtonPressed = false;
        //    _stopButtonPressed = false;
        //    KLib.Unity.SetButtonState(StopButton, true, _buttonColor);

        //    ++_responseAttempt;
        //    StartCoroutine(RecordResponse());
        //}
        //else if (_recordingState == RecordingState.TimedOut)
        //{
        //    ++_responseAttempt;
        //    StartCoroutine(RecordResponse());
        //}
        //else
        //{
        //    prompt.text = "Wait...";
        //    _recordingState = RecordingState.StopAndRedo;
        //}
    }



}
