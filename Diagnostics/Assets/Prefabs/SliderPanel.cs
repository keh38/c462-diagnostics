using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderPanel : MonoBehaviour
{
    [SerializeField] private Text _title;
    [SerializeField] private Toggle _muteButton;

    public Action<bool> Setter = null;

    public void SetTitle(string title)
    {
        _title.text = title;
    }

    public void OnToggleClick(bool isPressed)
    {
        Setter?.Invoke(!isPressed);
    }

}
