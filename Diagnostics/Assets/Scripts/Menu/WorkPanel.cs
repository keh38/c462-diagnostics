using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorkPanel : MonoBehaviour
{
    public ListBoxControl listBox;
    public MessageBox messageBox;

    internal class SceneDescription
    {
        public string name;
        public string scene;
        public string description;
    }

    private List<SceneDescription> _scenes;

    void Start()
    {
        messageBox.Hide();
        CreateSceneList();

        for (int k = 0; k < _scenes.Count; k++)
        {
            listBox.AddItem(k, _scenes[k].name);
        }
        listBox.OnChange += OnListBoxChange;
        listBox.SelectByIndex(0);
    }

    public void OnListBoxChange(GameObject go, int intSelected)
    {
        messageBox.Show(_scenes[intSelected].description);
    }

    public void OnOpenButtonClick()
    {
        Debug.Log($"Loading '{_scenes[listBox.SelectedIndex].scene}' scene ({_scenes[listBox.SelectedIndex].scene})");
        SceneManager.LoadScene(_scenes[listBox.SelectedIndex].scene);
    }

    private void CreateSceneList()
    {
        _scenes = new List<SceneDescription>();
        _scenes.Add(
            new SceneDescription()
            {
                name = "LabVIEW Audio",
                scene = "LV Audio",
                description = "Tests streaming to multiple audio devices using LabVIEW-based .NET assembly"
            }
            );
    }
}
