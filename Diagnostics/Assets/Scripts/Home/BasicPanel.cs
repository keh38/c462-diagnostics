using System.Collections;
using System.Collections.Generic;
using System.IO;
using BasicMeasurements;
using KLib;
using KLibU;
using UnityEngine;
using UnityEngine.SceneManagement;

using C462.Shared;

public class BasicPanel : MonoBehaviour
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
        var lastItem = AppState.GetLastUsedItem("BasicPanel");
        if (string.IsNullOrEmpty(lastItem) || _dropDown.Items.Find(x => x.name.Equals(lastItem)) == null)
        {
            lastItem = "Audiogram";
        }

        _dropDown.SelectByText(lastItem);
    }

    public void OnPlayButtonClick()
    {
        var fileType = _dropDown.Items[_dropDown.SelectedIndex].name;

        GameManager.DataForNextScene = _listBox.SelectedText;

        AppState.SetLastUsedItem("BasicPanel", fileType);
        AppState.SetLastUsedItem(GetPrefix(fileType), _listBox.SelectedText);

        if (fileType == "Audiogram")
        {
            SceneManager.LoadScene("Audiogram");
            return;
        }
        if (fileType == "Combined")
        {
            SceneManager.LoadScene("Combined Audio-LDL");
            return;
        }
        if (fileType == "Digits")
        {
            SceneManager.LoadScene("Digits");
            return;
        }
        if (fileType == "LDL")
        {
            var configPath = Path.Combine(SharedFileLocations.GetConfigFile("LDL", _listBox.SelectedText));
            var config = Files.XmlDeserialize<BasicMeasurementConfiguration>(configPath) as LDL.LDLMeasurementSettings;
            if (config.HapticStimulus != null && config.HapticStimulus.Source != HapticSource.NONE)
            {
                SceneManager.LoadScene("LDL_Haptics");
                return;
            }
            SceneManager.LoadScene("LDL");
            return;
        }

        if (fileType == "Tapping")
        {
            SceneManager.LoadScene("Tapping");
            return;
        }

        if (fileType == "Questionnaire")
        {
            SceneManager.LoadScene("Questionnaire");
            return;
        }
    }

    private void FillListBox(string fileType)
    {
        var prefix = GetPrefix(fileType);

        var files = Directory.GetFiles(SharedFileLocations.ResourceFolder("Config Files"), $"{prefix}.*.xml");
        foreach (var i in _listBox.Items)
        {
            i.Destroy();
        }
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
