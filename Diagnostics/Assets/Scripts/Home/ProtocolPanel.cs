using System.Collections;
using System.Collections.Generic;
using System.IO;
using KLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ProtocolPanel : MonoBehaviour
{
    [SerializeField] private ListBoxControl _listBox;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private TMPro.TMP_Text _timeStamp;

    private void Start()
    {
        _listBox.OnChange += _listBox_OnChange;

        _startButton.interactable = false;
        _resumeButton.gameObject.SetActive(false);
        _timeStamp.text = "";
    }

    public void ShowPanel()
    {
        FillListBox();
    }

    public void OnPlayButtonClick()
    {
        ProtocolManager.StartProtocol(false);
    }

    public void OnResumeButtonClick()
    {
        ProtocolManager.StartProtocol(true);
    }

    private void FillListBox()
    {
        var files = Directory.GetFiles(FileLocations.LocalResourceFolder("Protocols"), $"*.xml");
        foreach (var i in _listBox.Items)
        {
            i.Destroy();
        }
        _listBox.Items.Clear();

        for (int k=0; k < files.Length; k++)
        {
            var item = Path.GetFileNameWithoutExtension(files[k]);
            _listBox.AddItem(k, item);
        }

        var lastItem = AppState.GetLastUsedItem("Protocols");
        if (_listBox.Items.Find(x => x.name.Equals(lastItem)) != null)
        {
            _listBox.SelectByText(lastItem);
        }
    }

    private void _listBox_OnChange(GameObject go, int intSelected)
    {
        bool canResume = ProtocolManager.InitializeProtocol(_listBox.Items[intSelected].name);

        _startButton.interactable = true;
        _resumeButton.gameObject.SetActive(canResume);

        _timeStamp.text = canResume ? $"{ProtocolManager.LastestTime}" : "";
    }


}
