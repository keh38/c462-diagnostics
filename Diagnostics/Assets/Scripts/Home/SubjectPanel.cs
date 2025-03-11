using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SubjectPanel : MonoBehaviour
{
    [SerializeField] private DropDownListControl projectDropDown;
    [SerializeField] private DropDownListControl subjectDropDown;
    [SerializeField] private TMPro.TMP_InputField subjectInputField;
    [SerializeField] private Button applyButton;

    public UnityEvent<string> SubjectChangedEvent { get; } = new UnityEvent<string>();

    private string _selectedProject = "";
    private string _selectedSubject = "";

    private void Start()
    {
        projectDropDown.OnSelectionChange += OnProjectDropDownChange;
        subjectDropDown.OnSelectionChange += OnSubjectDropDownChange;
    }

    public void ShowPanel()
    {
        _selectedProject = GameManager.Project;
        _selectedSubject = GameManager.Subject;

        subjectDropDown.Clear();
        FillProjectDropDown();
    }

    public void OnProjectDropDownChange(GameObject go, int intSelected)
    {
        _selectedProject = projectDropDown.Items[intSelected].name;
        FillSubjectDropDown();
   }

    public void OnSubjectDropDownChange(GameObject go, int intSelected)
    {
        var subject = subjectDropDown.Items[intSelected].name;
        GameManager.SetSubject(_selectedProject, subject);
        SubjectChangedEvent.Invoke(GameManager.Subject);
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

    private void FillProjectDropDown()
    {
        var projects = FileLocations.EnumerateProjects();

        projectDropDown.Clear();
        for (int k = 0; k < projects.Count; k++)
        {
            projectDropDown.AddItem(k, projects[k]);
        }

        projectDropDown.SelectByText(_selectedProject);
    }

    private void FillSubjectDropDown()
    {
        var subjects = FileLocations.EnumerateSubjects(_selectedProject);

        subjectDropDown.Clear();
        for (int k = 0; k < subjects.Count; k++)
        {
            subjectDropDown.AddItem(k, subjects[k]);
        }
        subjectDropDown.AddItem(2, "Create new...");
        var x = subjectDropDown.DdlListBox.Items[2];
        x.ItemNormalColor = Color.red;
        x.Text = "argh";
        subjectDropDown.SelectByText(_selectedSubject);
    }

}
