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
    [SerializeField] private SyncPanel _syncPanel;
    [SerializeField] private UpdatePanel _updatePanel;
    [SerializeField] private OneDrivePanel _oneDrivePanel;
    [SerializeField] private GameObject _hardwarePanel;

    [SerializeField] private Button _syncMenuButton;
    [SerializeField] private Button _updateMenuButton;
    [SerializeField] private Button _oneDriveMenuButton;
    [SerializeField] private Button _hardwareMenuButton;

    [SerializeField] private Image _cloudIcon;

    private GameObject _activePanel = null;
    private Button _activeButton = null;

    private TMPro.TMP_Text _subjectLabel;

    void Start()
    {
        _cloudIcon.color = OneDrivePanel.GetStatusColor(MSGraphClient.GetConnectionStatus(out string details));
        if (!HardwareInterface.IsReady && !HardwareInterface.ErrorAcknowledged)
        {
            HardwareInterface.AcknowledgeError();
            _hardwareMenuButton.Select();
            OnHardwareMenuButtonClick();
        }
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
        SelectItem(_syncMenuButton, _syncPanel.gameObject);
    }

    public void OnUpdateMenuButtonClick()
    {
        SelectItem(_updateMenuButton, _updatePanel.gameObject);
        _updatePanel.CheckForUpdates();
    }

    public void OnOneDriveClick()
    {
        if (_activePanel != _oneDrivePanel.gameObject)
        {
            SelectItem(_oneDriveMenuButton, _oneDrivePanel.gameObject);
        }
//        else
        {
            StartCoroutine(_oneDrivePanel.ConnectToOneDrive());
        }
    }

    public void OnHardwareMenuButtonClick()
    {
        SelectItem(_hardwareMenuButton, _hardwarePanel.gameObject);
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
