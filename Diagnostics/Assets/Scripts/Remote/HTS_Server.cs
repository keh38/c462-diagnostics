using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Net.Sockets;

using KLib.Network;

public class HTS_Server : MonoBehaviour
{
    //a true/false variable for connection status
    private bool _listenerReady = false;

    KTcpClient _client;
    private KTcpListener _listener = null;
    private bool _stopServer;
    private NetworkDiscoveryServer _discoveryServer;
    private string _address;
    private int _port = 4950;

    private bool _remoteConnected = false;

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
                ProcessMessage();
            }
            yield return null;
        }
    }

    void ProcessMessage()
    {
        bool exit = false;

        //TestState receivedTestState = null;
        bool result = true;

        _listener.AcceptTcpClient();

        string input = _listener.ReadString();
        var parts = input.Split(new char[] { ':' });
        string command = parts[0];
        string data = null;
        if (parts.Length > 1)
        {
            data = parts[1];
        }

        Debug.Log("Command received: " + command);

        switch (command)
        {
            case "DoNextTrial":
                _listener.SendAcknowledgement();
//                _scene.DoNextTrial();
                break;

            //case "Disconnect":
            //    _listener.SendAcknowledgement();
            //    _scene.Disconnect();
            //    break;

            //case "EndTest":
            //    _listener.SendAcknowledgement();
            //    _scene.EndTest();
            //    break;

            //case "GetResources":
            //    var resources = _scene.EnumerateResources();
            //    _listener.WriteByteArray(Message.ToProtoBuf(resources));
            //    break;

            //case "GetSubjectInfo":
            //    var subjInfo = _scene.GetSubjectInfo();
            //    _listener.WriteByteArray(Message.ToProtoBuf(subjInfo));
            //    break;

            case "Ping":
                _listener.SendAcknowledgement();
//                _scene.RemoteIPAddress = data;
                Debug.Log("remote address = " + data);
                _remoteConnected = true;
//                _scene.Ping();
                break;

            //case "Play":
            //    _listener.SendAcknowledgement();
            //    _scene.PlayItem(int.Parse(data));
            //    break;

            //case "Return":
            //    _listener.SendAcknowledgement();
            //    exit = true;
            //    break;

            //case "RandomList":
            //    receivedTestState = ReceiveTestState();
            //    receivedTestState = _scene.InitializeRandomList(false);
            //    SendTestStateResponse(receivedTestState);
            //    break;

            //case "SetLevels":
            //    receivedTestState = ReceiveTestState();
            //    _listener.SendAcknowledgement();
            //    _scene.SetLevels(receivedTestState.level, receivedTestState.snr);
            //    break;

            //case "SetList":
            //    receivedTestState = ReceiveTestState();
            //    receivedTestState = _scene.InitializeList(receivedTestState.listNum, false);
            //    SendTestStateResponse(receivedTestState);
            //    break;

            //case "SetMaskerState":
            //    _listener.SendAcknowledgement();
            //    _scene.SetMaskerState(bool.Parse(data));
            //    break;

            //case "SetPupilSettings":
            //    var pupilSettings = ReceivePupilSettings();
            //    _listener.SendAcknowledgement();
            //    _scene.SetPupilSettings(pupilSettings);
            //    break;

            //case "SetTest":
            //    receivedTestState = ReceiveTestState();
            //    receivedTestState = _scene.InitializeTest(receivedTestState.testName);
            //    SendTestStateResponse(receivedTestState);
            //    break;

            //case "SetTestSNRVector":
            //    receivedTestState = ReceiveTestState();
            //    result = _scene.SetTestLevels(receivedTestState.level, receivedTestState.testSNRs);
            //    _listener.SendAcknowledgement(result);
            //    break;

            //case "ShowMessage":
            //    _listener.SendAcknowledgement();
            //    _scene.ShowMessage(data);
            //    break;

            //case "StartTest":
            //    _listener.SendAcknowledgement();
            //    _scene.StartTest(int.Parse(data));
            //    break;

            default:
                _listener.SendAcknowledgement(false);
                break;
        }

        _listener.CloseTcpClient();

//        if (exit) _scene.Return();
    }

    //    private TestState ReceiveTestState()
    //    {
    //        TestState state = null;
    //        var bytes = _listener.ReadByteArrayFromInputStream();
    //        if (bytes != null)
    //        {
    //            state = Message.FromProtoBuf<TestState>(bytes);
    //        }

    //        return state;
    //    }

    //    private SpeechPupilConfiguration ReceivePupilSettings()
    //    {
    //        SpeechPupilConfiguration settings = null;
    //        var bytes = _listener.ReadByteArrayFromInputStream();
    //        if (bytes != null)
    //        {
    //            settings = Message.FromProtoBuf<SpeechPupilConfiguration>(bytes);
    //        }

    //        return settings;
    //    }

    //    private void SendTestStateResponse(TestState response)
    //    {
    //        if (response != null)
    //        {
    //            _listener.WriteByteArray(SRI.Messages.Message.ToProtoBuf(response));
    //        }
    //        else
    //        {
    //            _listener.SendAcknowledgement(false);
    //        }
    //    }

    //    public static string FindServerAddress()
    //    {
    //        System.Diagnostics.Process p = null;
    //        string output = string.Empty;
    //        string address = string.Empty;

    //        try
    //        {
    //            p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("arp", "-a")
    //            {
    //                CreateNoWindow = true,
    //                UseShellExecute = false,
    //                RedirectStandardOutput = true
    //            });

    //            output = p.StandardOutput.ReadToEnd();
    //            p.Close();

    //            foreach (var line in output.Split(new char[] { '\n', '\r' }))
    //            {
    //                // Parse out all the MAC / IP Address combinations
    //                if (!string.IsNullOrEmpty(line))
    //                {
    //                    var pieces = (from piece in line.Split(new char[] { ' ', '\t' })
    //                                  where !string.IsNullOrEmpty(piece)
    //                                  select piece).ToArray();

    //                    if (line.StartsWith("Interface:") && pieces[1].StartsWith("169.254"))
    //                    {
    //                        address = pieces[1];
    //                        ServerAddress = address;
    //                        return address;
    //                    }
    //                    if (line.StartsWith("Interface:") && pieces[1].StartsWith("11.12"))
    //                    {
    //                        address = pieces[1];
    //                        ServerAddress = address;
    //                        return address;
    //                    }
    //                }
    //            }

    //        }
    //        catch (Exception ex)
    //        {
    //            throw new Exception("IPInfo: Error Retrieving 'arp -a' Results", ex);
    //        }
    //        finally
    //        {
    //            if (p != null)
    //            {
    //                p.Close();
    //            }
    //        }
    //#if UNITY_EDITOR
    //        if (string.IsNullOrEmpty(address))
    //        {
    //            address = "localhost";
    //            ServerAddress = address;
    //        }
    //#endif

    //        return address;
    //    }
    #endregion
}
