using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChecklistItem : MonoBehaviour
{
    [SerializeField] private Text _label;
    [SerializeField] private Toggle _toggle;

    public delegate void ToggledDelegate(string name, bool isPressed);
    public ToggledDelegate Toggled;
    private void OnToggled(bool isPressed) { Toggled?.Invoke(Name, isPressed); }

    public bool Value
    {
        get { return _toggle.isOn; }
        set { _toggle.isOn = value; }
    }

    public string Name { get; private set; }

    public void SetLabel(string label, int fontSize)
    {
        Name = label;
        _label.fontSize = fontSize;
        _label.text = label;
    }

    public float GetWidth()
    {
        var rt = _label.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        return rt.anchoredPosition.x + rt.rect.width;
    }

    public float GetHeight()
    {
        var rt = _label.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        return rt.anchoredPosition.y + rt.rect.height;
    }

    public void SetGroup(ToggleGroup group)
    {
        _toggle.group = group;
    }

    public void OnToggleClick(bool isPressed)
    {
        OnToggled(isPressed);
    }

    public void Clear()
    {
        _toggle.isOn = false;
    }
}
