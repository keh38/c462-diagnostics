using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using KLib.Signals;
using Turandot.Inputs;
using Turandot.Screen;
using Input = Turandot.Inputs.Input;
using UnityEditor.Experimental.GraphView;

namespace Turandot.Scripts
{
    public class CombinedLevelSlider : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text _label;
        [SerializeField] private TMPro.TMP_Text _leftLabel;
        [SerializeField] private TMPro.TMP_Text _rightLabel;
        [SerializeField] private Slider _slider;
        [SerializeField] private RectTransform _thumbRectTransform;
        [SerializeField] private GameObject _button;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _subthresholdImage;
        [SerializeField] private Image _tooLoudImage;
        [SerializeField] private TMPro.TMP_Text _promptText;
        [SerializeField] private TMPro.TMP_Text _messageText;

        private RectTransform _buttonRectTransform;

        private Action<float> _paramSetter = null;

        private enum Phase { Threshold, LDL, Finished}
        private Phase _phase;

        private float _value;
        private List<float> _values = new List<float>();
        private float _minVal;
        private float _maxVal;
        private float _range;
        private float _minExcursionSize;
        private int _minNumOfReversals;

        private int _reversals = 0;
        private int _direction;
        private float _lastExtremum;

        private float _thresholdSliderValue;

        private void Awake()
        {
            _buttonRectTransform = _button.GetComponent<RectTransform>();
        }

        private void Start()
        {
            _promptText.text = "";
            _messageText.gameObject.SetActive(false);
            _subthresholdImage.fillAmount = 0;
            _tooLoudImage.fillAmount = 0;
            _slider.value = 0;
            _button.gameObject.SetActive(false);
        }

        public void Initialize(float minExcursionSize, int minNumReversals)
        {
            _minExcursionSize = minExcursionSize;
            _minNumOfReversals = minNumReversals;

            _slider.value = 0;
            _subthresholdImage.raycastTarget = false;
            _backgroundImage.raycastTarget = false;
        }


        public void ClearLog()
        {
            //_log.Clear();
        }

        public void Activate(float min, float max)
        {
            _minVal = min;
            _maxVal = max;
            _range = max - min;

            _phase = Phase.Threshold;
            _promptText.text = "Adjust the slider until you just barely hear the sound";

            _reversals = 0;
            _direction = 1;
            _lastExtremum = float.NegativeInfinity;

            _slider.value = 0;
            _button.SetActive(false);

            _thresholdSliderValue = 0;
            _subthresholdImage.fillAmount = 0;
            _tooLoudImage.fillAmount = 0;
            _messageText.gameObject.SetActive(false);
        }

        //override public void Deactivate()
        //{
        //    ButtonData.value = false;
        //    base.Deactivate();
        //}

        public void OnSliderValueChanged(float sliderValue)
        {
            if (sliderValue < _thresholdSliderValue)
            {
                _slider.value = _thresholdSliderValue;
                return;
            }

            _value = sliderValue * _range + _minVal;


            CheckForReversal(_value);

            //if (_action != null)
            //{
            //    if (_action.Scale == ParamSliderAction.SliderScale.Log)
            //    {
            //        _value = Mathf.Exp(sliderValue * _range) * _minVal;
            //    }
            //    else
            //    {
            //        _value = sliderValue * _range + _minVal;

            //    }
            //    _paramSetter?.Invoke(_value);
            //    _log.Add(Time.timeSinceLevelLoad, _value);
            //}
        }

        private void CheckForReversal(float newValue)
        {
            if (_direction == 1) // Increasing level
            {
                if (newValue > _lastExtremum) // still going up
                {
                    _lastExtremum = newValue;
                    return;
                }

                if (newValue <= _lastExtremum - _minExcursionSize) // reversal to decreasing
                {
                    _direction = -1;
                    _lastExtremum = newValue;
                    return;
                }
                return;
            }

            if (_direction == -1) // Decreasing level
            {
                if (newValue < _lastExtremum) // still going down
                {
                    _lastExtremum = newValue;
                    return;
                }

                if (newValue >= _lastExtremum + _minExcursionSize) // reversal to increasing
                {
                    Debug.Log("Reversal!");
                    _reversals++;
                    _direction = 1;
                    _lastExtremum = newValue;
                    return;
                }
            }
        }

        public void OnPointerDown(BaseEventData data)
        {
            _messageText.gameObject.SetActive(false);
            _button.SetActive(false);
        }

        public void OnPointerUp(BaseEventData data)
        {
            if (_reversals < _minNumOfReversals)
            {
                _messageText.gameObject.SetActive(true);
                return;
            }

            _buttonRectTransform.anchorMin = new Vector2(_thumbRectTransform.anchorMin.x, -1);
            _buttonRectTransform.anchorMax = new Vector2(_thumbRectTransform.anchorMin.x, -1);
            _button.SetActive(true);
        }

        public void OnButtonClick()
        {
            _button.SetActive(false);

            if (_phase == Phase.Threshold)
            {
                _phase = Phase.LDL;
                _promptText.text = "Adjust the slider to the loudest level you can tolerate";
                _thresholdSliderValue = _slider.value;
                _subthresholdImage.fillAmount = _slider.value;
                return;
            }

            _phase = Phase.Finished;
            _tooLoudImage.fillAmount = (1 - _slider.value);
            _backgroundImage.color = 0.75f * Color.green;
            _promptText.text = "";
            _thumbRectTransform.gameObject.SetActive(false);  

            //ButtonData.value = true;
            //_result = $"{_action.Property}={_value};";
        }
    }
}