using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Net.Sockets;

//using SRI.Messages;

public class SRITCP : MonoBehaviour
{
/*    //a true/false variable for connection status
    private bool _listenerReady = false;

    Sockets.KTcpClient _client;
    private Sockets.KTcpListener _listener = null;
    private bool _stopServer;
    private UDPMulticastServer _udpServer;
    private int _port = 4950;

    private SpeechReceptionInterface _scene;

    public static string ServerAddress { private set; get; }

    // Make singleton
    private static SRITCP _instance;
    public static SRITCP Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gobj = GameObject.Find("SRI");
                if (gobj != null)
                {
                    _instance = gobj.GetComponent<SRITCP>();
                }
                else
                {
                    _instance = new GameObject("SRI").AddComponent<SRITCP>();
                }
                _instance.Init();
            }
            return _instance;
        }
    }

    public void Init()
    {
        var go = new GameObject("Multicast Server").AddComponent<UDPMulticastServer>();
        go.transform.parent = this.gameObject.transform;
        _udpServer = go.GetComponent<UDPMulticastServer>();

        DontDestroyOnLoad(this);
    }

    public bool Use { get; private set; }
    public string Host { get; private set; }

    public void StartServer(SpeechReceptionInterface scene)
    {
        _scene = scene;
        _stopServer = false;

        Use = true;

        _listener = new Sockets.KTcpListener();
        _listener.StartListener(_port, reverseWriteBytes: false);

        StartCoroutine(TCPServer());
        _udpServer.StartReceiving("SRI", _port, "LittleEndian");
        Host = FindServerAddress() + ":" + _port;

        Debug.Log("started SRI TCP listener on " + Host);
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

        _udpServer.StopReceiving();
        Debug.Log("stopped SRI TCP listener");
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

        TestState receivedTestState = null;
        bool result = true;

        _listener.AcceptTcpClient();

        string input = _listener.ReadStringFromInputStream();
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
                _scene.DoNextTrial();
                break;

            case "Disconnect":
                _listener.SendAcknowledgement();
                _scene.Disconnect();
                break;

            case "EndTest":
                _listener.SendAcknowledgement();
                _scene.EndTest();
                break;

            case "GetResources":
                var resources = _scene.EnumerateResources();
                _listener.WriteByteArray(Message.ToProtoBuf(resources));
                break;

            case "GetSubjectInfo":
                var subjInfo = _scene.GetSubjectInfo();
                _listener.WriteByteArray(Message.ToProtoBuf(subjInfo));
                break;

            case "Ping":
                _listener.SendAcknowledgement();
                _scene.RemoteIPAddress = data;
                Debug.Log("remote address = " + data);
                _scene.Ping();
                break;

            case "Play":
                _listener.SendAcknowledgement();
                _scene.PlayItem(int.Parse(data));
                break;

            case "Return":
                _listener.SendAcknowledgement();
                exit = true;
                break;

            case "RandomList":
                receivedTestState = ReceiveTestState();
                receivedTestState = _scene.InitializeRandomList(false);
                SendTestStateResponse(receivedTestState);
                break;

            case "SetLevels":
                receivedTestState = ReceiveTestState();
                _listener.SendAcknowledgement();
                _scene.SetLevels(receivedTestState.level, receivedTestState.snr);
                break;

            case "SetList":
                receivedTestState = ReceiveTestState();
                receivedTestState = _scene.InitializeList(receivedTestState.listNum, false);
                SendTestStateResponse(receivedTestState);
                break;

            case "SetMaskerState":
                _listener.SendAcknowledgement();
                _scene.SetMaskerState(bool.Parse(data));
                break;

            case "SetPupilSettings":
                var pupilSettings = ReceivePupilSettings();
                _listener.SendAcknowledgement();
                _scene.SetPupilSettings(pupilSettings);
                break;

            case "SetTest":
                receivedTestState = ReceiveTestState();
                receivedTestState = _scene.InitializeTest(receivedTestState.testName);
                SendTestStateResponse(receivedTestState);
                break;

            case "SetTestSNRVector":
                receivedTestState = ReceiveTestState();
                result = _scene.SetTestLevels(receivedTestState.level, receivedTestState.testSNRs);
                _listener.SendAcknowledgement(result);
                break;

            case "ShowMessage":
                _listener.SendAcknowledgement();
                _scene.ShowMessage(data);
                break;

            case "StartTest":
                _listener.SendAcknowledgement();
                _scene.StartTest(int.Parse(data));
                break;

            default:
                _listener.SendAcknowledgement(false);
                break;
        }

        _listener.CloseTcpClient();

        if (exit) _scene.Return();
    }

    private TestState ReceiveTestState()
    {
        TestState state = null;
        var bytes = _listener.ReadByteArrayFromInputStream();
        if (bytes != null)
        {
            state = Message.FromProtoBuf<TestState>(bytes);
        }

        return state;
    }

    private SpeechPupilConfiguration ReceivePupilSettings()
    {
        SpeechPupilConfiguration settings = null;
        var bytes = _listener.ReadByteArrayFromInputStream();
        if (bytes != null)
        {
            settings = Message.FromProtoBuf<SpeechPupilConfiguration>(bytes);
        }

        return settings;
    }

    private void SendTestStateResponse(TestState response)
    {
        if (response != null)
        {
            _listener.WriteByteArray(SRI.Messages.Message.ToProtoBuf(response));
        }
        else
        {
            _listener.SendAcknowledgement(false);
        }
    }

    public static string FindServerAddress()
    {
        System.Diagnostics.Process p = null;
        string output = string.Empty;
        string address = string.Empty;

        try
        {
            p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("arp", "-a")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });

            output = p.StandardOutput.ReadToEnd();
            p.Close();

            foreach (var line in output.Split(new char[] { '\n', '\r' }))
            {
                // Parse out all the MAC / IP Address combinations
                if (!string.IsNullOrEmpty(line))
                {
                    var pieces = (from piece in line.Split(new char[] { ' ', '\t' })
                                  where !string.IsNullOrEmpty(piece)
                                  select piece).ToArray();

                    if (line.StartsWith("Interface:") && pieces[1].StartsWith("169.254"))
                    {
                        address = pieces[1];
                        ServerAddress = address;
                        return address;
                    }
                    if (line.StartsWith("Interface:") && pieces[1].StartsWith("11.12"))
                    {
                        address = pieces[1];
                        ServerAddress = address;
                        return address;
                    }
                }
            }

        }
        catch (Exception ex)
        {
            throw new Exception("IPInfo: Error Retrieving 'arp -a' Results", ex);
        }
        finally
        {
            if (p != null)
            {
                p.Close();
            }
        }
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(address))
        {
            address = "localhost";
            ServerAddress = address;
        }
#endif

        return address;
    }
*/
}
