using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LogicUI.FancyTextRendering;

public class InstructionPanel : MonoBehaviour
{
    [SerializeField] private MarkdownRenderer _markdownRenderer;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _continueButton;

    private int _pageIndex;
    private string[] _pages;
    private Turandot.Instructions _instructions;

    public delegate void InstructionsFinishedDelegate();
    public InstructionsFinishedDelegate InstructionsFinished;
    private void OnInstructionsFinished()
    {
        InstructionsFinished?.Invoke();
    }

    public void ShowInstructions(Turandot.Instructions instructions)
    {
        _instructions = instructions;
        _pageIndex = 0;

        _backButton.GetComponentInChildren<TMPro.TMP_Text>().fontSize = instructions.FontSize;
        _continueButton.GetComponentInChildren<TMPro.TMP_Text>().fontSize = instructions.FontSize;
        _markdownRenderer.TextMesh.fontSize = instructions.FontSize;

        _markdownRenderer.TextMesh.alignment = ConvertAlignmentEnum(instructions.HorizontalAlignment);
        ApplyVerticalAlignment(instructions.VerticalAlignment);

        _pages = _instructions.Text
            .Replace("\r", "")
            .Split(new string[] { "[br]\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        
        ShowPage(_pageIndex);
        UpdateContinueButtonLabel();
    }

    private TMPro.TextAlignmentOptions ConvertAlignmentEnum(Turandot.Instructions.HorizontalTextAlignment horizontalAlignment)
    {
        switch (horizontalAlignment)
        {
            case Turandot.Instructions.HorizontalTextAlignment.Left:
                return TMPro.TextAlignmentOptions.TopLeft;
            case Turandot.Instructions.HorizontalTextAlignment.Center:
                return TMPro.TextAlignmentOptions.Top;
            case Turandot.Instructions.HorizontalTextAlignment.Right:
                return TMPro.TextAlignmentOptions.TopRight;
        }

        return TMPro.TextAlignmentOptions.Left;
    }

    private void ApplyVerticalAlignment(Turandot.Instructions.VerticalTextAlignment verticalAlignment)
    {
        var rectTransform = _markdownRenderer.GetComponent<RectTransform>();

        float yval = 1;
        if (verticalAlignment == Turandot.Instructions.VerticalTextAlignment.Top) { yval = 1; }
        else if (verticalAlignment == Turandot.Instructions.VerticalTextAlignment.Middle) { yval = 0.5f; }
        else if (verticalAlignment == Turandot.Instructions.VerticalTextAlignment.Bottom) { yval = 0; }

        rectTransform.anchorMin = new Vector2(0, yval);
        rectTransform.anchorMax = new Vector2(1, yval); ;
        rectTransform.pivot = new Vector2(0.5f, yval);
    }

    private void ShowPage(int index)
    {
        string text = _pages[index];
        if (_instructions.LineSpacing > 1)
        {
            text = text.Replace("\n", "\n\n");
        }

        _markdownRenderer.Source = text;
        _backButton.interactable = index > 0;
    }

    public void OnBackButtonClick()
    {
        _pageIndex--;
        ShowPage(_pageIndex);
        UpdateContinueButtonLabel();
    }

    public void OnContinueButtonClick()
    {
        _pageIndex++;
        if (_pageIndex < _pages.Length)
        {
            ShowPage(_pageIndex);
            UpdateContinueButtonLabel();
        }
        else
        {
            OnInstructionsFinished();
        }
    }

    private void UpdateContinueButtonLabel()
    {
        _continueButton.GetComponentInChildren<TMPro.TMP_Text>().text = _pageIndex < _pages.Length - 1 ? "Next" : "Begin";
    }
}
