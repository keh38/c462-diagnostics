using System;
using System.Collections;
using System.Linq;
using System.IO;
using System.Net;

using UnityEngine;
using UnityEngine.SceneManagement;

using KLib;
using KLibU;
using KLibU.Net;
using System.Threading.Tasks;
using HTS.Unity.Tcp;
using C462.Shared;
using C462.Shared.Protocol;
using C462.Shared.Protocol.DTOs;
using System.Collections.Generic;

public class HTS_Server : MonoBehaviour
{
    private KTcpListener _tcpListener = null;
    private bool _stopServer;
    private IPEndPoint _ipEndPoint;
    private string _address;

    private IRemoteControllable _currentScene = null;
    private string _currentSceneName = "";
    private bool _remoteConnected = false;
    private IPEndPoint _remoteEndPoint = null;

    private KTcpListener _beaconListener = null;

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
    public static bool IsLocalConnection { get { return RemoteConnected && (IPAddress.IsLoopback(instance._remoteEndPoint.Address) || instance._remoteEndPoint.Address.ToString().Equals("127.0.0.1") || _instance._remoteEndPoint.Address.ToString().Equals(_instance._address)); } }
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
            Data = Files.JSONSerializeToString(data)
        };
        SendRequest(command, payload);
    }

    public static void SendDataFile(string sceneName, string path, FileDestination destination = FileDestination.SubjectData)
    {
        if (IsLocalConnection)
        {
            return;
        }

        SendRequest("ReceiveData", sceneName, new TextFilePayload
        {
            Destination = destination,
            Filename = Path.GetFileName(path),
            Content = File.ReadAllText(path)
        });
    }

    public static void SendRequest(string target, string command)
    {
        if (instance._remoteEndPoint == null)
        {
            return;
        }
        KTcpClient.SendRequest(instance._remoteEndPoint, TcpMessage.Request($"{target}:{command}"));
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

        _beaconListener = new KTcpListener();
        _beaconListener.StartListener(new IPEndPoint(IPAddress.Loopback, HTSProtocol.BeaconPort));
        StartCoroutine(BeaconServer());
        Debug.Log($"Beacon server started on port {HTSProtocol.BeaconPort}");

        var discoveryBeacon = gameObject.AddComponent<NetworkDiscoveryBeacon>();
        discoveryBeacon.StartBroadcasting(
            name: $"HEARING.TEST.SUITE",
            address: _ipEndPoint.Address.ToString(),
            port: _ipEndPoint.Port);
    }

    public void StopServer()
    {
        SendRequest("Disconnect", (object)null);

        _stopServer = true;
        if (_tcpListener != null)
        {
            _tcpListener.CloseListener();
            _tcpListener = null;
        }

        Debug.Log("stopped HTS TCP listener");

        if (_beaconListener != null)
        {
            _beaconListener.CloseListener();
            _beaconListener = null;
        }
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

    IEnumerator BeaconServer()
    {
        while (!_stopServer)
        {
            if (_beaconListener.Pending())
            {
                try
                {
                    _beaconListener.AcceptTcpClient();
                    _beaconListener.ReadRequest();  // content ignored
                    _beaconListener.WriteResponse(TcpMessage.Ok(new EndpointPayload
                    {
                        Address = _ipEndPoint.Address.ToString(),
                        Port = _ipEndPoint.Port
                    }));
                    _beaconListener.CloseTcpClient();
                }
                catch (Exception ex)
                {
                    Debug.Log($"Beacon server error: {ex.Message}");
                }
            }
            yield return null;
        }

        _beaconListener.CloseListener();
        _beaconListener = null;
        Debug.Log("Beacon server stopped");
    }

    void ProcessMessage()
    {
        string folder;

        _tcpListener.AcceptTcpClient();

        var request = _tcpListener.ReadRequest();

        switch (request.Command)
        {
            case "Connect":
                Debug.Log("Command received: " + request.Command);
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
                    Debug.Log("Bringing window to front for new connection...");
                    WindowManager.BringToFront();
                    OnControllerConnected?.Invoke(this, EventArgs.Empty);
                }
                break;

            case "SetDataRoot":
                Debug.Log("Command received: " + request.Command);
                _tcpListener.WriteResponse(TcpMessage.Ok());
                string dataRoot = request.GetPayload<string>();
                SharedFileLocations.SetProjectRootFolder(dataRoot);
                break;

            case "Disconnect":
                Debug.Log("Command received: " + request.Command);
                _tcpListener.WriteResponse(TcpMessage.Ok());
                _remoteConnected = false;
                OnControllerDisconnected?.Invoke(this, EventArgs.Empty);
                if (SceneManager.GetActiveScene().name != "Lobby")
                {
                    SceneManager.LoadScene("Lobby");
                }
                break;

            case "Ping":
                _tcpListener.WriteResponse(TcpMessage.Ok());
                break;

            case "ChangeScene":
                Debug.Log("Command received: " + request.Command);
                string sceneName = request.GetPayload<string>();
                _tcpListener.WriteResponse(TcpMessage.Ok());
                KLogger.Log.FlushLog();
                GameManager.DataForNextScene = "";
                Debug.Log($"changing scene to {sceneName}...");
                if (_currentScene != null)
                {
                    _currentScene.ChangeScene(sceneName);
                }
                else
                {
                    SceneManager.LoadScene(sceneName);
                }
                break;

            case "SwitchToGame":
                Debug.Log($"Command received: {request.Command}");

                // Give the active scene a chance to clean itself up before changing scenes. 
                var scResponse = _currentScene?.ProcessRPC(request);
                if (scResponse != null && scResponse.IsOk)
                {
                    _tcpListener.WriteResponse(scResponse);
                    break;
                }
                _tcpListener.WriteResponse(TcpMessage.Ok());
                Debug.Log($"changing scene to Lobby ...");
                GameBridge.RestoreControlToGame();
                KLogger.Log.FlushLog();
                SceneManager.LoadScene("Lobby");
                break;

            case "CreateProject":
                string projectName = request.GetPayload<string>();
                Debug.Log($"Command received: {request.Command} {projectName}");
                _tcpListener.WriteResponse(TcpMessage.Ok());
                SharedFileLocations.CreateHtsProjectFolder(projectName);
                break;

            case "GetCurrentSceneName":
                Debug.Log($"Command received: {request.Command}");
                _tcpListener.WriteResponse(TcpMessage.Ok(_currentSceneName));
                break;

            case "GetSubjectInfo":
                Debug.Log($"Command received: {request.Command}");
                _tcpListener.WriteResponse(TcpMessage.Ok($"{GameManager.Project}/{GameManager.Subject}"));
                break;

            case "GetProjectList":
                Debug.Log($"Command received: {request.Command}");
                _tcpListener.WriteResponse(TcpMessage.Ok(SharedFileLocations.EnumerateHtsProjects()));
                break;

            case "GetSubjectList":
                Debug.Log($"Command received: {request.Command}");
                string projectToEnumerate = request.GetPayload<string>();
                _tcpListener.WriteResponse(TcpMessage.Ok(SharedFileLocations.EnumerateHtsSubjects(projectToEnumerate)));
                break;

            case "SetSubjectInfo":
                string projectAndSubject = request.GetPayload<string>();
                Debug.Log($"Command received: {request.Command} {projectAndSubject}");
                _tcpListener.WriteResponse(TcpMessage.Ok());
                GameManager.SetSubject(projectAndSubject);
                OnSubjectChanged?.Invoke(this, EventArgs.Empty);
                break;

            case "GetSubjectMetadata":
                Debug.Log($"Command received: {request.Command}");
                _tcpListener.WriteResponse(TcpMessage.Ok(GameManager.GetSubjectMetadata()));
                break;

            case "SetSubjectMetadata":
                Debug.Log($"Command received: {request.Command}");
                _tcpListener.WriteResponse(TcpMessage.Ok());
                var metaData = request.GetPayload<SubjectMetadata>();
                GameManager.SetSubjectMetadata(metaData);
                OnSubjectChanged?.Invoke(this, EventArgs.Empty);
                break;

            case "SetSubjectMetrics":
                Debug.Log($"Command received: {request.Command}");
                _tcpListener.WriteResponse(TcpMessage.Ok());
                var metrics = request.GetPayload<SerializeableDictionary<string>>();
                GameManager.SetSubjectMetrics(metrics);
                OnSubjectChanged?.Invoke(this, EventArgs.Empty);
                break;

            case "GetTransducers":
                Debug.Log($"Command received: {request.Command}");
                _tcpListener.WriteResponse(TcpMessage.Ok(SharedFileLocations.EnumerateTransducers()));
                break;

            case "GetAdapterMap":
                Debug.Log($"Command received: {request.Command}");
                _tcpListener.WriteResponse(TcpMessage.Ok(HardwareInterface.AdapterMap));
                break;

            case "GetLog":
                Debug.Log($"Command received: {request.Command}");
                KLogger.Log.FlushLog();
                var logFilePayload = new TextFilePayload()
                {
                    Filename = Path.GetFileName(KLogger.LogPath),
                    Content = File.ReadAllText(KLogger.LogPath)
                };
                _tcpListener.WriteResponse(TcpMessage.Ok(logFilePayload));
                break;

            case "GetSyncLog":
                Debug.Log($"Command received: {request.Command}");
                var logPath = HardwareInterface.ClockSync.LogFile;
                //Debug.Log($"sync log path = {logPath}");
                if (!string.IsNullOrEmpty(logPath))
                {
                    var syncLogPayload = new TextFilePayload()
                    {
                        Filename = Path.GetFileName(logPath),
                        Content = File.ReadAllText(logPath)
                    };
                    _tcpListener.WriteResponse(TcpMessage.Ok(syncLogPayload));
                }
                else
                {
                    _tcpListener.WriteResponse(TcpMessage.NotFound(logPath));
                }
                break;

            case "GetScreenSize":
                Debug.Log($"Command received: {request.Command}");
                _tcpListener.WriteResponse(TcpMessage.Ok($"{Screen.width},{Screen.height}"));
                break;

            case "GetLEDColors":
                Debug.Log($"Command received: {request.Command}");
                var cstring = "none";
                if (HardwareInterface.LED.IsInitialized)
                {
                    cstring = HardwareInterface.LED.GetColor();
                }
                _tcpListener.WriteResponse(TcpMessage.Ok($"{cstring}"));
                break;

            case "ReceiveTextFile":
                _tcpListener.WriteResponse(TcpMessage.Ok());
                var filePayload = request.GetPayload<TextFilePayload>();
                ReceiveTextFile(filePayload);
                break;

            case "ReceiveAudiogram":
                _tcpListener.WriteResponse(TcpMessage.Ok());
                var audiogramPayload = request.GetPayload<TextFilePayload>();
                ReceiveAudiogram(audiogramPayload);
                break;

            case "RunInstaller":
                _tcpListener.WriteResponse(TcpMessage.Ok());
                var data = request.GetPayload<string>();
                Debug.Log($"Command received: {request.Command} {data}");
                RunInstaller(data);
                break;

            case "FileExists":
                var fileInfoPayload = request.GetPayload<FileInfoPayload>();
                folder = FileLocations.ResolveFolder(fileInfoPayload.Destination, fileInfoPayload.SubPath);
                var fullpath = Path.Combine(folder, fileInfoPayload.Filename);
                if (File.Exists(fullpath))
                {
                    fileInfoPayload.LastModified = File.GetLastAccessTime(fullpath);
                    _tcpListener.WriteResponse(TcpMessage.Ok(fileInfoPayload));
                }
                else
                {
                    _tcpListener.WriteResponse(TcpMessage.NotFound(fileInfoPayload.Filename));
                }
                break;

            case "ReceiveBufferedFile":
                var largeFilePayload = request.GetPayload<BufferedFilePayload>();
                _tcpListener.WriteResponse(TcpMessage.Ok()); // signal ready

                folder = FileLocations.ResolveFolder(largeFilePayload.Destination, largeFilePayload.SubPath);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var destPath = Path.Combine(folder, largeFilePayload.Filename);

                using (var fs = new FileStream(destPath, FileMode.Create, FileAccess.Write))
                using (var writer = new BinaryWriter(fs))
                {
                    for (long k = 0; k < largeFilePayload.NumBuffers; k++)
                    {
                        var bytes = _tcpListener.ReadRawBytes();
                        writer.Write(bytes);
                    }
                }

                _tcpListener.WriteResponse(TcpMessage.Ok()); // signal complete
                Debug.Log($"ReceiveBufferedFile complete: {largeFilePayload.Filename}");
                break;

            case "RunMeasurements":
                Debug.Log($"Command received: {request.Command}");
                var runMeasurementsPayload = request.GetPayload<RunMeasurementsPayload>();
                if (!string.IsNullOrEmpty(runMeasurementsPayload.ListFile))
                {
                    bool protocolStarted = ProtocolManager.StartProtocol(runMeasurementsPayload);
                    if (!protocolStarted)
                    {
                        _tcpListener.WriteResponse(TcpMessage.Error("Failed to start protocol"));
                        break;
                    }
                }
                _tcpListener.WriteResponse(TcpMessage.Ok());

                if (string.IsNullOrEmpty(runMeasurementsPayload.ListFile))
                {
                    GameBridge.RetakeControl(runMeasurementsPayload);
                    if (SceneManager.GetActiveScene().name != "Lobby")
                        SceneManager.LoadScene("Lobby");
                }

                break;

            case "Quit":
                Debug.Log($"Command received: {request.Command}");
                StartCoroutine(QuitNextFrame());
                _tcpListener.WriteResponse(TcpMessage.Ok());
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

    private IEnumerator QuitNextFrame()
    {
        Debug.Log("Quitting application by remote request...");
        yield return null;
        Application.Quit();
    }

    public static void SendBufferedFile(string localPath, string remoteFilename)
    {
        if (instance._remoteEndPoint == null) return;
        Task.Run(() => instance._SendBufferedFile(localPath, remoteFilename));
    }

    private void _SendBufferedFile(string localPath, string remoteFilename)
    {
        int bufferSize = 16384;
        var fileInfo = new FileInfo(localPath);
        long numBuffers = (long)Math.Ceiling((double)fileInfo.Length / bufferSize);

        var client = new KTcpClient();
        try
        {
            client.StartBufferedSend(_remoteEndPoint);

            var payload = new BufferedFilePayload
            {
                Destination = FileDestination.SubjectData,
                Filename = remoteFilename,
                NumBuffers = numBuffers,
                BufferSize = bufferSize
            };

            client.WriteBuffer(System.Text.Encoding.UTF8.GetBytes(
                TcpMessage.Request("ReceiveBufferedFile", payload).Serialize()));

            var ready = client.ReadBufferedSendResponse();
            if (!ready.IsOk)
            {
                Debug.Log($"ReceiveBufferedFile refused by controller: {ready.Command}");
                return;
            }

            using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                for (long k = 0; k < numBuffers; k++)
                {
                    var bytes = reader.ReadBytes(bufferSize);
                    client.WriteBuffer(bytes);
                }
            }

            client.ReadBufferedSendResponse(); // wait for completion acknowledgement
            Debug.Log($"SendBufferedFile complete: {remoteFilename}");
        }
        catch (Exception ex)
        {
            Debug.Log($"SendBufferedFile error: {ex.Message}");
        }
        finally
        {
            client.EndBufferedSend();
        }
    }

    private void ReceiveTextFile(TextFilePayload filePayload)
    {
        var folder = FileLocations.ResolveFolder(filePayload.Destination, filePayload.SubPath);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        var filePath = Path.Combine(folder, filePayload.Filename);
        File.WriteAllText(filePath, filePayload.Content);
    }

    private void ReceiveAudiogram(TextFilePayload filePayload)
    {
        var folder = SharedFileLocations.SubjectMetaFolder;
        var filePath = Path.Combine(folder, filePayload.Filename);
        File.WriteAllText(filePath, filePayload.Content);

        SessionContext.SetAudiogram(SharedFileLocations.AudiogramPath);
        SessionContext.SetLDL(SharedFileLocations.LDLPath);
    }

    private void RunInstaller(string filename)
    {
        string exePath = Path.Combine(
            SharedFileLocations.HtsProjectFolder,
            "Downloads", filename);

        if (!File.Exists(exePath))
        {
            Debug.LogError($"Installer not found: {exePath}");
            return;
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = exePath,
            Arguments = "/SILENT /NORESTART /RESTARTAPPLICATIONS"
        });

#if !UNITY_EDITOR
        if (filename.StartsWith("Hearing_Test_Suite"))
        {
            Application.Quit();
        }
#endif
    }

    #endregion
}
