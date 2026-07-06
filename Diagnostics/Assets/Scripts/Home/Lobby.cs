using System.IO;
using KLib;
using KLibU.Net;
using Turandot.Schedules;
using Turandot;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _message;
    [SerializeField] private GameObject _quitPanel;

    private bool _waitingForResponse = false;

    void Start()
    {
        HTS_Server.SetCurrentScene("Lobby", null);

        // Not sure why this was ever a good idea. Should be handled by the GameBridge when the Game is launched, not here.
        //HardwareInterface.Yield();
    }

    private void OnDisable()
    {
        if (GameBridge.GameHasControl)
        {
            // We shouldn't be leaving the lobby until the Game has formally relinquished control.
            // But there is a loophole: if the Game is closed before it relinquishes control, 
            // then we will never resume hardware control, specifically the Digitimer, leading to downstream
            // errors. 

            Debug.Log("WARNING! Resuming hardware control, but it appears the Game is still in control");

            HardwareInterface.Resume();
        }
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
