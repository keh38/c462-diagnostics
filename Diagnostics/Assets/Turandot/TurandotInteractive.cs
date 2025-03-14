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

    private AdapterMap CreateAdapterMap()
    {
        var map = new AdapterMap();

        map.Add("Audio", "Left", "Front-Left");
        map.Add("Audio", "Right", "Front-Right");
        //map.Add("Electric", "1", "Center");
        //map.Add("Electric", "2", "Subwoofer");
        //map.Add("None", "0", "Side-Left");
        //map.Add("None", "1", "Side-Right");
        //map.Add("Haptic", "1", "Rear-Left");
        //map.Add("Haptic", "2", "Rear-Right");

        return map;
    }

    private void CreateDefaultSignalManager()
    {
        var ch = new Channel()
        {
            Name = "Audio",
            Adapter = "Audio",
            Laterality = Laterality.Diotic,
            waveform = new Sinusoid()
            {
                Frequency_Hz = 300
            },
            modulation = new KLib.Signals.Modulations.SinusoidalAM()
            {
                Frequency_Hz = 40,
                Depth = 1
            },
            gate = new Gate()
            {
                Active = true,
                Duration_ms = 500,
                Period_ms = 1000
            },
            level = new Level()
            {
                Units = LevelUnits.dB_attenuation,
                Value = -20
            }
        };

        AudioSettings.GetDSPBufferSize(out int bufferLength, out int numBuffers);

        //_sigMan = new SignalManager(CreateAdapterMap());
        _sigMan = new SignalManager();
        _sigMan.AdapterMap = CreateAdapterMap();
        _sigMan.AddChannel(ch);
        _sigMan.Initialize(AudioSettings.outputSampleRate, bufferLength);
        //_sigMan.StartPaused();

        //_udpData = new byte[sizeof(float) * _sigMan.CurrentAmplitudes.Length];

        //_audioInitialized = true;
    }

    private void SetParams(string data)
    {
        _audioInitialized = false;

        _sigMan = FileIO.XmlDeserializeFromString<SignalManager>(data);

        AudioSettings.GetDSPBufferSize(out int bufferLength, out int numBuffers);

        _sigMan.AdapterMap = CreateAdapterMap();
        _sigMan.Initialize(AudioSettings.outputSampleRate, bufferLength);
        _sigMan.StartPaused();

        _udpData = new byte[sizeof(float) * _sigMan.CurrentAmplitudes.Length];

        _audioInitialized = true;
    }

    private void SetParameter(string data)
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

    private void StartStreaming()
    {
        _sigMan.Unpause();
    }

    private void StopStreaming()
    {
        _sigMan.Pause();
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
            case "SetParameter":
                SetParameter(data);
                break;
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

}
