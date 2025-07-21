using LDL;
using System.Diagnostics.Tracing;
using System;
using UnityEngine;
using UnityEngine.UI;

using KLib.Signals;
using KLib.Signals.Waveforms;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AnimateSlider))]
public class LDLLevelSlider : MonoBehaviour
{
    private Slider _slider;
    //private UIButton _thumb;
    private AnimateSlider _mover;

    private Action<float> _paramSetter;
    private AnimationCurve _ioFunction = null;

    //private UISprite _sliderFG;
    //private KEventDelegate _notifyOnSliderMove;

    private SliderSettings _settings = null;
    private float _paramRange;

    private bool _isActive = true;
    private bool _isVisible = true;
    private bool _hasMoved = false;
    private bool _firstMove;

    private SignalManager _signalManager;
    private bool _audioInitialized = false;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        _mover = GetComponent<AnimateSlider>();

        //_thumb = transform.FindChild("Thumb").GetComponent<UIButton>();
        //_sliderFG = transform.FindChild("Foreground").GetComponent<UISprite>();

        //_sigGen = transform.FindChild("Signal Generator").GetComponent<SliderSignalGenerator>();

        _isActive = false;
    }
    void Start()
    {
        //_slider.ThumbOnly = true;

        //UIEventListener fgl = UIEventListener.Get(_thumb.gameObject);
        //fgl.onPress += OnPressThumb;
    }

    //public KEventDelegate NotifyOnSliderMove
    //{
    //    set { _notifyOnSliderMove = value; }
    //}

    public void InitializeStimulusGeneration(Channel ch)
    {
        var audioConfig = AudioSettings.GetConfiguration();
        _signalManager = new SignalManager(audioConfig.sampleRate, audioConfig.dspBufferSize);
        _signalManager.AdapterMap = HardwareInterface.AdapterMap;

        _signalManager.AddChannel(ch);
        _signalManager.Initialize();

        _signalManager.StartPaused();

        _audioInitialized = true;
    }

    public void OnPointerDown(BaseEventData data)
    {
        _signalManager.Unpause();
        //_varyMin = _levelSlider.InteractWith == KRangeSlider.InteractionState.Low;
        //UpdateStimulusLevel();
        //_signalManager.ResetSweepables();
        //_audioEnabled = _levelSlider.InteractWith == KRangeSlider.InteractionState.Low || _levelSlider.InteractWith == KRangeSlider.InteractionState.High;
    }

    public void OnPointerUp(BaseEventData data)
    {
        _signalManager.Pause();
        //_audioEnabled = false;
    }

    public bool HasMoved
    {
        get { return _hasMoved || !_isVisible; }
    }

    public bool IsVisible
    {
        get { return _isVisible; }
    }

    public AnimateSlider Mover
    {
        get { return _mover; }
    }

    //public Channel Channel
    //{
    //    get { return _sigGen.Channel; }
    //}

    public void Show(bool isVisible)
    {
        //this._isVisible = isVisible;
        //NGUITools.SetActive(this.gameObject, isVisible);
    }

    //public ParamSliderSettings Settings
    //{
    //    get { return _settings; }
    //}

    public void Lock(bool isLocked)
    {
        //_sliderFG.color = (isLocked) ? Color.green : Color.white;
        //_slider.enabled = !isLocked;
        //_isActive = !isLocked;
    }

    public void DefaultState()
    {
        //_sliderFG.color = Color.white;
        _slider.value = 0.5f;
        _slider.enabled = false;
        _isActive = false;
    }

    public void ApplySettings(SliderSettings settings)
    {
        ApplySettings(settings, null);
    }
    public void ApplySettings(SliderSettings settings, AnimationCurve ioFunc)
    {
        _firstMove = true;
        _isVisible = true;
        _isActive = true;
        _hasMoved = false;

        Lock(false);
        //NGUITools.SetActive(this.gameObject, true);

        _settings = settings;

        _paramRange = Mathf.Round(settings.max - settings.min);
        //_slider.numberOfSteps = (int)Mathf.Round(_paramRange) + 1;

        _slider.value = (settings.start - settings.min) / _paramRange;
        _settings.isMaxed = false;

        //_sigGen.Reset();

        //_paramSetter = _sigGen.Channel.ParamSetter(_settings.var);
        _paramSetter(_settings.start);

        _ioFunction = ioFunc ?? DefaultInputOutputFunction();
    }

    private AnimationCurve DefaultInputOutputFunction()
    {
        AnimationCurve ac = new AnimationCurve();
        ac.AddKey(new Keyframe(0, _settings.min));
        ac.AddKey(new Keyframe(1, _settings.max));
        //ac.MakePiecewiseLinear();
        return ac;
    }

    public void ResetFirstMove()
    {
        _firstMove = true;
    }

    public void OnValueChange()
    {
        if (_settings != null && !_firstMove)
        {
            _settings.end = _ioFunction.Evaluate(_slider.value);
            _settings.isMaxed = _slider.value > 0.99f;
            _paramSetter(_settings.end);

            if (!_hasMoved && _settings.end != _settings.start)
            {
                _hasMoved = true;
                //if (_notifyOnSliderMove != null)
                //    _notifyOnSliderMove();
            }
        }

        _firstMove = false;
    }

    public void SimulateMove()
    {
        _slider.value = 0.9f;
        _settings.end = _slider.value * _paramRange + _settings.min;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_audioInitialized)
        {
            _signalManager.Synthesize(data);
        }
    }
}
