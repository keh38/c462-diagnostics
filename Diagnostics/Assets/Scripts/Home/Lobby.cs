using System.IO;
using KLib;
using KLibU.Net;
using Turandot.Schedules;
using Turandot;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

using HTS.Unity.Tcp;
using System.Net;
using UnityEngine.Playables;

public class Lobby : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private TMPro.TMP_Text _versionLabel;
    [SerializeField] private GameObject _message;
    [SerializeField] private GameObject _quitPanel;

    private bool _waitingForResponse = false;

    NotificationDescriptor _notificationDescriptor;

    void Start()
    {
        HTS_Server.SetCurrentScene("Lobby", this);

        _versionLabel.text = "V" + Application.version;
        HardwareInterface.Yield();
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A && !_waitingForResponse)
        {
            _waitingForResponse = true;
            _message.SetActive(false);
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
        _message.SetActive(true);
        _waitingForResponse = false;
    }

    TcpMessage IRemoteControllable.ProcessRPC(TcpMessage request)
    {
        switch (request.Command)
        {
            case "RunMeasurements":
                var runMeasurementsPayload = request.GetPayload<RunMeasurementsPayload>();
                _notificationDescriptor = runMeasurementsPayload.Notification;
                StartCoroutine(StartProtocolNextFrame(runMeasurementsPayload.ListFile));
                return TcpMessage.Ok();
            default:
                return TcpMessage.NotFound(request.Command);
        }
    }

    IEnumerator StartProtocolNextFrame(string protocolFilename)
    {
        HardwareInterface.Resume();
        WindowManager.BringToFront();

        yield return null;
        //Protocol protocol = Protocol.LoadFromFile(protocolFilename);
        //GameManager.CurrentProtocol = protocol;
        //SceneManager.LoadScene("Measurement");

        yield return new WaitForSeconds(5);

        var gameEndPoint = new IPEndPoint(
            IPAddress.Parse(_notificationDescriptor.Address),
            _notificationDescriptor.Port);

        // Fire and forget — the Game's listener just needs the knock.
        KTcpClient.SendRequest(gameEndPoint, TcpMessage.Request(_notificationDescriptor.Command));  // "MeasurementsComplete"
    }

    public void ChangeScene(string newScene)
    {
    }
}
