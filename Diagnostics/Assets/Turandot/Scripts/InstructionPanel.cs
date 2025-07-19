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

        _markdownRenderer.TextMesh.fontSize = instructions.FontSize;

        _pages = _instructions.Text
            .Replace("\r", "")
            .Split(new string[] { "[br]\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        
        ShowPage(_pageIndex);
        UpdateContinueButtonLabel();
    }

    private void ShowPage(int index)
    {
        _markdownRenderer.Source = _pages[index];
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
