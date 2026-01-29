using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

using KLib.Signals;
using KLib.Signals.Waveforms;

using LDL;

[RequireComponent(typeof(SliderAnimator))]
public class LDLLevelSlider : MonoBehaviour
{
    [SerializeField] private Image _fill;

    private Slider _slider;
    private SliderAnimator _mover;


    private Action<float> _paramSetter;

    private SliderSettings _settings = null;

    private bool _isActive = true;
    private bool _hasMoved = false;
    private bool _firstMove;

    private SignalManager _signalManager;
    private Channel _myChannel;
    private bool _audioInitialized = false;

    private float _bandWidth;
    private float _minLevel;
    private float _modDepth_pct;

    public UnityAction SliderMoved;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        _mover = GetComponent<SliderAnimator>();

        _isActive = false;
    }

    public void InitializeStimulusGeneration(Channel ch, float bandWidth, float minLevel, float modDepth)
    {
        _myChannel = ch;
        _myChannel.Name = this.name;
        _bandWidth = bandWidth;
        _minLevel = minLevel;
        _modDepth_pct = modDepth;

        var audioConfig = AudioSettings.GetConfiguration();
        _signalManager = new SignalManager(audioConfig.sampleRate, audioConfig.dspBufferSize);
        _signalManager.AdapterMap = HardwareInterface.AdapterMap;

        _signalManager.AddChannel(ch);

        _paramSetter = _myChannel.GetParamSetter("Level");

        _audioInitialized = true;
    }

    public void ResetSlider(TestCondition test)
    {
        _firstMove = true;
        _isActive = true;
        _hasMoved = false;

        _settings = new SliderSettings();
        _settings.var = "Level";
        _settings.ear = test.ear;
        _settings.Freq_Hz = test.Freq_Hz;

        if (test.discomfortLevel.Count == 0 || float.IsNaN(test.discomfortLevel[^1]))
        {
            _settings.min = _minLevel;
            _settings.max = float.PositiveInfinity;
            _settings.start = _settings.min + UnityEngine.Random.Range(0f, 15f);
        }
        else
        {
            float lastLDL = test.discomfortLevel[^1];

            _settings.min = lastLDL - UnityEngine.Random.Range(30f, 60f);
            _settings.max = lastLDL + UnityEngine.Random.Range(10f, 40f);
            _settings.start = _settings.min + UnityEngine.Random.Range(0f, 10f);
        }

        if (_settings.Freq_Hz > 0f)
        {
            if (_bandWidth == 0)
            {
                var fm = new FM();
                fm.Carrier_Hz = _settings.Freq_Hz;
                fm.Depth_Hz = _settings.Freq_Hz * _modDepth_pct / 100f;
                _myChannel.waveform = fm;
            }
            else
            {
                var wf = new Noise()
                {
                    filter = new FilterSpec()
                    {
                        shape = KLib.Signals.Enumerations.FilterShape.Band_pass,
                        CF = _settings.Freq_Hz,
                        BW = _bandWidth,
                        bandwidthMethod = KLib.Signals.Enumerations.BandwidthMethod.Octaves
                    }
                };
                _myChannel.waveform = wf;
            }
        }
        else
        {
            _myChannel.waveform = new Noise();
        }

        _myChannel.Laterality = _settings.ear;

        _paramSetter(_settings.start);
        _signalManager.Initialize();
        _signalManager.StartPaused();

        _settings.max = Mathf.Min(_settings.max, _myChannel.GetMaxLevel());
        _slider.value = (_settings.start - _settings.min) / (_settings.max - _settings.min);
        _settings.isMaxed = false;
        _slider.enabled = true;

        Lock(false);
    }

    public void OnPointerDown(BaseEventData data)
    {
        if (_isActive)
        {
            _signalManager.Unpause();
        }
    }

    public void OnPointerUp(BaseEventData data)
    {
        if (_isActive)
        {
            _signalManager.Pause();
        }
    }

    public bool HasMoved
    {
        get { return _hasMoved || !_slider.gameObject.activeSelf; }
    }

    public SliderAnimator Mover
    {
        get { return _mover; }
    }

    public SliderSettings Settings
    {
        get { return _settings; }
    }

    public void Lock(bool isLocked)
    {
        _fill.gameObject.SetActive(isLocked);
        _slider.interactable = !isLocked;
        _isActive = !isLocked;
    }

    public void DefaultState()
    {
        _fill.gameObject.SetActive(false);
        _slider.value = 0.5f;
        _slider.enabled = false;
        _isActive = false;
    }

    public void OnValueChange(float value)
    {
        if (_settings != null && !_firstMove)
        {
            _settings.end = _slider.value * (_settings.max - _settings.min) + _settings.min;

            _settings.isMaxed = _slider.value > 0.99f;
            _paramSetter(_settings.end);

            if (!_hasMoved && _settings.end != _settings.start)
            {
                _hasMoved = true;
                SliderMoved?.Invoke();
            }
        }

        _firstMove = false;
    }

    public void SimulateMove()
    {
        _slider.value = UnityEngine.Random.Range(0.25f, 0.75f);
        _settings.end = _slider.value * (_settings.max - _settings.min) + _settings.min;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_audioInitialized)
        {
            _signalManager.Synthesize(data);
        }
    }
}
