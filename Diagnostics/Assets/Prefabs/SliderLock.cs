using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderLock : MonoBehaviour
{
    [SerializeField] private Image _onImage;
    [SerializeField] private Image _offImage;

    [SerializeField] private ParameterSlider _leftSlider;
    [SerializeField] private ParameterSlider _rightSlider;

    private bool _isLocked = true;

    private void Start()
    {
        _leftSlider.OnValueChange += OnLeftValueChanged;
        _rightSlider.OnValueChange += OnRightValueChanged;
    }

    public void OnStateToggle(bool pressed)
    {
        _isLocked = pressed;

        _onImage.enabled = _isLocked;
        _offImage.enabled = !_isLocked;

        if (_isLocked)
        {
            _rightSlider.SetValue(_leftSlider.Value);
        }
    }

    public void OnLeftValueChanged(float value)
    {
        if (_isLocked)
        {
            _rightSlider.SetValue(value);
        }
    }

    public void OnRightValueChanged(float value)
    {
        if (_isLocked)
        {
            _leftSlider.SetValue(value);
        }
    }
}
