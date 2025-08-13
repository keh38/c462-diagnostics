using Audiograms;
using Audiometer;
using KLib;
using KLib.Signals;
using KLib.Signals.Waveforms;

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

public class AudiometerController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private TextAsset _defaultInstructions;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private GameObject _workPanel;

    List<ANSI_dBHL> dBHL_tables;
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

        dBHL_tables = new List<ANSI_dBHL>();
        _pulsedChannelIndex = _settings.Channels.ToList().FindIndex(x => x.Pulsed);

        _signalManager = new SignalManager();
        _signalManager.AdapterMap = HardwareInterface.AdapterMap;

        for (int k=0; k<_settings.Channels.Length; k++)
        {
            dBHL_tables.Add(ANSI_dBHL.GetTable(_settings.Channels[k].Transducer));

            var ch = CreateChannel(k + 1, Laterality.Left, _settings.Channels[k]);
            _signalManager.AddChannel(ch);
            ch = CreateChannel(k + 1, Laterality.Right, _settings.Channels[k]);
            _signalManager.AddChannel(ch);
        }

        var audioConfig = AudioSettings.GetConfiguration();
        _signalManager.Initialize(audioConfig.sampleRate, audioConfig.dspBufferSize);

        _isRunning = true;
    }

    private Channel CreateChannel(int number, Laterality laterality, AudiometerChannel channel)
    {
        var ch = new Channel()
        {
            //Name = $"Channel{number}{laterality}",
            Modality = KLib.Signals.Enumerations.Modality.Audio,
            Laterality = laterality,
            active = (channel.Continuous && (channel.Routing == "Binaural" || channel.Routing == laterality.ToString())),
            waveform = new FM()
            {
                Carrier_Hz = channel.Freq,
                ModFreq_Hz = 5,
                Depth_Hz = 0
            },
            level = new Level()
            {
                Units = LevelUnits.dB_SPL,
                Value = channel.Level + dBHL_tables[number - 1].HL_To_SPL(channel.Freq)
            },
            gate = new Gate()
            {
                Active = channel.Pulsed,
                Duration_ms = _settings.Duration,
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
            _signalManager.channels[2*chNum].active = value && _settings.Channels[chNum].Routing != "Right";
            _signalManager.channels[2*chNum + 1].active = value && _settings.Channels[chNum].Routing != "Left";
        }
        else if (prop == "Routing")
        {
            _settings.Channels[chNum].Routing = parts[2];
            if (_settings.Channels[chNum].Continuous)
            {
                _signalManager.channels[2 * chNum].active = _settings.Channels[chNum].Routing != "Right";
                _signalManager.channels[2 * chNum + 1].active = _settings.Channels[chNum].Routing != "Left";
            }
        }
        else if (prop == "Freq")
        {
            var freq = float.Parse(parts[2]);
            var level = float.Parse(parts[3]);

            _settings.Channels[chNum].Freq = freq;
            _settings.Channels[chNum].Level = level;

            (_signalManager.channels[2 * chNum].waveform as FM).Carrier_Hz = _settings.Channels[chNum].Freq;
            _signalManager.channels[2 * chNum].level.Value = level + dBHL_tables[chNum].HL_To_SPL(_settings.Channels[chNum].Freq);

            (_signalManager.channels[2 * chNum + 1].waveform as FM).Carrier_Hz = _settings.Channels[chNum].Freq;
            _signalManager.channels[2 * chNum + 1].level.Value = level + dBHL_tables[chNum].HL_To_SPL(_settings.Channels[chNum].Freq);
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
            _signalManager.channels[2 * _pulsedChannelIndex].SetActive(_settings.Channels[_pulsedChannelIndex].Routing != "Right");
            _signalManager.channels[2 * _pulsedChannelIndex + 1].SetActive(_settings.Channels[_pulsedChannelIndex].Routing != "Left");
        }
    }

    private void SetDuration(string data)
    {
        _settings.Duration = float.Parse(data);


        foreach (var ch in _signalManager.channels)
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
            case "Pulsed":
                UpdatePulsedChannelIndex(data);
                break;
            case "SetDuration":
                SetDuration(data);
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
