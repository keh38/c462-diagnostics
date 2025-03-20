using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

using KLib;
using KLib.Signals;
using KLib.Signals.Waveforms;

public class TurandotInteractive : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private GameObject _quitPanel;

    private SignalManager _sigMan;

    private bool _quitPanelShowing = false;
    private bool _audioInitialized = false;

    private UdpClient _udpClient;
    private int _udpPort = 63557;
    private IPEndPoint _udpEndPoint;
    private byte[] _udpData;

    void Start()
    {
        HTS_Server.SetCurrentScene("Turandot Interactive", this);
        _udpClient = new UdpClient();
        _udpEndPoint = new IPEndPoint(IPAddress.Parse(HTS_Server.MyAddress), _udpPort);

        CreateDefaultSignalManager();
    }

    private void Update()
    {
        if (_audioInitialized)
        {
            var amplitudes = _sigMan.CurrentAmplitudes;
            Buffer.BlockCopy(amplitudes, 0, _udpData, 0, _udpData.Length);
            _udpClient.Send(_udpData, _udpData.Length, _udpEndPoint);
        }
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A && !_quitPanelShowing)
        {
            _quitPanelShowing = true;
            _quitPanel.SetActive(true);
        }
    }

    public void OnQuitConfirmButtonClick()
    {
        SceneManager.LoadScene("Home");
    }

    public void OnQuitCancelButtonClick()
    {
        _quitPanel.SetActive(false);
        _quitPanelShowing = false;
    }

    public void OnStartButtonClick()
    {
        StartStreaming();
    }

    public void OnStopButtonClick()
    {
        StopStreaming();
    }

    private void CreateDefaultSignalManager()
    {
        var ch = new Channel()
        {
            Name = "Electric",
            Modality = KLib.Signals.Enumerations.Modality.Electric,
            Laterality = Laterality.Diotic,
            Location = "Site 2",
            waveform = new Digitimer()
            {
                PulseRate_Hz = 100
            },
            gate = new Gate()
            {
                Active = true,
                Delay_ms = 100,
                Duration_ms = 500,
                Period_ms = 1000
            },
            level = new Level()
            {
                Units = LevelUnits.mA,
                Value = 0.2f
            }
        };
        var ch2 = new Channel()
        {
            Name = "Audio",
            Modality = KLib.Signals.Enumerations.Modality.Audio,
            Laterality = Laterality.Left,
            waveform = new Noise(),
            gate = new Gate()
            {
                Active = true,
                Delay_ms = 100,
                Duration_ms = 500,
                Period_ms = 1000
            },
            level = new Level()
            {
                Units = LevelUnits.dB_attenuation,
                Value = -6
            }
        };
        var ch3 = new Channel()
        {
            Name = "Audio",
            Modality = KLib.Signals.Enumerations.Modality.Audio,
            Laterality = Laterality.Right,
            waveform = new Sinusoid(),
            gate = new Gate()
            {
                Active = true,
                Delay_ms = 100,
                Duration_ms = 500,
                Period_ms = 1000
            },
            level = new Level()
            {
                Units = LevelUnits.dB_attenuation,
                Value = -100
            }
        };


        AudioSettings.GetDSPBufferSize(out int bufferLength, out int numBuffers);

        _sigMan = new SignalManager();
        _sigMan.AdapterMap = AdapterMap.Default7point1Map();
        _sigMan.AdapterMap.AudioTransducer = "HD280";
        _sigMan.AddChannel(ch);
        _sigMan.AddChannel(ch2);
        _sigMan.AddChannel(ch3);
        _sigMan.Initialize(AudioSettings.outputSampleRate, bufferLength);
        _sigMan.StartPaused();

        //_udpData = new byte[sizeof(float) * _sigMan.CurrentAmplitudes.Length];

        _audioInitialized = true;
    }

    private void SetParams(string data)
    {
        _audioInitialized = false;

        var settings = FileIO.XmlDeserializeFromString<Turandot.Interactive.InteractiveSettings>(data);
        _sigMan = settings.SigMan;

        AudioSettings.GetDSPBufferSize(out int bufferLength, out int numBuffers);

        _sigMan.AdapterMap = HardwareInterface.AdapterMap;
        _sigMan.Initialize(AudioSettings.outputSampleRate, bufferLength);
        _sigMan.StartPaused();

        _udpData = new byte[sizeof(float) * _sigMan.CurrentAmplitudes.Length];

        var digitimerChannels = _sigMan.channels.FindAll(x => x.waveform.Shape == KLib.Signals.Enumerations.Waveshape.Digitimer && x.Modality == KLib.Signals.Enumerations.Modality.Electric);
        foreach (var d in digitimerChannels)
        {
            if (int.TryParse(d.MyEndpoint.transducer.Substring(3), out int id))
            {
                HardwareInterface.Digitimer.EnableDevice(id, d.waveform as Digitimer);
            }
        }

        _audioInitialized = true;
    }

    private void SetProperty(string data)
    {
        var parts = data.Split('=');
        if (parts.Length == 2)
        {
            var lhs = parts[0].Split(new char[] { '.' }, 2);
            string name = lhs[0];
            string param = lhs[1];
            float value = float.Parse(parts[1]);
            _sigMan.SetParameter(name, param, value);
        }
    }

    private void SetActive(string data)
    {
        var parts = data.Split('=');
        if (parts.Length == 2)
        {
            string name = parts[0];
            bool value = float.Parse(parts[1]) > 0;
            _sigMan[name].active = value;
        }
    }

    private void StartStreaming()
    {
        _sigMan.Unpause();
    }

    private void StopStreaming()
    {
        _sigMan.Pause();
        var digitimerChannels = _sigMan.channels.FindAll(x => x.waveform.Shape == KLib.Signals.Enumerations.Waveshape.Digitimer && x.Modality == KLib.Signals.Enumerations.Modality.Electric);
        foreach (var d in digitimerChannels)
        {
            if (int.TryParse(d.MyEndpoint.transducer.Substring(3), out int id))
            {
                HardwareInterface.Digitimer.DisableDevice(id);
            }
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_audioInitialized)
        {
            _sigMan.Synthesize(data);
        }
    }

    void IRemoteControllable.ProcessRPC(string command, string data)
    {
        Debug.Log("Turandot Interactive: " + command);
        switch (command)
        {
            case "Start":
                StartStreaming();
                break;
            case "Stop":
                StopStreaming();
                break;
            case "SetParams":
                SetParams(data);
                break;
            case "SetProperty":
                SetProperty(data);
                break;
            case "SetActive":
                SetActive(data);
                break;
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

}
