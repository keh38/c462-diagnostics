using System;
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
    [SerializeField] private DropDownListControl transducerDropDown;
    [SerializeField] private TMPro.TMP_InputField subjectInputField;

    public UnityEvent<string> SubjectChangedEvent { get; } = new UnityEvent<string>();

    private string _selectedProject = "";
    private string _selectedSubject = "";

    private bool _ignoreEvents = false;

    private void Start()
    {
        projectDropDown.OnSelectionChange += OnProjectDropDownChange;
        subjectDropDown.OnSelectionChange += OnSubjectDropDownChange;
        transducerDropDown.OnSelectionChange += OnTransducerDropDownChange;
    }

    public void ShowPanel()
    {
        _selectedProject = GameManager.Project;
        _selectedSubject = GameManager.Subject;

        subjectInputField.gameObject.SetActive(false);

        subjectDropDown.Clear();

        FileLocations.CheckFolderStructure();
        FillProjectDropDown();
    }

    public void OnProjectDropDownChange(GameObject go, int intSelected)
    {
        _selectedProject = projectDropDown.Items[intSelected].name;
        FillSubjectDropDown();
    }

    public void OnSubjectDropDownChange(GameObject go, int intSelected)
    {
        subjectInputField.gameObject.SetActive(intSelected == 0);

        if (intSelected > 0)
        {
            var subject = subjectDropDown.Items[intSelected].name;
            GameManager.SetSubject(_selectedProject, subject);
            SubjectChangedEvent.Invoke(GameManager.Subject);

            _ignoreEvents = true;

            FillTransducerDropDown();
            transducerDropDown.SelectByText(GameManager.Transducer);
            HardwareInterface.AdapterMap.AudioTransducer = GameManager.Transducer;
            if (HardwareInterface.LED.IsInitialized)
            {
                HardwareInterface.LED.SetColorFromString(GameManager.ProjectSettings.DefaultLEDColor);
            }

            _ignoreEvents = false;
        }
        else
        {
            subjectInputField.Select();
        }
    }

    public void OnSubjectInputFieldEndEdit(string value)
    {
        CreateNewSubject(subjectInputField.text);
    }

    public void OnTransducerDropDownChange(GameObject go, int intSelected)
    {
        if (!_ignoreEvents)
        {
            GameManager.Transducer = transducerDropDown.Items[intSelected].name;
            HardwareInterface.AdapterMap.AudioTransducer = GameManager.Transducer;
        }
    }

    private void CreateNewSubject(string name)
    {
        _selectedSubject = name;
        GameManager.SetSubject(_selectedProject, name);

        FillSubjectDropDown();
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
        subjectDropDown.AddItem(0, "Create new subject...");
        for (int k = 0; k < subjects.Count; k++)
        {
            subjectDropDown.AddItem(k+1, subjects[k]);
        }

        var item = subjectDropDown.DdlListBox.Items[0];
        var text = item.transform.GetChild(1)?.GetComponent<Text>();
        if (text != null)
        {
            text.fontStyle = FontStyle.Italic;
        }

        subjectDropDown.SelectByText(_selectedSubject);
    }

    private void FillTransducerDropDown()
    {
        var transducers = GameManager.EnumerateTransducers();

        transducerDropDown.Clear();
        for (int k=0; k<transducers.Count; k++)
        {
            transducerDropDown.AddItem(k, transducers[k]);
        }
    }
}
