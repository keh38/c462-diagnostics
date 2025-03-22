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

    private float _yoffset = -120;

    public Action<bool> Setter = null;

    public void SetTitle(string title)
    {
        _title.text = title;
    }

    public void OnToggleClick(bool isPressed)
    {
        Setter?.Invoke(!isPressed);
    }

    public void AddSlider(GameObject gobj)
    {
        _yoffset -= _spacing;
        var rt = gobj.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(_spacing, _yoffset);
        rt.sizeDelta += new Vector2(-(_leftOffset + _rightOffset), 0);
        _yoffset -= rt.rect.height;

        var myRT = GetComponent<RectTransform>();
        myRT.sizeDelta += new Vector2(0, rt.rect.height + _spacing);
    }

}
