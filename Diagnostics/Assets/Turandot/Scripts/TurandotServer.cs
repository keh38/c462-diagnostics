using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class TurandotServer : MonoBehaviour
{
    // TURANDOT FIX
    /*
    public TurandotManager manager;

    private Sockets.KTcpListener _socket = null;
    private bool _stopServer;

    private UDPMulticastServer _udpServer;
    private int _port = 4950;

    void Start()
    {

    }

    public void StartServer()
    {
        _stopServer = false;

        _socket = new Sockets.KTcpListener();

        int ntries = 0;
        while (ntries < 10)
        {
            try
            {
                _socket.StartListener(_port, reverseWriteBytes: true);
                break;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _port++;

                ntries++;
                if (ntries == 10)
                {
                    throw (ex);
                }
            }
        }

        StartCoroutine(TCPServer());
        Debug.Log("started Turandot TCP server on port " + _port);

        StartMulticastServer(_port, "BigEndian");
    }

    private void StartMulticastServer(int port, string format)
    {
        var go = new GameObject("Multicast Server").AddComponent<UDPMulticastServer>();
        go.transform.parent = this.gameObject.transform;
        _udpServer = go.GetComponent<UDPMulticastServer>();

        _udpServer.StartReceiving("TURANDOT", port, format);
    }

    public void StopServer()
    {
        _stopServer = true;
        if (_socket != null)
        {
            _socket.CloseListener();
            _socket = null;
        }
        if (_udpServer != null)
        {
            _udpServer.StopReceiving();
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
            if (_socket.Pending())
            {
                ProcessMessage();
            }
            yield return null;
        }
    }

    void ProcessMessage()
    {
        _socket.AcceptTcpClient();
        string message = _socket.ReadStringFromInputStream();
        var parts = message.Split(';');
        string command = parts[0];
        string data = (parts.Length > 1) ? parts[1] : "";
        Debug.Log("Command received: " + command);

        switch (command)
        {
            case "Ping":
                _socket.SendAcknowledgement();
                break;

            case "Load":
                _socket.SendAcknowledgement();
                var fn = _socket.ReadStringFromInputStream();
                manager.RpcLoadParameters(fn);
                _socket.SendAcknowledgement();
                break;

            case "Data":
                _socket.WriteStringAsByteArray(KLib.FileIO.ReadTextFile(manager.MainDataFile));
                _socket.SendAcknowledgement();
                break;

            case "Set":
                _socket.SendAcknowledgement();
                string xml = _socket.ReadStringFromInputStream();
                manager.RpcSetParameters(xml);
                _socket.SendAcknowledgement();
                break;

            case "Show":
                _socket.SendAcknowledgement();
                var stateName = _socket.ReadStringFromInputStream();
                manager.RpcShowState(stateName);
                _socket.SendAcknowledgement();
                break;

            case "Start":
                _socket.SendAcknowledgement();
                manager.RpcStart(true);
                break;

            case "Run":
                _socket.SendAcknowledgement();
                manager.RpcStart(false);
                break;

            case "Add":
                _socket.SendAcknowledgement();
                var expr = _socket.ReadStringFromInputStream();
                manager.RpcAddBlock(expr);
                _socket.SendAcknowledgement();
                break;

            case "Next":
                _socket.SendAcknowledgement();
                manager.RpcNextBlock();
                break;

            case "Abort":
                _socket.SendAcknowledgement();
                manager.RpcAbort();
                break;

            case "Message":
                _socket.SendAcknowledgement();
                var msg = _socket.ReadStringFromInputStream();
                _socket.SendAcknowledgement();
                manager.RpcMessage(msg);
                break;

            case "GetCurrentProject":
                _socket.WriteStringAsByteArray(SubjectManager.CurrentProject);
                break;

            case "GetCurrentSubject":
                _socket.WriteStringAsByteArray(SubjectManager.CurrentSubject);
                break;

            case "GetMetrics":
                _socket.WriteStringAsByteArray(KLib.FileIO.JSONSerializeToString(SubjectManager.MetaData.metrics));
                break;

            case "GetMetricPropVals":
                _socket.WriteStringAsByteArray(KLib.FileIO.JSONSerializeToString(SubjectManager.MetaData.GetMetricPropVals()));
                break;

            case "GetProjects":
                var projects = new List<string>(Directory.GetDirectories(DataFileLocations.DataRoot));
                projects.Sort();
                var sb = new StringBuilder();
                foreach (var p in projects) sb.Append(Path.GetFileName(p) + ";");
                _socket.WriteStringAsByteArray(sb.ToString());
                break;

            case "GetSubjects":
                var project = data;
                var subjects = new List<string>(Directory.GetDirectories(KLib.FileIO.CombinePaths(DataFileLocations.DataRoot, project, "Subjects")));
                subjects.Sort();
                sb = new StringBuilder();
                foreach (var s in subjects) sb.Append(Path.GetFileName(s) + ";");
                _socket.WriteStringAsByteArray(sb.ToString());
                break;

            case "SetSubject":
                _socket.SendAcknowledgement();

                var subjInfo = data.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
                SubjectManager.Instance.ChangeSubject(subjInfo[0], subjInfo[1]);
                break;

            case "Exit":
                manager.RpcExit();
                _socket.SendAcknowledgement();
                break;
        }


        _socket.CloseTcpClient();
    }
    */
}