using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Turandot.Inputs;

public class ParameterSlider : MonoBehaviour
{
    private ParameterSliderProperties _properties = new ParameterSliderProperties();

    [SerializeField] private TMPro.TMP_Text _label;
    [SerializeField] private Slider _slider;
    [SerializeField] private TMPro.TMP_InputField _inputField;

    public Action<float> Setter = null;

    public string FullParameterName { get; private set; }
    public float Value { get; private set; } = 0;

    public bool Interactable
    {
        get { return _slider.interactable; }
        set { _slider.interactable = value; }
    }

    public delegate void ValueChangeDelegate(float value);
    public event ValueChangeDelegate ValueChange;
    private void OnValueChange(float value)
    {
        ValueChange?.Invoke(value);
    }

    public void Initialize(ParameterSliderProperties properties)
    {
        _properties = properties;
        Value = _properties.StartValue;
        FullParameterName = _properties.FullParameterName;

        _label.text = properties.Label;
        _slider.SetValueWithoutNotify(ParameterValueToSliderValue(Value));
        _inputField.text = Value.ToString(_properties.DisplayFormat);
    }

    public void SetValue(float value)
    {
        Value = value;
        _slider.value = ParameterValueToSliderValue(value);
        _inputField.text = value.ToString(_properties.DisplayFormat);
    }

    public void OnSliderValueChanged(float sliderValue)
    {
        Value = SliderValueToParameterValue(sliderValue);
        _inputField.text = Value.ToString(_properties.DisplayFormat);

        Setter?.Invoke(Value);
        OnValueChange(Value);
    }

    public void OnInputFieldEndEdit(string expr)
    {
        float newValue;
        if (float.TryParse(expr, out newValue))
        {
            newValue = Mathf.Min(newValue, _properties.MaxValue);
            Value = Math.Max(newValue, _properties.MinValue);

            _slider.SetValueWithoutNotify(ParameterValueToSliderValue(Value));
            _inputField.text = Value.ToString(_properties.DisplayFormat);

            Setter?.Invoke(Value);
            OnValueChange(Value);
        }
    }

    private float SliderValueToParameterValue(float sliderVal)
    {
        float paramVal = float.NaN;
        if (_properties.Scale == ParameterSliderProperties.SliderScale.Linear)
        {
            paramVal = _properties.MinValue + sliderVal * (_properties.MaxValue - _properties.MinValue);
        }
        else if (_properties.Scale == ParameterSliderProperties.SliderScale.Log)
        {
            paramVal = _properties.MinValue * Mathf.Exp(sliderVal * Mathf.Log(_properties.MaxValue / _properties.MinValue));
        }

        return paramVal;
    }

    private float ParameterValueToSliderValue(float paramVal)
    {
        float sliderVal = float.NaN;
        if (_properties.Scale == ParameterSliderProperties.SliderScale.Linear)
        {
            sliderVal = (paramVal - _properties.MinValue) / (_properties.MaxValue - _properties.MinValue);
        }
        else if (_properties.Scale == ParameterSliderProperties.SliderScale.Log)
        {
            sliderVal = Mathf.Log(paramVal / _properties.MinValue) / Mathf.Log(_properties.MaxValue / _properties.MinValue);
        }

        return sliderVal;
    }

}
