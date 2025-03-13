using UnityEngine;
using System.Collections;

public class TurandotPupillometer : MonoBehaviour
{
    // TURANDOT FIX
    /*
    int _gx;
    int _gy;
    int _size;
    int _code;

    private bool _stopServer;
    private bool _newValue;
    private Sockets.KTcpListener _socket = null;
    int _readPort = 4975;

    private int _lastCode = 0;
    private bool _isRunning = false;

    private Turandot.ScalarData _data = new Turandot.ScalarData("Pupil");

    public Turandot.ScalarData Data
    {
        get { return _data; }
    }

    void FixedUpdate()
    {
        if (_isRunning)
        {
            if (_newValue)
            {
                _data.SetValue(_code);
                _newValue = false;
            }
            else
            {
                _data.hasChanged = false;
            }
        }
    }

    public void Activate()
    {
        _code = 0;
        _isRunning = true;
        StartServer();
//        IPC.Instance.SendCommand("PupilAnalysis");
    }

    public void StartServer()
    {
        _stopServer = false;

        _socket = new Sockets.KTcpListener();
        _socket.StartListener(_readPort);

        StartCoroutine(TCPServer());

        Debug.Log("started pupillometer TCP server");
    }

    public void StopServer()
    {
        _isRunning = false;
        _stopServer = true;
        if (_socket != null)
        {
            _socket.CloseListener();
            _socket = null;
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
        string command = _socket.ReadStringFromInputStream();

        var parts = command.Split(new char[] { ':' });
        command = parts[0].ToLower();

        switch (command)
        {
            case "ping":
                _socket.SendAcknowledgement();
                break;

            case "value":
                _socket.SendAcknowledgement();
                //Debug.Log(command + ": " + parts[1]);
                _code = int.Parse(parts[1]);
                _newValue = true;
                break;
        }

        _socket.CloseTcpClient();
    }*/
}
