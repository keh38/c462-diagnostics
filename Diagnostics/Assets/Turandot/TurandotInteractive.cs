using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurandotInteractive : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private GameObject _quitPanel;

    private bool _quitPanelShowing = false;

    void Start()
    {
        HTS_Server.SetCurrentScene("Turandot Interactive", this);
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A && !_quitPanelShowing)
        {
            _quitPanelShowing = true;
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
        _quitPanelShowing = false;
    }

    void IRemoteControllable.ProcessRPC(string command, string data)
    {

    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

}
