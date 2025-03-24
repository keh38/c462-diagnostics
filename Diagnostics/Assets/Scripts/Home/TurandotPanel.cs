using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurandotPanel : MonoBehaviour
{
    [SerializeField] private ListBoxControl _listBox;
    [SerializeField] private MessageBox _messageBox;

    void Start()
    {
        _messageBox.Hide();
        FillListBox("");
    }

    public void OnPlayButtonClick()
    {
        string configFile = $"Interactive.{_listBox.SelectedText}";
        GameManager.DataForNextScene = configFile;
        SceneManager.LoadScene("Turandot Interactive");
    }

    private void FillListBox(string fileType)
    {
        var files = Directory.GetFiles(FileLocations.LocalResourceFolder("Config Files"), "Interactive.*.xml");
        _listBox.Items.Clear();
        for (int k=0; k < files.Length; k++)
        {
            var item = Path.GetFileNameWithoutExtension(files[k]).Remove(0, ("Interactive.").Length);
            _listBox.AddItem(k, item);
        }
    }
}
