using System;
using System.Collections;
using System.Linq;
using System.IO;
using System.Net;

using UnityEngine;
using UnityEngine.SceneManagement;

using KLib;
using KLibU.Net;
using System.Threading.Tasks;
using HTS.Unity.Tcp;

public class HTS_Server : MonoBehaviour
{
    private bool _listenerReady = false;

    private KTcpListener _tcpListener = null;
    private bool _stopServer;
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

    public static event EventHandler OnControllerConnected;
    public static event EventHandler OnControllerDisconnected;
    public static event EventHandler OnSubjectChanged;

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
            var changeScenePayload = new ChangeScenePayload() { SceneName = name };
            KTcpClient.SendRequest(instance._remoteEndPoint, TcpMessage.Request("ChangedScene", changeScenePayload));
        }
    }
    public static TcpMessage SendRequest(string command, object payload)
    {
        if (instance._remoteEndPoint == null)
        {
            return null;
        }
        return KTcpClient.SendRequest(instance._remoteEndPoint, TcpMessage.Request(command, payload));
    }

    public static void SendRequest(string command, string target, object data)
    {
        var payload = new RemoteMessagePayload
        {
            Target = target,
            Data = FileIO.JSONSerializeToString(data)
        };
        SendRequest(command, payload);
    }

    //public static void SendRequest(string command, string target, string data = "")
    //{
    //    var payload = new RemoteMessagePayload { Target = target, Data = data };
    //    SendRequest(command, payload);
    //}
    public static void SendRequest(string target, string command)
    {
        if (instance._remoteEndPoint == null)
        {
            return;
        }
        KTcpClient.SendRequest(instance._remoteEndPoint, TcpMessage.Request($"{target}:{command}"));
    }

    public static void SendBufferedFile(string localPath)
    {
        //if (instance._remoteEndPoint == null) return;

        //int bufferSize = 16384;

        //var fileInfo = new FileInfo(localPath);

        //long numBuffers = (long)Math.Ceiling((double)fileInfo.Length / bufferSize);

        //KTcpClient client = new KTcpClient();
        //client.ConnectTCPServer(instance._remoteEndPoint.Address.ToString(), _instance._remoteEndPoint.Port);

        //var result = client.WriteStringAsByteArray($"ReceiveBufferedFile:{Path.GetFileName(localPath)}:{bufferSize}:{numBuffers}");
        //if (result <= 0)
        //{
        //    return;
        //}

        //using (FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read))
        //using (BinaryReader reader = new BinaryReader(fs))
        //{
        //    for (int k = 0; k < numBuffers; k++)
        //    {
        //        var bytes = reader.ReadBytes(bufferSize);
        //        result = client.SendBuffer(bytes);
        //    }
        //}

        //client.CloseTCPServer();
    }

    private void _Init()
    {
        DontDestroyOnLoad(this);
    }

    #region private methods
    private void _StartServer()
    {
        _remoteConnected = false;
        _stopServer = false;

        _ipEndPoint = Discovery.FindNextAvailableEndPoint();
        _address = _ipEndPoint.Address.ToString();

        _tcpListener = new KTcpListener();
        _tcpListener.StartListener(_ipEndPoint);

        StartCoroutine(TCPServer());
        Debug.Log("started HTS TCP server on " + _ipEndPoint.ToString());
        
        var discoveryBeacon = gameObject.AddComponent<NetworkDiscoveryBeacon>();
        discoveryBeacon.StartBroadcasting(
            name: $"HEARING.TEST.SUITE",
            address: _ipEndPoint.Address.ToString(),
            port: _ipEndPoint.Port);
    }

    public void StopServer()
    {
        _stopServer = true;
        if (_tcpListener != null)
        {
            _tcpListener.CloseListener();
            _tcpListener = null;
        }

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
            if (_tcpListener.Pending())
            {
                try
                {
                    ProcessMessage();
                }
                catch (Exception ex)
                {
                    _tcpListener.WriteResponse(TcpMessage.Error(ex.Message));   
                    Debug.Log("error processing TCP message: " + ex.Message);
                }
            }
            yield return null;
        }
    }

    void ProcessMessage()
    {
        _tcpListener.AcceptTcpClient();

        var request = _tcpListener.ReadRequest();

        if (!request.Command.Equals("Ping"))
        {
            Debug.Log("Command received: " + request.Command);
        }

        switch (request.Command)
        {
            case "Connect":
                var connectionData = request.GetPayload<ConnectionRequestPayload>();
                var incomingEndPoint = new IPEndPoint(IPAddress.Parse(connectionData.Address), connectionData.Port);
                bool acceptConnection = !_remoteConnected || incomingEndPoint.Equals(_remoteEndPoint);

                var response = new ConnectionResponsePayload()
                {
                    HostName = "unknown",
                    SceneName = _currentSceneName,
                    VersionNumber = Application.version
                };

                _tcpListener.WriteResponse(TcpMessage.Ok(response));

                if (!acceptConnection)
                {
                    Debug.Log($"Refused incoming connection from {incomingEndPoint}");
                }
                else
                {
                    Debug.Log($"Connection accepted from {incomingEndPoint}");
                    _remoteConnected = true;
                    _remoteEndPoint = incomingEndPoint;
                    OnControllerConnected?.Invoke(this, EventArgs.Empty);
                }
                break;

            case "SetDataRoot":
                string dataRoot = request.GetPayload<string>();
                _tcpListener.WriteResponse(TcpMessage.Ok());
                //FileLocations.SetDataRoot(dataRoot);
                break;

            case "Disconnect":
                _tcpListener.WriteResponse(TcpMessage.Ok());
                _remoteConnected = false;
                OnControllerDisconnected?.Invoke(this, EventArgs.Empty);
                if (SceneManager.GetActiveScene().name != "Home")
                {
                    SceneManager.LoadScene("Home");
                }
                break;

            case "Ping":
                _tcpListener.WriteResponse(TcpMessage.Ok());
                break;

            case "ChangeScene":
                string sceneName = request.GetPayload<string>();
                _tcpListener.WriteResponse(TcpMessage.Ok());
                KLogger.Log.FlushLog();
                GameManager.DataForNextScene = "";
                Debug.Log($"changing scene to {sceneName}...");
                _currentScene.ChangeScene(sceneName);
                break;

            case "CreateProject":
                string projectName = request.GetPayload<string>();
                _tcpListener.WriteResponse(TcpMessage.Ok());
                FileLocations.CreateProjectFolder(projectName);
                break;

            case "GetCurrentSceneName":
                _tcpListener.WriteStringAsByteArray(_currentSceneName);
                break;

            case "GetVersionNumber":
                _tcpListener.WriteStringAsByteArray(Application.version);
                break;

            case "GetSubjectInfo":
                _tcpListener.WriteResponse(TcpMessage.Ok($"{GameManager.Project}/{GameManager.Subject}"));
                break;

            case "GetProjectList":
                _tcpListener.WriteResponse(TcpMessage.Ok(FileLocations.EnumerateProjects()));
                break;

            case "GetSubjectList":
                string projectToEnumerate = request.GetPayload<string>();
                _tcpListener.WriteResponse(TcpMessage.Ok(FileLocations.EnumerateSubjects(projectToEnumerate)));
                break;

            case "SetSubjectInfo":
                string projectAndSubject = request.GetPayload<string>();
                _tcpListener.WriteResponse(TcpMessage.Ok());
                GameManager.SetSubject(projectAndSubject);
                OnSubjectChanged?.Invoke(this, EventArgs.Empty);
                break;

            case "GetSubjectMetadata":
                _tcpListener.WriteResponse(TcpMessage.Ok(GameManager.GetSubjectMetadata()));
                break;

            case "SetSubjectMetadata":
                _tcpListener.WriteResponse(TcpMessage.Ok());
                var metaData = request.GetPayload<SubjectMetadata>();
                GameManager.SetSubjectMetadata(metaData);
                OnSubjectChanged?.Invoke(this, EventArgs.Empty);
                break;

            case "SetSubjectMetrics":
                _tcpListener.WriteResponse(TcpMessage.Ok());
                var metrics = request.GetPayload<SerializeableDictionary<string>>();
                GameManager.SetSubjectMetrics(metrics);
                OnSubjectChanged?.Invoke(this, EventArgs.Empty);
                break;

            case "GetTransducers":
                _tcpListener.WriteResponse(TcpMessage.Ok(GameManager.EnumerateTransducers()));
                break;

            //case "GetAdapterMap":
            //    _tcpListener.WriteStringAsByteArray(KLib.FileIO.XmlSerializeToString(HardwareInterface.AdapterMap));
            //    break;

            case "GetLog":
                KLogger.Log.FlushLog();
                var logFilePayload = new TextFilePayload() 
                { 
                    Filename = Path.GetFileName(KLogger.LogPath),
                    Content = File.ReadAllText(KLogger.LogPath)
                }; 
                _tcpListener.WriteResponse(TcpMessage.Ok(logFilePayload));
                break;

            case "GetSyncLog":
                var logPath = HardwareInterface.ClockSync.LogFile;
                Debug.Log($"sync log path = {logPath}");
                if (!string.IsNullOrEmpty(logPath))
                {
                    var syncLogText = File.ReadAllText(logPath);
                    if (string.IsNullOrEmpty(syncLogText))
                    {
                        _tcpListener.WriteStringAsByteArray("none");
                    }
                    else
                    {
                        _tcpListener.WriteStringAsByteArray($"{Path.GetFileName(logPath)}:{File.ReadAllText(logPath)}");
                    }
                }
                else
                {
                    _tcpListener.WriteResponse(TcpMessage.NotFound(logPath));
                }
                break;

            case "GetScreenSize":
                _tcpListener.WriteResponse(TcpMessage.Ok($"{Screen.width},{Screen.height}"));
                break;

            case "GetLEDColors":
                var cstring = "none";
                if (HardwareInterface.LED.IsInitialized)
                {
                    cstring = HardwareInterface.LED.GetColor();
                }
                _tcpListener.WriteResponse(TcpMessage.Ok($"{cstring}"));
                break;

            case "TransferFile":
                _tcpListener.WriteResponse(TcpMessage.Ok());
                //ReceiveFile(data);
                break;

            case "RunInstaller":
                _tcpListener.WriteResponse(TcpMessage.Ok());
                //RunInstaller(data);
                break;

            //case "FileExists":
            //    _tcpListener.WriteResponse(TcpMessage.Error());
            //    //var fullpath = Path.Combine(FileLocations.ProjectFolder, "Resources", data);
            //    //if (File.Exists(fullpath))
            //    //{
            //    //    var dt = File.GetLastAccessTime(fullpath);
            //    //    _tcpListener.WriteStringAsByteArray(FileIO.JSONSerializeToString(dt));
            //    //}
            //    //else
            //    //{
            //    //    _tcpListener.WriteStringAsByteArray("404");
            //    //}
            //    break;

            case "ReceiveBufferedFile":
                //ReceiveBufferedFile(data);
                break;
                
            default:
                if (_currentScene != null)
                {
                    _tcpListener.WriteResponse(_currentScene.ProcessRPC(request));
                }
                else
                {
                    _tcpListener.WriteResponse(TcpMessage.BadRequest());
                }

                break;
        }
        _tcpListener.CloseTcpClient();
    }

    private void ReceiveFile(string data)
    {
        var parts = data.Split(new char[] { ':' }, 3);
        if (parts.Length != 3) return;
        var folder = FileLocations.LocalResourceFolder(parts[0]);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        var filePath = Path.Combine(folder, parts[1]);
        File.WriteAllText(filePath, parts[2]);
    }

    private void ReceiveBufferedFile(string data)
    {
        //var parts = data.Split(new char[] { ':' }, 3);
        //if (parts.Length != 3)
        //{
        //    _tcpListener.SendAcknowledgement(false);
        //    return;
        //}
 
        //int bufferSize = int.Parse(parts[1]);
        //int numBuffers = int.Parse(parts[2]);

        //var filePath = Path.Combine(FileLocations.ProjectFolder, "Resources", parts[0]);
        //if (parts[0].StartsWith("Downloads"))
        //{
        //    filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), parts[0]);
        //}
        //var tempPath = Path.GetTempFileName();

        //_tcpListener.SendAcknowledgement();

        //using (FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
        //using (BinaryWriter bw = new BinaryWriter(fs))
        //{
        //    for (int k = 0; k < numBuffers; k++)
        //    {
        //        var bytes = _tcpListener.ReadByteArray();
        //        bw.Write(bytes);

        //        _tcpListener.SendAcknowledgement();
        //    }

        //    bw.Close();
        //    fs.Close();
        //}

        //if (File.Exists(filePath))
        //{
        //    File.Delete(filePath);
        //}
        //File.Move(tempPath, filePath); 
    }

    private void RunInstaller(string data)
    {
        string exePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", data);
        System.Diagnostics.Process.Start(exePath);

#if !UNITY_EDITOR
        if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif

    }

    private IPEndPoint ParseEndPoint(string address)
    {
        var parts = address.Split('/');
        var port = Int32.Parse(parts[1]);

        return new IPEndPoint(IPAddress.Parse(parts[0]), port);
    }

#endregion
}
