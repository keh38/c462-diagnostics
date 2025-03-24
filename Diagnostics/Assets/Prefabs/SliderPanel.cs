using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderPanel : MonoBehaviour
{
    [SerializeField] private Text _title;
    [SerializeField] private Toggle _muteButton;
    [SerializeField] private float _spacing = 50;
    [SerializeField] private float _leftOffset = 50;
    [SerializeField] private float _rightOffset = 50;

    private List<ParameterSlider> _sliders = new List<ParameterSlider>();
    public List<ParameterSlider> Sliders {get { return _sliders; } }
    public string ChannelName { get; private set; }

    private float _yoffset = -120;

    public Action<bool> Setter = null;
    public bool SelfChange { get; private set; }
    public int IsActive { get; private set; }

    public bool _ignoreEvents = false;

    public void SetTitle(string title)
    {
        ChannelName = title;
        _title.text = title;
        _sliders = new List<ParameterSlider>();
    }

    public void OnToggleClick(bool isPressed)
    {
        if (_ignoreEvents) return;

        Setter?.Invoke(!isPressed);
        IsActive = isPressed ? 0 : 1;
        SelfChange = true;
    }

    public void SetChannelActive(bool active)
    {
        _ignoreEvents = true;

        _muteButton.isOn = !active;
        IsActive = active ? 1 : 0;
        SelfChange = false;

        _ignoreEvents = false;
    }

    public void AddSlider(ParameterSlider slider)
    {
        _sliders.Add(slider);

        _yoffset -= _spacing;
        var rt = slider.gameObject.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(_spacing, _yoffset);
        rt.sizeDelta += new Vector2(-(_leftOffset + _rightOffset), 0);
        _yoffset -= rt.rect.height;

        var myRT = GetComponent<RectTransform>();
        myRT.sizeDelta += new Vector2(0, rt.rect.height + _spacing);
    }

}
