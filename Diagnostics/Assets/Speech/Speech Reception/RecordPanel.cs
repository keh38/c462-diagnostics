using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class RecordPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMPro.TMP_Text _prompt;
    [SerializeField] private RecordButton _recordButton;
    [SerializeField] private RecordButton _stopButton;
    [SerializeField] private RecordButton _redoButton;
    [SerializeField] private RecordButton _acceptButton;
    [SerializeField] private Slider _powerBar;
    [SerializeField] private RecordButton _playButton;
    [SerializeField] private Slider _volumeSlider;

    [Header("Audio")]
    [SerializeField] private AudioClip _recordStartClip;
    [SerializeField] private AudioClip _recordEndClip;

    private AudioSource _audioAlert;
    private AudioSource _audioRecord;

    public int Fs { get; private set; } = 22050;

    private bool _reviewThisOne;

    private static float _maxRecordTime_sec = 10;
    private static readonly int _maxNumRecordAttempts = 3;

    private float _playbackVolumeMin = -40f;
    private float _playbackVolumeMax = 0;

    private enum RecordingState { Waiting, Recording, TimedOut, Stop, StopAndRedo, StopAndContinue, Validating };
    private RecordingState _recordingState;

    public int NumAttempts { get; private set; }
    public bool AudioCuesOnly { get; set; }
    public float[] Data { get; private set; }

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
        //_audioRecord.volume = 0.25f;

        _volumeSlider.value = 0.1f;
    }

    public void Initialize(int Fs, AudioSource audioSource)
    {
        this.Fs = Fs;
        _audioAlert = audioSource;
    }

    public void Hide()
    {
        _recordButton.gameObject.SetActive(false);
        _stopButton.gameObject.SetActive(false);
        _redoButton.gameObject.SetActive(false);
        _acceptButton.gameObject.SetActive(false);
        _powerBar.gameObject.SetActive(false);
        _playButton.gameObject.SetActive(false);
        _volumeSlider.gameObject.SetActive(false);
    }

    public void AcquireAudioResponse(bool review)
    {
        _reviewThisOne = review;

        NumAttempts = 1;

        if (AudioCuesOnly)
        {
            StartCoroutine(RecordResponse());
        }
        else
        {
            _prompt.text = "What did you hear?";
            
            _recordButton.gameObject.SetActive(true);
            _recordButton.SetInteractable(true);

            _stopButton.gameObject.SetActive(true);
            _stopButton.SetInteractable(false);
        }
    }

    IEnumerator RecordResponse()
    {
        if (AudioCuesOnly)
        {
            yield return new WaitForSeconds(0.5f);

            _recordButton.gameObject.SetActive(true);
            _recordButton.SetInteractable(false);
        }

        _playButton.gameObject.SetActive(false);
        _volumeSlider.gameObject.SetActive(false);

        _recordButton.gameObject.SetActive(_reviewThisOne);

        _redoButton.gameObject.SetActive(!_reviewThisOne);
        _redoButton.SetInteractable(!_reviewThisOne);

        _stopButton.gameObject.SetActive(_reviewThisOne);
        _stopButton.SetInteractable(_reviewThisOne);

        _acceptButton.gameObject.SetActive(!_reviewThisOne);
        _acceptButton.SetInteractable(!_reviewThisOne);

        _prompt.text = "What did you hear?";
        OnStatusUpdate("RecordingStarted");

        _acceptButton.SetLabel(_reviewThisOne? "It's good!" : "Done");

        _recordingState = RecordingState.Recording;

        _audioRecord.clip = Microphone.Start(null, false, (int)(_maxRecordTime_sec), Fs);

        _audioAlert.clip = _recordStartClip;
        _audioAlert.Play();

        _powerBar.gameObject.SetActive(true);
        float progressBarSamples = (float)(_audioRecord.clip.samples);

        _powerBar.value = 0f;
        while (Microphone.IsRecording(null) && _recordingState == RecordingState.Recording)
        {
            yield return new WaitForSeconds(0.1f);
            _powerBar.value = Microphone.IsRecording(null) ? Mathf.Min(1f, (float)(Microphone.GetPosition(null) / progressBarSamples)) : 0;
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
            Data = new float[endPosition];
            _audioRecord.clip.GetData(Data, 0);
            _audioRecord.clip = AudioClip.Create("clip", Data.Length, 1, Fs, false);
            _audioRecord.clip.SetData(Data, 0);
        }

        _audioAlert.clip = _recordEndClip;
        _audioAlert.Play();

        _powerBar.value = 0f;

        OnStatusUpdate("ResponseAcquired");

        if (_reviewThisOne)
        {
            _recordingState = (NumAttempts < _maxNumRecordAttempts) ? RecordingState.Validating : RecordingState.StopAndContinue;
        }

        StartCoroutine(DisposeOfResponse());
    }

    private IEnumerator DisposeOfResponse()
    {
        switch (_recordingState)
        {
            case RecordingState.Validating:
                StartCoroutine(PlaybackResponse());
                break;

            case RecordingState.StopAndContinue:
                yield return new WaitForSeconds(0.5f);
                OnStatusUpdate("ResponseAccepted");
                break;

            case RecordingState.StopAndRedo:
                yield return new WaitForSeconds(0.25f);
                ++NumAttempts;
                StartCoroutine(RecordResponse());
                break;

            case RecordingState.TimedOut:
                break;
        }
    }

    private IEnumerator PlaybackResponse()
    {
        _prompt.text = "Listen to your response...";

        _volumeSlider.gameObject.SetActive(true);
        _powerBar.value = 0f;
        _audioRecord.Play();
        while (_audioRecord.isPlaying)
        {
            _powerBar.value = (float)(_audioRecord.time) / _audioRecord.clip.length;
            yield return new WaitForSeconds(0.1f);
        }
        _powerBar.value = 0;

        _recordButton.gameObject.SetActive(false);
        _stopButton.gameObject.SetActive(false);
        
        _prompt.text = "Was your response recorded OK?";

        _playButton.gameObject.SetActive(true);
        _playButton.SetInteractable(true);

        _redoButton.gameObject.SetActive(true);
        _redoButton.SetInteractable(true);

        _acceptButton.gameObject.SetActive(true);
        _acceptButton.SetInteractable(true);
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

    public void OnPlayButtonClick()
    {
        _redoButton.SetInteractable(false);
        _acceptButton.SetInteractable(false);
        _playButton.SetInteractable(false);

        StartCoroutine(PlaybackResponse());
    }

    public void OnRedoButtonClick()
    {
        _redoButton.SetInteractable(false);
        _acceptButton.gameObject.SetActive(false);

        if (_recordingState == RecordingState.Validating || _recordingState == RecordingState.TimedOut)
        {
            NumAttempts++;
            StartCoroutine(RecordResponse());
        }
        else
        {
            _recordingState = RecordingState.StopAndRedo;
        }
    }

    public void OnAcceptButtonClick()
    {
        _acceptButton.SetInteractable(false);

        if (_recordingState == RecordingState.Validating || _recordingState == RecordingState.TimedOut)
        {
            OnStatusUpdate("ResponseAccepted");
        }
        else
        {
            _recordingState = RecordingState.StopAndContinue;
        }
    }

    public void OnVolumeSliderChange(float value)
    {
        _audioRecord.volume = Mathf.Pow(10, (value * (_playbackVolumeMax - _playbackVolumeMin) + _playbackVolumeMin) / 20);
    }


}
