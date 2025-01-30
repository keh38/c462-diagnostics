using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SubjectPanel : MonoBehaviour
{
    public TMPro.TMP_InputField subjectInputField;
    public Button applyButton;

    public UnityEvent<string> SubjectChangedEvent { get; } = new UnityEvent<string>();

    public void ShowPanel()
    {
        subjectInputField.text = GameManager.Subject;
        applyButton.interactable = false;
    }

    public void SubjectInputFieldEndEdit(string value)
    {
        applyButton.interactable = true;
    }

    public void ApplyButtonClick()
    {
        applyButton.interactable = false;
        GameManager.SetSubject("Scratch", subjectInputField.text);

        SubjectChangedEvent.Invoke(GameManager.Subject);
    }
}
