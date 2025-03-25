using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurandotPanel : MonoBehaviour
{
    [SerializeField] private DropDownListControl _dropDown; 
    [SerializeField] private ListBoxControl _listBox;

    private string _selectedType;

    private void Start()
    {
        _dropDown.OnSelectionChange += OnFileTypeDropDownChange;
    }

    public void ShowPanel()
    {
        var lastItem = AppState.GetLastUsedItem("TurandotPanel");
        if (string.IsNullOrEmpty(lastItem) || _dropDown.Items.Find(x => x.name.Equals(lastItem)) == null)
        {
            lastItem = "Turandot";
        }

        _dropDown.SelectByText(lastItem);
    }

    public void OnPlayButtonClick()
    {
        var fileType = _dropDown.Items[_dropDown.SelectedIndex].name;

        GameManager.DataForNextScene = _listBox.SelectedText;

        AppState.SetLastUsedItem("TurandotPanel", fileType);
        AppState.SetLastUsedItem(GetPrefix(fileType), _listBox.SelectedText);

        if (fileType == "Interactive")
        {
            SceneManager.LoadScene("Turandot Interactive");
        }
        else
        {
            SceneManager.LoadScene("Turandot");
        }
    }

    private void FillListBox(string fileType)
    {
        var prefix = GetPrefix(fileType);

        var files = Directory.GetFiles(FileLocations.LocalResourceFolder("Config Files"), $"{prefix}.*.xml");
        _listBox.Items.Clear();
        for (int k=0; k < files.Length; k++)
        {
            var item = Path.GetFileNameWithoutExtension(files[k]).Remove(0, prefix.Length + 1);
            _listBox.AddItem(k, item);
        }

        var lastItem = AppState.GetLastUsedItem(prefix);
        if (_listBox.Items.Find(x => x.name.Equals(lastItem)) != null)
        {
            _listBox.SelectByText(lastItem);
        }
    }

    private string GetPrefix(string fileType)
    {
        return (fileType.Equals("Script")) ? "TScript" : fileType;
    }

    public void OnFileTypeDropDownChange(GameObject go, int intSelected)
    {
        FillListBox(_dropDown.Items[intSelected].name);
    }
}
