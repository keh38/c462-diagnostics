using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using KLib;
using KLib.Signals;
using KLib.Signals.Waveforms;

using Turandot.Inputs;
using Turandot.Interactive;

public class TurandotInteractive : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private GameObject _titleBar;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _quitPanel;
    [SerializeField] private GameObject _sliderPanelPrefab;
    [SerializeField] private GameObject _sliderPrefab;
    [SerializeField] private GameObject _sliderArea;

    private SignalManager _sigMan;

    private bool _quitPanelShowing = false;
    private bool _audioInitialized = false;

    private UdpClient _udpClient;
    private int _udpPort = 63557;
    private IPEndPoint _udpEndPoint;

    private UDPPacket _udpPacket = new UDPPacket();
    private bool _isRemoteConnected;

    private List<ParameterSlider> _sliders;
    private List<SliderPanel> _sliderPanels;

    void Start()
    {
        HTS_Server.SetCurrentScene("Turandot Interactive", this);
        _udpClient = new UdpClient();
        _isRemoteConnected = HTS_Server.RemoteConnected;
        if (_isRemoteConnected)
        {
            _udpEndPoint = new IPEndPoint(HTS_Server.RemoteAddress, _udpPort);
        }

#if HACKING
        GameManager.SetSubject("Scratch/_Ken");
        GameManager.DataForNextScene = "GateTest";
#endif

        if (!string.IsNullOrEmpty(GameManager.DataForNextScene))
        {
            var param = FileIO.XmlDeserialize<InteractiveSettings>(FileLocations.ConfigFile("Interactive", GameManager.DataForNextScene));
            param.ShowSliders = true;
            ApplyParameters(param);

            InitializeSliders(param.Sliders);
        }
        else
        {
            _menuPanel.SetActive(false);
            var rt = _titleBar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, rt.anchorMin.y);
        }

    }

    private void Update()
    {
        if (_audioInitialized && _isRemoteConnected)
        {
            _udpPacket.Status = 1;
            _udpPacket.SetAmplitudes(_sigMan.CurrentAmplitudes);
            for (int k = 0; k < _sliders.Count; k++)
            {
                _udpPacket.Values[k] = _sliders[k].SelfChange ? _sliders[k].Value : float.NaN;
            }
            for (int k = 0; k < _sliderPanels.Count; k++)
            {
                _udpPacket.Active[k] = _sliderPanels[k].SelfChange ? _sliderPanels[k].IsActive : -1;
            }
            _udpPacket.UpdateByteArray();
            _udpClient.Send(_udpPacket.ByteArray, _udpPacket.ByteArray.Length, _udpEndPoint);
        }
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A && !_quitPanelShowing)
        {
            _sliderArea.SetActive(false);
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
        _sliderArea.SetActive(true);
        _quitPanel.SetActive(false);
        _quitPanelShowing = false;
    }

    public void OnStartButtonToggle(bool ispressed)
    {
        if (ispressed)
        {
            StartStreaming();
        }
        else
        {
            StopStreaming();
        }
    }

    public void OnBackButtonClick()
    {
        _sliderArea.SetActive(false);
        _quitPanelShowing = true;
        _quitPanel.SetActive(true);
    }

    private void CreateDefaultSignalManager()
    {
        var ch = new Channel()
        {
            Name = "Audio",
            Modality = KLib.Signals.Enumerations.Modality.Audio,
            Laterality = Laterality.Diotic,
            Location = "Site 2",
            waveform = new Sinusoid()
            {
                Frequency_Hz = 500
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
                Units = LevelUnits.dB_attenuation,
                Value = -20
            }
        };
        AudioSettings.GetDSPBufferSize(out int bufferLength, out int numBuffers);

        _sigMan = new SignalManager();
        _sigMan.AdapterMap = AdapterMap.DefaultStereoMap();
        _sigMan.AdapterMap.AudioTransducer = "HD280";
        _sigMan.AddChannel(ch);
        _sigMan.Initialize(AudioSettings.outputSampleRate, bufferLength);
        _sigMan.StartPaused();

        _audioInitialized = true;
    }

    private void InitializeSliders(List<ParameterSliderProperties> properties)
    {
        _sliderArea.GetComponent<FlowLayout>().Clear();

        if (properties.Count == 0) return;

        var groups = _sigMan.channels.Select(x => x.Name).ToList();

        _sliderPanels = new List<SliderPanel>();
        _sliders = new List<ParameterSlider>();
        foreach (var g in groups)
        {
            var sliderPanel = InitializeSliderPanel(g, properties.FindAll(x => x.Channel.Equals(g)));
            _sliderPanels.Add(sliderPanel);
            _sliders.AddRange(sliderPanel.Sliders);
        }
    }

    private SliderPanel InitializeSliderPanel(string name, List<ParameterSliderProperties> properties)
    {
        var panelObj = GameObject.Instantiate(_sliderPanelPrefab, _sliderArea.transform);

        var sliderPanel = panelObj.GetComponent<SliderPanel>();
        sliderPanel.SetTitle(name);
        sliderPanel.Setter = _sigMan[name].GetActiveSetter();

        foreach (var prop in properties)
        {
            var gobj = GameObject.Instantiate(_sliderPrefab, panelObj.transform);
            var slider = gobj.GetComponent<ParameterSlider>();
            slider.Initialize(prop);
            slider.Setter = _sigMan.GetParamSetter(prop.FullParameterName);
            slider.Setter?.Invoke(prop.StartValue);

            if (prop.Property.Contains("Digitimer.Demand") || prop.Property.Contains("Digitimer.PulseWidth"))
            {
                slider.Setter += x => this.UpdateDigitimer(x);
            }

            sliderPanel.AddSlider(slider);
        }

        var flowLayout = _sliderArea.GetComponent<FlowLayout>();
        flowLayout.Add(panelObj);

        return sliderPanel;
    }

    private void UpdateDigitimer(float value)
    {
        HardwareInterface.Digitimer?.EnableDevices(_sigMan.GetDigitimerChannels());
    }

    private void ApplyParameters(InteractiveSettings settings)
    {
        _audioInitialized = false;

        _sigMan = settings.SigMan;

        AudioSettings.GetDSPBufferSize(out int bufferLength, out int numBuffers);

        _sigMan.AdapterMap = HardwareInterface.AdapterMap;
        _sigMan.Initialize(AudioSettings.outputSampleRate, bufferLength);
        _sigMan.StartPaused();

        //HardwareInterface.Digitimer?.EnableDevices(_sigMan.GetDigitimerChannels());

        _audioInitialized = true;
    }

    private void SetParams(string data)
    {
        try
        {
            var settings = FileIO.XmlDeserializeFromString<InteractiveSettings>(data);
            ApplyParameters(settings);
            InitializeSliders(settings.Sliders);
            _sliderArea.SetActive(settings.ShowSliders);
        }
        catch (Exception ex)
        {
            Debug.Log($"[Turandot Interactive] error setting parameters: {ex.Message}");
            HTS_Server.SendMessage("TurandotInteractive", "Error:failed to set parameters...check log");
        }
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

            _sliders.Find(x => x.FullParameterName.Equals(parts[0]))?.SetValue(value);

            if (param.StartsWith("Digitimer.Demand") || param.StartsWith("Digitimer.PulseWidth"))
            {
                HardwareInterface.Digitimer?.EnableDevices(_sigMan.GetDigitimerChannels());
            }
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
            _sliderPanels.Find(x => x.ChannelName == name).SetChannelActive(value);
        }
    }

    private void StartStreaming()
    {
        HardwareInterface.Digitimer?.EnableDevices(_sigMan.GetDigitimerChannels());
        _sigMan.Unpause();
    }

    private void StopStreaming()
    {
        _sigMan.Pause();
        HardwareInterface.Digitimer?.DisableDevices(_sigMan.GetDigitimerChannels());
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
                Debug.Log($"property: {data}");
                SetProperty(data);
                break;
            case "SetActive":
                SetActive(data);
                break;
            case "ShowSliders":
                _sliderArea.SetActive(data.Equals("True"));
                break;
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

}
