using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StyleControl : MonoBehaviour
{
    [SerializeField] public StyleDefinition style;

    public void ApplyStyle()
    {
        var cam = GameObject.FindObjectOfType<Camera>();
        cam.backgroundColor = style.mainColor;

        var title = GameObject.FindObjectOfType<TitleBarControl>();
        if (title != null)
        {
            title.SetStyle(style.title);
        }

        var menu = GameObject.FindObjectOfType<MenuBarControl>();
        if (menu != null)
        {
            menu.SetStyle(style.menu, style.mainColor);
        }

        var buttons = GameObject.FindObjectsOfType<ButtonControl>(true);
        foreach (var b in buttons)
        {
            b.SetStyle(style.button);
        }
    }
}
