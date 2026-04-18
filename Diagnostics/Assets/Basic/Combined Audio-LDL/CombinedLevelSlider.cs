using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using KLib.Signals;
using CombinedAudioLDL;

public enum SliderMeasurement { Threshold, LDL }

public class CombinedLevelSlider : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _label;
    [SerializeField] private TMPro.TMP_Text _leftLabel;
    [SerializeField] private TMPro.TMP_Text _rightLabel;
    [SerializeField] private Slider _slider;
    [SerializeField] private RectTransform _thumbRectTransform;
    [SerializeField] private GameObject _button;
    [SerializeField] private GameObject _maxButton;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _subthresholdImage;
    [SerializeField] private Image _tooLoudImage;
    [SerializeField] private TMPro.TMP_Text _promptText;
    [SerializeField] private TMPro.TMP_Text _messageText;

    public Action<SliderMeasurement, float> MeasurementLocked;
    public Action<float> ParamSetter { get; set; }

    internal enum ButtonPress
    {
        None = -1,
        ThumbDown = 0,
        ThumbUp = 1,
        Threshold = 2,
        LDL = 3,
        Max = 4
    }

    public CombinedSliderLog Log { get; private set; } 

    private RectTransform _buttonRectTransform;
    private TMPro.TMP_Text _buttonText;
    private TMPro.TMP_Text _maxButtonText;

    private enum Phase { Threshold, LDL, Finished }
    private Phase _phase;

    private float _value;
    private float _minVal;
    private float _maxVal;
    private float _range;
    private float _minExcursionSize;
    private int _minNumReversals;

    private int _reversals = 0;
    private int _direction;
    private float _lastExtremum;

    private float _thresholdSliderValue;
    private Color _defaultBackgroundColor;

    private void Awake()
    {
        _buttonRectTransform = _button.GetComponent<RectTransform>();
        _buttonText = _button.GetComponentInChildren<TMPro.TMP_Text>();
        _maxButtonText = _maxButton.GetComponentInChildren<TMPro.TMP_Text>();
        _defaultBackgroundColor = _backgroundImage.color;
    }

    private void Start()
    {
        _promptText.text = "";
        _messageText.gameObject.SetActive(false);
        _subthresholdImage.fillAmount = 0;
        _tooLoudImage.fillAmount = 0;
        _slider.value = 0;
        _button.gameObject.SetActive(false);
        _maxButton.gameObject.SetActive(false);
    }

    public void Initialize(float minExcursionSize, int minNumReversals)
    {
        _minExcursionSize = minExcursionSize;
        _minNumReversals = minNumReversals;

        _slider.value = 0;
        _subthresholdImage.raycastTarget = false;
        _backgroundImage.raycastTarget = false;
    }

    public void Activate(float min, float max)
    {
        Log = new CombinedSliderLog();

        _minVal = min;
        _maxVal = max;
        _range = max - min;

        Debug.Log($"Slider activated with range [{_minVal}, {_maxVal}]");   

        _phase = Phase.Threshold;
        _promptText.text = "Adjust the slider until you just barely hear the sound";
        _buttonText.text = "I just hear it";
        _maxButtonText.text = "Still can't hear it";

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

    public void Reset()
    {
        Debug.Log("Slider reset");
        _subthresholdImage.fillAmount = 0;
        _tooLoudImage.fillAmount = 0;
        _slider.value = 0;
        _thumbRectTransform.gameObject.SetActive(true);
        _thresholdSliderValue = 0;
        _slider.SetValueWithoutNotify(0);
        _backgroundImage.color = _defaultBackgroundColor;
    }

    public void OnSliderValueChanged(float sliderValue)
    {
        if (sliderValue < _thresholdSliderValue)
        {
            _slider.value = _thresholdSliderValue;
            return;
        }

        _value = sliderValue * _range + _minVal;
        Log?.Add(sliderValue, _value);

        CheckForReversal(_value);

        _value = sliderValue * _range + _minVal;
        ParamSetter?.Invoke(_value);
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
                _reversals++;
                _direction = 1;
                _lastExtremum = newValue;
                return;
            }
        }
    }

    public void OnPointerDown(BaseEventData data)
    {
        Log?.Add(button: (int)ButtonPress.ThumbDown);
        _messageText.gameObject.SetActive(false);
        _button.SetActive(false);
        _maxButton.SetActive(false);
    }

    public void OnPointerUp(BaseEventData data)
    {
        Log?.Add(button: (int)ButtonPress.ThumbUp);
        if (_maxVal - _value < _minExcursionSize)
        {
            _maxButton.SetActive(true);
        }

        if (_reversals < _minNumReversals)
        {
            _messageText.gameObject.SetActive(true);
            return;
        }

        _buttonRectTransform.anchorMin = new Vector2(_thumbRectTransform.anchorMin.x, _buttonRectTransform.anchorMin.y);
        _buttonRectTransform.anchorMax = new Vector2(_thumbRectTransform.anchorMin.x, _buttonRectTransform.anchorMin.y);
        _button.SetActive(true);
    }

    public void ButtonClick()
    {
        _button.SetActive(false);
        _maxButton.SetActive(false);
        _messageText.gameObject.SetActive(false);

        _value = _slider.value * _range + _minVal;

        if (_phase == Phase.Threshold)
        {
            _phase = Phase.LDL;
            _reversals = 0;
            _promptText.text = "Adjust the slider to the loudest level you can tolerate";
            _buttonText.text = "No louder than this";
            _maxButtonText.text = "Could go higher";
            _thresholdSliderValue = _slider.value;
            _subthresholdImage.fillAmount = _slider.value;
            Log?.Add(button: (int)ButtonPress.Threshold);
            MeasurementLocked?.Invoke(SliderMeasurement.Threshold, _value);
            return;
        }

        _phase = Phase.Finished;
        _tooLoudImage.fillAmount = (1 - _slider.value);
        _backgroundImage.color = 0.75f * Color.green;
        _promptText.text = "";
        _thumbRectTransform.gameObject.SetActive(false);
        Log?.Add(button: (int)ButtonPress.LDL);
        MeasurementLocked?.Invoke(SliderMeasurement.LDL, _value);
    }

    public void MaxButtonClick()
    {
        _button.SetActive(false);
        _maxButton.SetActive(false);
        _messageText.gameObject.SetActive(false);

        var measurement = _phase == Phase.Threshold ? SliderMeasurement.Threshold : SliderMeasurement.LDL;

        _phase = Phase.Finished;
        _tooLoudImage.fillAmount = 0;
        if (_phase == Phase.Threshold)
        {
            _subthresholdImage.fillAmount = 1;
        }
        else 
        {
            _backgroundImage.color = 0.75f * Color.green;
        }
        _promptText.text = "";
        _thumbRectTransform.gameObject.SetActive(false);
        Log?.Add(button: (int)ButtonPress.Max);
        MeasurementLocked?.Invoke(
            measurement,
            float.PositiveInfinity);

    }
}
