using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParameterSlider : MonoBehaviour
{
    public enum Scale { Linear, Log}

    public Slider slider;
    public TMPro.TMP_InputField inputField;

    public Scale scale = Scale.Linear;
    public float minVal = 0;
    public float maxVal = 1;

    public string format = "F";

    public Action<float> Setter = null;

    public float Value { get; private set; } = 0;
    public bool Interactable
    {
        get { return slider.interactable; }
        set { slider.interactable = value; }
    }

    public delegate void OnValueChangeDelegate(float value);
    public event OnValueChangeDelegate OnValueChange;

    public void Initialize(Scale scale, float minVal, float maxVal, float value)
    {
        this.scale = scale;
        this.minVal = minVal;
        this.maxVal = maxVal;
        Value = value;

        slider.SetValueWithoutNotify(ParameterValueToSliderValue(value));
        inputField.text = value.ToString(format);
    }

    public void SetValue(float value)
    {
        Value = value;
        slider.value = ParameterValueToSliderValue(value);
        inputField.text = value.ToString(format);
    }

    public void OnSliderValueChanged(float sliderValue)
    {
        Value = SliderValueToParameterValue(sliderValue);
        inputField.text = Value.ToString(format);

        Setter?.Invoke(Value);
        OnValueChange?.Invoke(Value);
    }

    public void OnInputFieldEndEdit(string expr)
    {
        float newValue;
        if (float.TryParse(expr, out newValue))
        {
            newValue = Mathf.Min(newValue, maxVal);
            Value = Math.Max(newValue, minVal);

            slider.SetValueWithoutNotify(ParameterValueToSliderValue(Value));
            inputField.text = Value.ToString(format);

            Setter?.Invoke(Value);
            OnValueChange?.Invoke(Value);
        }
    }

    private float SliderValueToParameterValue(float sliderVal)
    {
        float paramVal = float.NaN;
        if (scale == Scale.Linear)
        {
            paramVal = minVal + sliderVal * (maxVal - minVal);
        }
        else if (scale == Scale.Log)
        {
            paramVal = minVal * Mathf.Exp(sliderVal * Mathf.Log(maxVal / minVal));
        }

        return paramVal;
    }

    private float ParameterValueToSliderValue(float paramVal)
    {
        float sliderVal = float.NaN;
        if (scale == Scale.Linear)
        {
            sliderVal = (paramVal - minVal) / (maxVal - minVal);
        }
        else if (scale == Scale.Log)
        {
            sliderVal = Mathf.Log(paramVal / minVal) / Mathf.Log(maxVal / minVal);
        }

        return sliderVal;
    }

}
