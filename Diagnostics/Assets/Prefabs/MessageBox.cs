using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LogicUI.FancyTextRendering;

public class MessageBox : MonoBehaviour
{
    public enum IconShape { None, Info, Error, Warning, Question}

    public TMPro.TMP_Text textLabel;
    public Image infoIcon;
    public Image errorIcon;
    public MarkdownRenderer markdownRenderer;

    private Image _currentIcon;

    private void Awake()
    {
        _currentIcon = infoIcon;
    }

    public void Show(string message, IconShape iconShape = IconShape.Info)
    {
        gameObject.SetActive(true);

        textLabel.text = message;
        textLabel.enabled = true;
        markdownRenderer.Source = "";
        markdownRenderer.enabled = false;

        var icon = GetIcon(iconShape);
        if (_currentIcon != null && _currentIcon != icon)
        {
            _currentIcon.enabled = false;
        }

        _currentIcon = icon;
        _currentIcon.enabled = true;
    }

    public void ShowMarkdown(string message, IconShape iconShape = IconShape.Info)
    {
        gameObject.SetActive(true);

        markdownRenderer.Source = message;
        textLabel.enabled = false;
        markdownRenderer.enabled = true;

        var icon = GetIcon(iconShape);
        if (_currentIcon != null && _currentIcon != icon)
        {
            _currentIcon.enabled = false;
        }

        _currentIcon = icon;
        _currentIcon.enabled = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private Image GetIcon(IconShape iconShape)
    {
        switch (iconShape)
        {
            case IconShape.Info:
                return infoIcon;
            case IconShape.Error:
                return errorIcon;
        }

        return null;
    }

}
