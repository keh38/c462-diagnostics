using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonControl : MonoBehaviour
{
    public bool useDefault = true;
    public MenuButtonStyle style;

    public Button button;
    public Image checkmark;
    public TMPro.TMP_Text label;
    public Image icon;

#if UNITY_EDITOR
    [MenuItem("Tools/Add MenuButtonControl")]
    static void AddComponent()
    {
        foreach (var go in Selection.gameObjects)
        {
            var mbc = go.AddComponent<MenuButtonControl>();
            mbc.button = go.GetComponent<Button>();
            mbc.label = go.GetComponentInChildren<TMPro.TMP_Text>();
            mbc.icon = go.transform.Find("Image")?.GetComponentInChildren<Image>();
        }
    }
#endif

    public void SetStyle(MenuBarStyle menuBarStyle, Color mainColor)
    {
        if (useDefault)
        {
            style.color = menuBarStyle.fontColor;
            style.fontSize = menuBarStyle.fontSize;
        }

        if (button != null)
        {
            var c = button.colors;
            c.pressedColor = mainColor;
            c.selectedColor = mainColor;
            button.colors = c;
        }

        if (checkmark != null)
        {
            checkmark.color = mainColor;
        }

        ApplyStyle();
    }

    public void ApplyStyle()
    {
        if (icon != null)
        {
            icon.color = style.color;
        }
        if (label != null)
        {
            label.color = style.color;
            label.fontSize = style.fontSize;
        }
    }
}
