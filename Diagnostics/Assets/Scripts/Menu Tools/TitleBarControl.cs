using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleBarControl : MonoBehaviour
{
    public bool useDefault;
    public TitleBarStyle style;

    public Image image;
    public TMPro.TMP_Text label;

    private TitleBarStyle _defaultStyle;

    public void SetStyle(TitleBarStyle newStyle)
    {
        if (useDefault)
        {
            style = newStyle;
            ApplyStyle();
        }
    }

    public void ApplyStyle()
    {
        if (image != null)
        {
            image.color = style.color;
        }
        if (label != null)
        {
            label.color = style.fontColor;
            label.fontSize = style.fontSize;
        }
    }
}
