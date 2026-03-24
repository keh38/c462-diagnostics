using System.IO;
using KLib;
using KLibU.Net;
using Turandot.Schedules;
using Turandot;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _versionLabel;
    [SerializeField] private TMPro.TMP_Text _message;
    [SerializeField] private GameObject _quitPanel;

    private bool _waitingForResponse = false;

    void Start()
    {
        HTS_Server.SetCurrentScene("Lobby", null);

        _versionLabel.text = "V" + Application.version;
        HardwareInterface.Yield();
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A && !_waitingForResponse)
        {
            _waitingForResponse = true;
            _message.gameObject.SetActive(false);
            _quitPanel.SetActive(true);
        }
    }

    public void OnQuitConfirmButtonClick()
    {
        SceneManager.LoadScene("Home");
    }

    public void OnQuitCancelButtonClick()
    {
        _quitPanel.SetActive(false);
        _message.gameObject.SetActive(true);
        _waitingForResponse = false;
    }

}
