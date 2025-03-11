using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

using KLib;

public class PlayPanel : MonoBehaviour
{
    public ListBoxControl listBox;
    public MessageBox messageBox;

    public class DemoDescription
    {
        public string name;
        public string tag;
        public string scene;
        public string description;
    }

    private List<DemoDescription> _demos;

    void Start()
    {
        messageBox.Hide();
        CreateDemoList();

        for (int k=0; k<_demos.Count; k++)
        {
            listBox.AddItem(k, _demos[k].name, "", _demos[k].tag);
        }
        listBox.OnChange += OnListBoxChange;

        //var index = _demos.FindIndex(o => o.name.Equals(GameManager.LastDemo));
        //if (index < 0) index = 0;
        //listBox.SelectByIndex(index);
    }

    public void OnListBoxChange(GameObject go, int intSelected)
    {
        messageBox.Show(_demos[intSelected].description);
    }

    public void PlayButtonClick()
    {
        //GameManager.LastDemo = _demos[listBox.SelectedIndex].name;
        Debug.Log($"Loading '{_demos[listBox.SelectedIndex].name}' scene ({_demos[listBox.SelectedIndex].scene})");
        SceneManager.LoadScene(_demos[listBox.SelectedIndex].scene);
    }

    private void CreateDemoList()
    {
//        _demos = FileIO.XmlDeserializeFromTextAsset<List<DemoDescription>>("demos");
    }

}
