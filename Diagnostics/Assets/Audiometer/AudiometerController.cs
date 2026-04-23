using Audiograms;
using Audiometer;
using KLib;
using KLibU.Net;
using KLib.Signals;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using C462.Shared;

using UDPPacket = Audiometer.UDPPacket;

public class AudiometerController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private TextAsset _defaultInstructions;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private GameObject _workPanel;

    List<TransducerType> _transducers;
    private AudiometerSettings _settings = new AudiometerSettings();

    private UdpClient _udpClient;
    private int _udpPort = 63557;
    private IPEndPoint _udpEndPoint;
    private UDPPacket _udpPacket = new UDPPacket();

    private SignalManager _signalManager;
    private int _pulsedChannelIndex;

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

        _transducers = new List<TransducerType>();
        _pulsedChannelIndex = _settings.Channels.ToList().FindIndex(x => x.Pulsed);

        _signalManager = new SignalManager();

        for (int k=0; k<_settings.Channels.Length; k++)
        {
            // TO DO: replace with actual transducer types
            if (_settings.Channels[k].Transducer == "Insert")
                _transducers.Add(TransducerType.Insert);
            else
                _transducers.Add(TransducerType.HD300);

            var ch = CreateChannel(k + 1, Laterality.Left, _settings.Channels[k]);
            _signalManager.AddChannel(ch);
            ch = CreateChannel(k + 1, Laterality.Right, _settings.Channels[k]);
            _signalManager.AddChannel(ch);
        }

        var audioConfig = AudioSettings.GetConfiguration();
        _signalManager.Initialize(audioConfig.sampleRate, audioConfig.dspBufferSize, SessionContext.Signal);

        _isRunning = true;
    }

    private Channel CreateChannel(int number, Laterality laterality, AudiometerChannel channel)
    {
        var ch = new Channel()
        {
            //Name = $"Channel{number}{laterality}",
            Modality = KLib.Signals.Modality.Audio,
            Laterality = laterality,
            Active = (channel.Continuous && (channel.Routing == "Binaural" || channel.Routing == laterality.ToString())),
            Waveform = new FM()
            {
                Carrier_Hz = channel.Freq,
                ModFreq_Hz = 5,
                Depth_Hz = 0
            },
            Level = new Level()
            {
                Units = LevelUnits.dB_SPL,
                Value = (channel.Level + ANSI_dBHL.HL_To_SPL(channel.Freq, 0, _transducers[number - 1])).ToString()
            },
            Gate = new Gate()
            {
                Active = channel.Pulsed,
                Width_ms = _settings.Duration,
                Ramp_ms = _settings.Ramp,
                Period_ms = Mathf.Max(_settings.PipInterval, _settings.Duration),
                Bursted = true,
                NumPulses = _settings.NumPulses,
                BurstDuration_ms = _settings.NumPulses * _settings.PipInterval,
                ForceOneShot = channel.Pulsed
            }
        };

        return ch;
    }

    private void UpdateChannel(string data)
    {
        Debug.Log($"Update channel: {data}");

        var parts = data.Split(':');
        int chNum = int.Parse(parts[0]) - 1;
        string prop = parts[1];

        if (prop == "Contin")
        {
            bool value = parts[2].ToLower().Equals("true");

            _settings.Channels[chNum].Continuous = value;
            _signalManager.Channels[2*chNum].Active = value && _settings.Channels[chNum].Routing != "Right";
            _signalManager.Channels[2*chNum + 1].Active = value && _settings.Channels[chNum].Routing != "Left";
        }
        else if (prop == "Routing")
        {
            _settings.Channels[chNum].Routing = parts[2];
            if (_settings.Channels[chNum].Continuous)
            {
                _signalManager.Channels[2 * chNum].Active = _settings.Channels[chNum].Routing != "Right";
                _signalManager.Channels[2 * chNum + 1].Active = _settings.Channels[chNum].Routing != "Left";
            }
        }
        else if (prop == "Freq")
        {
            var freq = float.Parse(parts[2]);
            var level = float.Parse(parts[3]);

            _settings.Channels[chNum].Freq = freq;
            _settings.Channels[chNum].Level = level;

            (_signalManager.Channels[2 * chNum].Waveform as FM).Carrier_Hz = _settings.Channels[chNum].Freq;
            _signalManager.Channels[2 * chNum].Level.SetParameter("Value", level + ANSI_dBHL.HL_To_SPL(_settings.Channels[chNum].Freq, 0, _transducers[chNum]));

            (_signalManager.Channels[2 * chNum + 1].Waveform as FM).Carrier_Hz = _settings.Channels[chNum].Freq;
            _signalManager.Channels[2 * chNum + 1].Level.SetParameter("Value", level + ANSI_dBHL.HL_To_SPL(_settings.Channels[chNum].Freq, 0, _transducers[chNum]));
        }
    }

    private void UpdatePulsedChannelIndex(string data)
    {
        _pulsedChannelIndex = int.Parse(data);
        for (int k=0; k<_settings.Channels.Length; k++)
        {
            _settings.Channels[k].Pulsed = k == _pulsedChannelIndex;
        }
    }

    private void Pulse()
    {
        if (_pulsedChannelIndex >= 0)
        {
            _signalManager.Channels[2 * _pulsedChannelIndex].SetActive(_settings.Channels[_pulsedChannelIndex].Routing != "Right");
            _signalManager.Channels[2 * _pulsedChannelIndex + 1].SetActive(_settings.Channels[_pulsedChannelIndex].Routing != "Left");
        }
    }

    private void SetDuration(string data)
    {
        _settings.Duration = float.Parse(data);


        foreach (var ch in _signalManager.Channels)
        {
            ch.SetParameter("Gate.Duration_ms", _settings.Duration);
            if (_settings.NumPulses == 1)
            {
                ch.SetParameter("Gate.Period_ms", _settings.Duration + 10);
            }
        }
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


    TcpMessage IRemoteControllable.ProcessRPC(TcpMessage request)
    {
        switch (request.Command)
        {
            case "Initialize":
                Debug.Log(request.Payload);
                Debug.Log(KLibU.Files.JSONSerializeToString(new AudiometerSettings()));
                _settings = request.GetPayload<AudiometerSettings>();
                InitializeStimulusGeneration();
                return TcpMessage.Ok();
            case "Channel":
                var data = request.GetPayload<string>();
                UpdateChannel(data);
                return TcpMessage.Ok();
            case "Pulsed":
                var chanStr = request.GetPayload<string>();
                UpdatePulsedChannelIndex(chanStr);
                return TcpMessage.Ok();
            case "SetDuration":
                var durStr = request.GetPayload<string>();
                SetDuration(durStr);
                return TcpMessage.Ok();
            case "Pulse":
                Pulse();
                return TcpMessage.Ok();
            case "Stop":
                _signalManager.Pause();
                return TcpMessage.Ok();
            default:
                return TcpMessage.NotFound(request.Command);
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
