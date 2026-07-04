using BasicMeasurements;
using C462.Shared;
using KLib.Signals;
using KLibU;
using KLibU.Net;
using KLibU.Synthesizers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Tapping;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TappingSceneController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private TextAsset _defaultInstructions;
    [SerializeField] private Text _finishText;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private QuestionBox _questionBox;
    [SerializeField] private GameObject _workPanel;
    [SerializeField] private Slider _progressBar;

    private bool _isRemote;

    private bool _stopMeasurement = false;
    private bool _localAbort = false;

    private TappingConfiguration _settings = new TappingConfiguration();

    private string _dataPath;
    private string _mySceneName = "Tapping";
    private string _configName;

    private Synthesizer _synth;
    private ClipTrack _clipTrack;

    private bool _audioEnabled = false;
    private bool _stopAudio = false;

    private InputAction _abortAction;

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += OnAbortAction;
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
    }
    void InitializeMeasurement()
    {
        InitDataFile();
    }

    void InitDataFile()
    {
        var fileStemStart = $"{GameManager.Subject}-{_mySceneName}";
        while (true)
        {
            string fileStem = $"{fileStemStart}-Run{GameManager.GetNextRunNumber(_mySceneName):000}";
            fileStem = Path.Combine(SharedFileLocations.HtsSubjectDataFolder, fileStem);
            _dataPath = fileStem + ".json";
            if (!File.Exists(_dataPath))
            {
                break;
            }
        }

        var header = new BasicMeasurementFileHeader()
        {
            measurementType = _mySceneName,
            configName = _configName,
            subjectID = GameManager.Subject
        };

        string json = Files.JSONStringAdd("", "info", Files.JSONSerializeToString(header));
        json = Files.JSONStringAdd(json, "params", Files.JSONSerializeToString(_settings));
        json += Environment.NewLine;

        File.WriteAllText(_dataPath, json);
    }

    private void CreatePlan()
    {

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

            HTS_Server.SendRequest(_mySceneName, "Status:Instructions");
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
        _workPanel.SetActive(true);

        InitializeSynth();

        _audioEnabled = true;
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
        HTS_Server.SendRequest(_mySceneName, $"Finished:{status}");
        HTS_Server.SendDataFile(_mySceneName, _dataPath);

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

        HTS_Server.SendRequest(_mySceneName, $"Error:{error}");
        Debug.Log($"[{_mySceneName} error]: {error}{Environment.NewLine}{stackTrace}");

        if (!_isRemote)
        {
            ShowFinishPanel("The run was stopped because of an error");
        }
    }

    private void InitializeSynth()
    {
        var audioConfig = AudioSettings.GetConfiguration();

        var channel = _settings.Channel.Clone();
        float Tmax = channel.Gate.Width_ms;
        int npts = Mathf.RoundToInt(audioConfig.sampleRate * Tmax / 1000f);

        var signalManager = new SignalManager();
        signalManager.AddChannel(channel);

        signalManager.Initialize(audioConfig.sampleRate, npts, SessionContext.Signal);

        _clipTrack = new ClipTrack(channelOffsetL: channel.OutputNum, channelOffsetR: channel.IsStereo ? channel.ContraOutputNum : -1);
        _clipTrack.Name = "ClipTrack";
        _clipTrack.ClipManager.CalibratedLevel = 0f;
        _clipTrack.ClipManager.Reverb.Active = false;
        _clipTrack.Sequencer.Active = true;
        _clipTrack.Sequencer.Steps = 8;
        _clipTrack.Sequencer.EventOrder = EventOrder.Fixed;
        _clipTrack.Sequencer.SetNote(0);
        _clipTrack.Sequencer.SetEvents(setAll: true);

        channel.SetActive(true);
        channel.Create();

        _clipTrack.ClipManager.AddClip(channel.Data, 0);
        _synth = new Synthesizer();

        float beatInterval_s = 2f * _settings.MinISI / 1000;
        float bpm = 60f / beatInterval_s;
        _synth.BPM = bpm; 

        _synth.Tracks.Add(_clipTrack);
    }

    TcpMessage IRemoteControllable.ProcessRPC(TcpMessage request)
    {
        var data = request.GetPayload<string>();
        switch (request.Command)
        {
            case "Initialize":
                _settings = Files.XmlDeserializeFromString<BasicMeasurementConfiguration>(data) as TappingConfiguration;
                InitializeMeasurement();
                return TcpMessage.Ok(Path.GetFileName(_dataPath));
            case "Begin":
                StartCoroutine(BeginNextFrame());
                return TcpMessage.Ok();
            case "Abort":
                _stopMeasurement = true;
                return TcpMessage.Ok();
            default:
                return TcpMessage.NotFound(request.Command);
        }
    }

    IEnumerator BeginNextFrame()
    {
        yield return null;
        Begin();
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_audioEnabled)
        {
            if (_stopAudio)
            {
                Gate.RampDown(data);
                _audioEnabled = false;
            }
        }
    }
}
