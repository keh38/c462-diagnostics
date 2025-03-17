using System;
using System.Collections;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using KLib.Network;

public class HTS_Server : MonoBehaviour
{
    private bool _listenerReady = false;

    private KTcpListener _listener = null;
    private bool _stopServer;
    private NetworkDiscoveryServer _discoveryServer;
    private string _address;
    private int _port = 4950;

    private IRemoteControllable _currentScene = null;
    private string _currentSceneName = "";
    private bool _remoteConnected = false;
    private IPEndPoint _remoteEndPoint = null;

    // Make singleton
    private static HTS_Server _instance;
    private static HTS_Server instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gobj = GameObject.Find("HTS_Server");
                if (gobj != null)
                {
                    _instance = gobj.GetComponent<HTS_Server>();
                }
                else
                {
                    _instance = new GameObject("HTS_Server").AddComponent<HTS_Server>();
                }
                _instance._Init();
            }
            return _instance;
        }
    }

    public static void StartServer() => instance._StartServer();
    public static bool RemoteConnected { get { return instance._remoteConnected; } }
    public static string MyAddress { get { return instance._address.Equals("localhost") ? "127.0.0.1" : instance._address; } }
    public static void SetCurrentScene(string name, IRemoteControllable controllableScene)
    {
        instance._currentSceneName = name;
        instance._currentScene = controllableScene;
        if (instance._remoteEndPoint != null)
        {
            var result = KTcpClient.SendMessage(instance._remoteEndPoint, $"ChangedScene:{name}");
        }
    }

    private void _Init()
    {
        var go = new GameObject("Discovery Server").AddComponent<NetworkDiscoveryServer>();
        go.transform.parent = this.gameObject.transform;
        _discoveryServer = go.GetComponent<NetworkDiscoveryServer>();

        DontDestroyOnLoad(this);
    }

    public bool Use { get; private set; }
    public string Host { get; private set; }

    #region private methods
    private void _StartServer()
    {
        _remoteConnected = false;
        _stopServer = false;

        Use = true;

        _address = NetworkUtils.FindServerAddress();

        _listener = new KTcpListener();
        _listener.StartListener(_address, _port, bigEndian: false);

        StartCoroutine(TCPServer());
        _discoveryServer.StartReceiving("HEARING.TEST.SUITE", _address, _port);
        Host = _address + ":" + _port;

        Debug.Log("started HTS TCP server on " + Host);
    }

    public void StopServer()
    {
        Use = false;
        _stopServer = true;
        if (_listener != null)
        {
            _listener.CloseListener();
            _listener = null;
        }

        _discoveryServer.StopReceiving();
        Debug.Log("stopped HTS TCP listener");
    }

    void OnDestroy()
    {
        StopServer();
    }

    IEnumerator TCPServer()
    {
        while (!_stopServer)
        {
            if (_listener.Pending())
            {
                try
                {
                    ProcessMessage();
                }
                catch (Exception ex)
                {
                    Debug.Log("error processing TCP message: " + ex.Message);
                }
            }
            yield return null;
        }
    }

    void ProcessMessage()
    {
        _listener.AcceptTcpClient();

        string input = _listener.ReadString();
        var parts = input.Split(new char[] { ':' }, 2);
        string command = parts[0];
        string data = null;
        if (parts.Length > 1)
        {
            data = parts[1];
        }

        Debug.Log("Command received: " + command);

        switch (command)
        {
            case "Connect":
                _listener.SendAcknowledgement();
                _remoteConnected = true;
                _remoteEndPoint = ParseEndPoint(data);
                _currentScene.ProcessRPC("Connect");
                break;

            case "Disconnect":
                _listener.SendAcknowledgement();
                _remoteConnected = false;
                _currentScene.ProcessRPC("Disconnect");
                //SceneManager.LoadScene("Home");
                break;

            case "Ping":
                _listener.SendAcknowledgement();
                break;


            case "ChangeScene":
                _currentScene.ChangeScene(data);
                break;

            case "GetCurrentSceneName":
                _listener.WriteStringAsByteArray(_currentSceneName);
                break;

            case "GetSubjectInfo":
                _listener.WriteStringAsByteArray($"{GameManager.Project}/{GameManager.Subject}");
                break;

            case "GetProjectList":
                _listener.WriteStringAsByteArray(KLib.FileIO.JSONSerializeToString(FileLocations.EnumerateProjects()));
                break;

            case "GetSubjectList":
                _listener.WriteStringAsByteArray(KLib.FileIO.JSONSerializeToString(FileLocations.EnumerateSubjects(data)));
                break;

            case "SetSubjectInfo":
                GameManager.SetSubject(data);
                _currentScene.ProcessRPC("SubjectChanged");
                break;

            case "GetSubjectMetadata":
                _listener.WriteStringAsByteArray(GameManager.SerializeSubjectMetadata());
                break;

            case "SetSubjectMetadata":
                GameManager.DeserializeSubjectMetadata(data);
                _currentScene.ProcessRPC("SubjectMetadataChanged");
                break;

            case "GetTransducers":
                _listener.WriteStringAsByteArray(KLib.FileIO.XmlSerializeToString(GameManager.EnumerateTransducers()));
                break;

            case "GetAdapterMap":
                _listener.WriteStringAsByteArray(KLib.FileIO.JSONSerializeToString(HardwareInterface.AdapterMap));
                break;

            default:
                _listener.SendAcknowledgement(false);
                _currentScene.ProcessRPC(command, data);
                break;
        }

        _listener.CloseTcpClient();
    }

    private IPEndPoint ParseEndPoint(string address)
    {
        var parts = address.Split('/');
        var port = Int32.Parse(parts[1]);

        return new IPEndPoint(IPAddress.Parse(parts[0]), port);
    }

    #endregion
}
