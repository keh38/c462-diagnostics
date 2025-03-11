using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBarControl : MonoBehaviour
{
    public Image image;

    private MenuBarStyle _style;

    public void SetStyle(MenuBarStyle newStyle, Color mainColor)
    {
        _style = newStyle;
        ApplyStyle(mainColor);
    }

    public void ApplyStyle(Color mainColor)
    {
        if (image != null)
        {
            image.color = _style.color;
        }

        var buttons = gameObject.GetComponentsInChildren<MenuButtonControl>();
        foreach (var b in buttons)
        {
            b.SetStyle(_style, mainColor);
        }
    }
}
