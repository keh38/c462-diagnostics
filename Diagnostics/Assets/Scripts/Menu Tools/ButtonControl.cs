using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ButtonControl : MonoBehaviour
{
    public Image button;
    public TMPro.TMP_Text label;
    public Image icon;

#if UNITY_EDITOR
    [MenuItem("Tools/Add ButtonControl")]
    static void AddComponent()
    {
        foreach (var go in Selection.gameObjects)
        {
            var bc = go.AddComponent<ButtonControl>();
            bc.button = go.GetComponent<Image>();
            bc.label = go.GetComponentInChildren<TMPro.TMP_Text>();
            bc.icon = go.transform.Find("Image")?.GetComponentInChildren<Image>();
        }
    }
#endif

    public void SetStyle(ButtonStyle style)
    {
        if (button != null)
        {
            button.color = style.color;
        }
        if (icon != null)
        {
            icon.color = style.foreColor;
        }
        if (label != null)
        {
            label.color = style.foreColor;
            label.fontSize = style.fontSize;
        }
    }

}
