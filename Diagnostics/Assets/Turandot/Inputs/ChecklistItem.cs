using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChecklistItem : MonoBehaviour
{
    [SerializeField] private Text _label;
    [SerializeField] private Toggle _toggle;

    private string _name;

    public delegate void ToggledDelegate(string name, bool isPressed);
    public ToggledDelegate Toggled;
    private void OnToggled(bool isPressed) { Toggled?.Invoke(_name, isPressed); }

    public void SetLabel(string label)
    {
        _name = label;
        _label.text = label;
    }

    public float GetWidth()
    {
        var rt = _label.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        return rt.anchoredPosition.x + rt.rect.width;
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
