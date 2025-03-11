using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using KLib;
using KLib.MSGraph;

public class AdminToolsMenu : MonoBehaviour
{
    public SyncPanel syncPanel;
    public UpdatePanel updatePanel;
    public OneDrivePanel oneDrivePanel;

    public Button syncMenuButton;
    public Button updateMenuButton;
    public Button oneDriveMenuButton;

    [SerializeField] private Image _cloudIcon;

    private GameObject _activePanel = null;
    private Button _activeButton = null;

    private TMPro.TMP_Text _subjectLabel;

    void Start()
    {
        _cloudIcon.color = OneDrivePanel.GetStatusColor(MSGraphClient.GetConnectionStatus(out string details));
    }

    //void EnableMenu(bool enabled)
    //{
    //    playMenuButton.interactable = enabled;
    //    subjectMenuButton.interactable = enabled;
    //    syncMenuButton.interactable = enabled;
    //    updateMenuButton.interactable = enabled;
    //    configMenuButton.interactable = enabled;
    //    oneDriveMenuButton.interactable = enabled;
    //    workMenuButton.interactable = enabled;
    //}

    public void OnSyncMenuButtonClick()
    {
        SelectItem(syncMenuButton, syncPanel.gameObject);
    }

    public void OnUpdateMenuButtonClick()
    {
        SelectItem(updateMenuButton, updatePanel.gameObject);
        updatePanel.CheckForUpdates();
    }

    public void OnOneDriveClick()
    {
        if (_activePanel != oneDrivePanel.gameObject)
        {
            SelectItem(oneDriveMenuButton, oneDrivePanel.gameObject);
        }
//        else
        {
            StartCoroutine(oneDrivePanel.ConnectToOneDrive());
        }
    }

    public void OnBackButtonClick()
    {
        SceneManager.LoadScene("Home");
    }

    private void SelectItem(Button button, GameObject panel)
    {
        if (_activePanel != null)
        {
            _activePanel.SetActive(false);
        }

        _activePanel = panel;
        _activeButton = button;

        _activePanel.SetActive(true);
    }
}
