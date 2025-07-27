using Audiometer;
using KLib;
using KLib.Signals;
using KLib.Signals.Waveforms;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudiometerController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private TextAsset _defaultInstructions;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private GameObject _workPanel;


    private AudiometerSettings _settings = new AudiometerSettings();

    private UdpClient _udpClient;
    private int _udpPort = 63557;
    private IPEndPoint _udpEndPoint;
    private UDPPacket _udpPacket = new UDPPacket();

    private SignalManager _signalManager;

    private string _mySceneName = "Audiometer";

    private InputAction _abortAction;

    private bool _isRunning = false;

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += OnAbortAction;
    }

    private void Start()
    {
        HTS_Server.SetCurrentScene(_mySceneName, this);

        //_title.text = "";

        _udpClient = new UdpClient();
        _udpEndPoint = new IPEndPoint(HTS_Server.RemoteAddress, _udpPort);
        Debug.Log(_udpEndPoint.ToString());
    }

    private void Update()
    {
        if (_isRunning)
        {
            _udpPacket.Status = 1;
            _udpPacket.SetAmplitudes(_signalManager.CurrentAmplitudes);
            _udpPacket.UpdateByteArray();
            _udpClient.Send(_udpPacket.ByteArray, _udpPacket.ByteArray.Length, _udpEndPoint);
        }
    }

    void InitializeStimulusGeneration()
    {
        _isRunning = false;

        _signalManager = new SignalManager();
        _signalManager.AdapterMap = HardwareInterface.AdapterMap;

        for (int k=0; k<_settings.Channels.Length; k++)
        {
            var ch = CreateChannel(k, Laterality.Left, _settings.Channels[k]);
            _signalManager.AddChannel(ch);
            ch = CreateChannel(k, Laterality.Right, _settings.Channels[k]);
            _signalManager.AddChannel(ch);
        }

        var audioConfig = AudioSettings.GetConfiguration();
        _signalManager.Initialize(audioConfig.sampleRate, audioConfig.dspBufferSize);
        _signalManager.StartPaused();
        _signalManager.Unpause();

        _isRunning = true;
    }

    private Channel CreateChannel(int number, Laterality laterality, AudiometerChannel channel)
    {
        var ch = new Channel()
        {
            Name = $"Channel{number}{laterality}",
            Modality = KLib.Signals.Enumerations.Modality.Audio,
            Laterality = laterality,
            active = (channel.Continuous && (channel.Routing == "Binaural" || channel.Routing == laterality.ToString())),
            waveform = new FM()
            {
                Carrier_Hz = 1000,
                ModFreq_Hz = 5,
                Depth_Hz = 0
            },
            level = new Level()
            {
                Units = LevelUnits.dB_SPL,
                Value = 75
            },
            gate = new Gate()
            {
                Active = !channel.Continuous,
                Duration_ms = _settings.Duration,
                Ramp_ms = _settings.Ramp,
                Period_ms = _settings.PipInterval,
                Bursted = true,
                NumPulses = _settings.NumPulses
            }
        };

        return ch;
    }

    private void UpdateChannel(string data)
    {
        var parts = data.Split(':');
        int chNum = int.Parse(parts[0]) - 1;
        string prop = parts[1];
        float value = float.Parse(parts[2]);

        if (prop == "Contin")
        {
            _signalManager.channels[chNum].active = value > 0 && _settings.Channels[chNum].Routing != "Right";
            _signalManager.channels[chNum + 1].active = value > 0 && _settings.Channels[chNum].Routing != "Left";
        }

    }

    private void Pulse()
    {
        _signalManager.channels[0].SetActive(true);
        //_signalManager.channels[1].SetActive(true);
    }

    void OnAbortAction(InputAction.CallbackContext context)
    {
        _abortAction.Disable();

        _workPanel.SetActive(false);
        _instructionPanel.gameObject.SetActive(false);
        _quitPanel.SetActive(true);
    }

    public void OnQuitConfirmButtonClick()
    {
    }

    public void OnQuitCancelButtonClick()
    {
        _quitPanel.SetActive(false);
        _abortAction.Enable();
    }

    private void ShowInstructions(string instructions, int fontSize)
    {
        _instructionPanel.gameObject.SetActive(true);
        //_instructionPanel.InstructionsFinished = StartMeasurement;
        _instructionPanel.ShowInstructions(new Turandot.Instructions() { Text = instructions, FontSize = fontSize });
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
                _settings = FileIO.JSONDeserializeFromString<AudiometerSettings>(data);
                InitializeStimulusGeneration();
                break;
            case "Channel":
                UpdateChannel(data);
                break;
            case "Pulse":
                Pulse();
                break;
            case "Stop":
                _signalManager.Pause();
                break;
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_isRunning)
        {
            _signalManager.Synthesize(data);

        }
    }
}
