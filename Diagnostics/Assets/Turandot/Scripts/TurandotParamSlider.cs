using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using KLib.Signals;
using KLib.Signals.Enumerations;
using KLib.Signals.Waveforms;
using Turandot.Inputs;
using Turandot.Screen;
using Input = Turandot.Inputs.Input;

namespace Turandot.Scripts
{
    public class TurandotParamSlider : TurandotInput
    {
        [SerializeField] private TMPro.TMP_Text _label;
        [SerializeField] private TMPro.TMP_Text _leftLabel;
        [SerializeField] private TMPro.TMP_Text _rightLabel;
        [SerializeField] private Slider _slider;
        [SerializeField] private GameObject _button;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _fillImage;

        private ParamSliderLayout _layout = new ParamSliderLayout();
        private ParamSliderAction _action = null;

        private SignalManager _sigMan;
        private Action<float> _paramSetter = null;

        private float _value;
        private List<float> _values = new List<float>();
        private float _minVal;
        private float _maxVal;
        private float _range;

        public override string Name { get { return _layout.Name; } }
        public ButtonData ButtonData { get; private set; }

        private string _result;
        public override string Result { get { return _result; } }

        public void Initialize(ParamSliderLayout layout)
        {
            _layout = layout;
            LayoutControl();
            ButtonData = new ButtonData() { name = layout.Name };

            _log = new InputLog(layout.Name);
        }

        private void LayoutControl()
        {
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(_layout.X, _layout.Y);
            rt.anchorMax = new Vector2(_layout.X, _layout.Y);
            rt.sizeDelta = new Vector2(_layout.Width, _layout.Height);
        }
        
        public void ClearLog()
        {
            _log.Clear();
        }

        override public void Activate(Input input, TurandotAudio audio)
        {
            var action = input as ParamSliderAction;
            _label.fontSize = action.FontSize;
            _leftLabel.fontSize = action.FontSize;
            _rightLabel.fontSize = action.FontSize;

            _label.text = action.Label;
            _leftLabel.text = action.MinLabel;
            _rightLabel.text = action.MaxLabel;

            if (_layout.ButtonStyle == ParamSliderButtonStyle.Mobile)
            {
                _button.SetActive(false);
            }

            _fillImage.raycastTarget = action.BarClickable;
            _backgroundImage.raycastTarget = action.BarClickable;

            _fillImage.enabled = !action.ThumbOnly;

            ButtonData.value = false;

            _result = "";

            if (action.BeginVisible)
            {
                _action = action;
                _sigMan = audio.SigMan;

                //Debug.Log($"wtf {_action.Channel} {_action.Property}");
                //_noise = _sigMan[_action.channel].waveform as Noise;

                //_setFilters = _noise != null && _action.parameter == "Frequency";
                //_paramSetter = _sigMan[_action.channel].ParamSetter(_setFilters ? "CF" : _action.parameter);
                _paramSetter = _sigMan[_action.Channel].GetParamSetter(_action.Property);
                if (_action.Property.Contains("Digitimer.Demand"))
                {
                   _paramSetter += x => this.UpdateDigitimer(x);
                }

                if (_paramSetter == null)
                {
                    throw new Exception($"{_action.Channel}.{_action.Property} not found");
                }

                //if (_action.parameter == "Level")
                //{
                //    _sigMan[_action.channel].ClampLevelToMax(true);
                //}

                //if (_ncalls == 0)
                //{
                _value = _action.StartValue;
                _minVal = _action.Min;
                _maxVal = _action.Max;
                //}

                if (_action.Scale == ParamSliderAction.SliderScale.Log)
                {
                    //if (_ncalls > 0)
                    //{
                    //    if (_action.shrinkFactor > 0)
                    //    {
                    //        float newRange = (1 - _action.shrinkFactor / 100) * Mathf.Log(_maxVal / _minVal); // number of log units
                    //        float dv = UnityEngine.Random.Range(0.5f * (1 - _action.startRange / 100), 0.5f * (1 + _action.startRange / 100));
                    //        _minVal = Mathf.Max(_minVal, _value * Mathf.Exp(-dv * newRange));
                    //        _maxVal = Mathf.Min(_maxVal, _minVal * Mathf.Exp(newRange));
                    //        _minVal = _maxVal * Mathf.Exp(-newRange);

                    //        _value = Mathf.Exp(UnityEngine.Random.Range(0f, 1f) * newRange) * _minVal;
                    //    }
                    //    else
                    //    {
                    //        _value = Mathf.Pow(2, UnityEngine.Random.Range(-0.5f, 0.5f)) * _action.startVal;
                    //    }
                    //    Debug.Log("random value = " + _value);

                    //    Debug.Log("Min=" + _minVal + "; Max=" + _maxVal);
                    //}
                    _range = Mathf.Log(_maxVal / _minVal);
                    _slider.SetValueWithoutNotify(Mathf.Log(_value / _minVal) / _range);
                }
                else
                {
                    _range = _maxVal - _minVal;
                    _slider.SetValueWithoutNotify((_value - _minVal) / _range);
                }

                //_action.thumbTogglesSound = false;
                //if (_action.thumbTogglesSound) _sigMan.StartPaused();
                //ApplyValue();
                _paramSetter?.Invoke(_value);
                _log.Add(Time.timeSinceLevelLoad, _value);

                //++_ncalls;

                //_values.Add(_value);

                //_ignoreEvents = false;
            }

            //button.IsVisible = false;
            //_xmin = input.X - (float)(slider.foregroundWidget.width - button.Width) / 2;
            //_xmax = input.X + (float)(slider.foregroundWidget.width - button.Width) / 2;

            base.Activate(input, audio);
        }

        private void UpdateDigitimer(float value)
        {
            HardwareInterface.Digitimer?.EnableDevices(_sigMan.GetDigitimerChannels());
        }

        override public void Deactivate()
        {
            ButtonData.value = false;
            base.Deactivate();
        }

        public void OnSliderValueChanged(float sliderValue)
        {
            if (_action != null)
            {
                if (_action.Scale == ParamSliderAction.SliderScale.Log)
                {
                    _value = Mathf.Exp(sliderValue * _range) * _minVal;
                }
                else
                {
                    _value = sliderValue * _range + _minVal;

                }
                _paramSetter?.Invoke(_value);
                _log.Add(Time.timeSinceLevelLoad, _value);
            }
        }

        public void OnPointerDown(BaseEventData data)
        {
            if (_layout.ButtonStyle == ParamSliderButtonStyle.Mobile)
            {
                _button.SetActive(false);
            }
        }

        public void OnPointerUp(BaseEventData data)
        {
            if (_layout.ButtonStyle == ParamSliderButtonStyle.Mobile)
            {
                _button.SetActive(true);
            }
        }

        public void OnButtonClick()
        {
            ButtonData.value = true;
            _result = _value.ToString();
        }
    }
}