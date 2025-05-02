using System;
using System.Collections;
using System.Linq;
using System.IO;
using System.Net;

using UnityEngine;
using UnityEngine.SceneManagement;

using KLib;
using KLib.Network;

public class HTS_Server : MonoBehaviour
{
    private bool _listenerReady = false;

    private KTcpListener _listener = null;
    private bool _stopServer;
    private NetworkDiscoveryServer _discoveryServer;
    private IPEndPoint _ipEndPoint;
    private string _address;

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
    public static IPAddress RemoteAddress { get { return instance._remoteEndPoint.Address; } }
    public static void SetCurrentScene(string name, IRemoteControllable controllableScene)
    {
        instance._currentSceneName = name;
        instance._currentScene = controllableScene;
        if (instance._remoteEndPoint != null)
        {
            var result = KTcpClient.SendMessage(instance._remoteEndPoint, $"ChangedScene:{name}");
        }
    }
    public static void SendMessage(string message, string data)
    {
        if (instance._remoteEndPoint != null)
        {
            var result = KTcpClient.SendMessage(instance._remoteEndPoint, $"{message}:{data}");
        }
    }

    private void _Init()
    {
        var go = new GameObject("Discovery Server").AddComponent<NetworkDiscoveryServer>();
        go.transform.parent = this.gameObject.transform;
        _discoveryServer = go.GetComponent<NetworkDiscoveryServer>();

        DontDestroyOnLoad(this);
    }

    #region private methods
    private void _StartServer()
    {
        _remoteConnected = false;
        _stopServer = false;

        _ipEndPoint = NetworkUtils.FindNextAvailableEndPoint();
        _address = _ipEndPoint.Address.ToString();

        _listener = new KTcpListener();
        _listener.StartListener(_ipEndPoint, bigEndian: false);

        StartCoroutine(TCPServer());
        _discoveryServer.StartReceiving("HEARING.TEST.SUITE", _ipEndPoint.Address.ToString(), _ipEndPoint.Port);

        Debug.Log("started HTS TCP server on " + _ipEndPoint.ToString());
    }

    public void StopServer()
    {
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

        if (!command.Equals("Ping"))
        {
            Debug.Log("Command received: " + command);
        }

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
                SceneManager.LoadScene("Home");
                break;

            case "Ping":
                _listener.SendAcknowledgement(_remoteConnected);
                break;

            case "ChangeScene":
                _listener.SendAcknowledgement();
                KLogger.Log.FlushLog();
                GameManager.DataForNextScene = "";
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
                _listener.SendAcknowledgement();
                GameManager.SetSubject(data);
                _currentScene.ProcessRPC("SubjectChanged");
                break;

            case "GetSubjectMetadata":
                _listener.WriteStringAsByteArray(GameManager.SerializeSubjectMetadata());
                break;

            case "SetSubjectMetadata":
                _listener.SendAcknowledgement();
                GameManager.DeserializeSubjectMetadata(data);
                _currentScene.ProcessRPC("SubjectMetadataChanged");
                break;

            case "SetSubjectMetrics":
                _listener.SendAcknowledgement();
                GameManager.DeserializeSubjectMetrics(data);
                break;

            case "GetTransducers":
                _listener.WriteStringAsByteArray(KLib.FileIO.XmlSerializeToString(GameManager.EnumerateTransducers()));
                break;

            case "GetAdapterMap":
                _listener.WriteStringAsByteArray(KLib.FileIO.XmlSerializeToString(HardwareInterface.AdapterMap));
                break;

            case "GetLog":
                KLogger.Log.FlushLog();
                _listener.WriteStringAsByteArray($"{Path.GetFileName(KLogger.LogPath)}:{File.ReadAllText(KLogger.LogPath)}");
                break;

            case "GetScreenSize":
                _listener.WriteStringAsByteArray($"{Screen.width},{Screen.height}");
                break;

            case "TransferFile":
                _listener.SendAcknowledgement();
                ReceiveFile(data);
                break;

            default:
                _listener.SendAcknowledgement(false);
                _currentScene.ProcessRPC(command, data);
                break;
        }
        _listener.CloseTcpClient();
    }

    private void ReceiveFile(string data)
    {
        var parts = data.Split(new char[] { ':' }, 3);
        for (int k = 0; k < parts.Length; k++) Debug.Log(parts[k]);
        if (parts.Length != 3) return;
        var folder = FileLocations.LocalResourceFolder(parts[0]);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        var filePath = Path.Combine(folder, parts[1]);
        File.WriteAllText(filePath, parts[2]);
    }

    private IPEndPoint ParseEndPoint(string address)
    {
        var parts = address.Split('/');
        var port = Int32.Parse(parts[1]);

        return new IPEndPoint(IPAddress.Parse(parts[0]), port);
    }

    #endregion
}
