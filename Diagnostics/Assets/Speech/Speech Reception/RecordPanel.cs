using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class RecordPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button _recordButton;
    [SerializeField] private TMPro.TMP_Text _prompt;

    [Header("Audio")]
    [SerializeField] private AudioClip _recordStartClip;
    [SerializeField] private AudioClip _recordEndClip;

    private AudioSource _audioAlert;
    private AudioSource _audioRecord;

    private bool _audioCuesOnly;
    private bool _reviewThisOne;

    private int _responseAttempt;
    private bool _responseAccepted;

    private static float _maxRecordTime_sec = 10;
    private static readonly int _maxNumRecordAttempts = 3;

    private enum RecordingState { Waiting, Recording, TimedOut, Stop, StopAndRedo, StopAndContinue, Validating };
    private RecordingState _recordingState;

    public delegate void StatusUpdateDelegate(string status);
    public StatusUpdateDelegate StatusUpdate;
    private void OnStatusUpdate(string status)
    {
        StatusUpdate?.Invoke(status);
    }

    void Start()
    {
        _recordButton.gameObject.SetActive(false);
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

    public void SetAlertAudioSource(AudioSource audioSource)
    {
        _audioAlert = audioSource;
    }

    public void AcquireAudioResponse(bool audioCuesOnly, bool review)
    {
        _audioCuesOnly = audioCuesOnly;
        _reviewThisOne = review;

        _responseAttempt = 1;
        _responseAccepted = false;

        if (_audioCuesOnly)
        {
            StartCoroutine(RecordResponse());
        }
        else
        {
            _prompt.text = "What did you hear?";
            _recordButton.gameObject.SetActive(true);
            _recordButton.interactable = true;
        }
    }

    public void OnRecordButtonClick()
    {
        _recordButton.interactable = false;
        OnStatusUpdate("RecordStart");

        StartCoroutine(RecordResponse());
    }

    IEnumerator RecordResponse()
    {
        if (_audioCuesOnly)
        {
            yield return new WaitForSeconds(0.5f);
        }

        _prompt.text = "What did you hear?";

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

        //NGUITools.SetActive(MicSprite.gameObject, true);
        //NGUITools.SetActive(prompt.gameObject, true);
        //prompt.text = "What did you hear?";
        //NGUITools.SetActive(progressBar.gameObject, true);
        //MicTweener.enabled = true;

        _recordingState = RecordingState.Recording;

        _audioRecord.clip = Microphone.Start(null, false, (int)(_maxRecordTime_sec), _srData.Fs);

        audioAlert.clip = recordStartClip;
        audioAlert.Play();

        //float progressBarSamples = (float)(audioRecord.clip.samples);

        //progressBar.value = 1.0f;
        //while (Microphone.IsRecording(null) && _recordingState == RecordingState.Recording)
        //{
        //    yield return new WaitForSeconds(0.1f);
        //    progressBar.value = Microphone.IsRecording(null) ? Mathf.Max(0f, 1f - (float)(Microphone.GetPosition(null) / progressBarSamples)) : 0;
        //}

        //if (_recordingState == RecordingState.Recording)
        //{
        //    _recordingState = RecordingState.TimedOut;
        //}
        //if (_recordingState == RecordingState.Stop || _recordingState == RecordingState.StopAndContinue)
        //{
        //    yield return new WaitForSeconds(0.5f);
        //}

        //int endPosition = Microphone.GetPosition(null);
        //Microphone.End(null);

        //KLib.Unity.SetButtonState(StopButton, false);

        //// Trim response, if needed
        //if (endPosition > 0 && endPosition < audioRecord.clip.samples)
        //{
        //    float[] data = new float[endPosition];
        //    audioRecord.clip.GetData(data, 0);
        //    audioRecord.clip = AudioClip.Create("clip", data.Length, 1, _srData.Fs, false, false);
        //    audioRecord.clip.SetData(data, 0);
        //}

        //audioAlert.clip = recordEndClip;
        //audioAlert.Play();

        ////progressBar.value = 0;
        //MicTweener.enabled = false;

        //bool volumeChanged = _volumeManager.GetMasterVolume(VolumeManager.VolumeUnit.Decibel) != _volumeAtten;
        //string respPath = SaveResponseClip(_srData.runNumber, _srData.test, _responseAttempt);

        //_tentativeResponse = new SpeechReception.Data.Response(_srList.sentences[_qnum].whole, _srList.sentences[_qnum].words, _srList.sentences[_qnum].SNR, volumeChanged, respPath);

        //if (ReviewThisOne())
        //{
        //    _recordingState = (_responseAttempt < maxNumRecordAttempts) ? RecordingState.Validating : RecordingState.StopAndContinue;
        //}

        //StartCoroutine(DisposeOfResponse());
        yield return null;
    }

}
