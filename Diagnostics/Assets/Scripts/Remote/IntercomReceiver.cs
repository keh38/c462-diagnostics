using System;
using System.Net;
using System.Threading;
using UnityEngine;

using KLib.Network;

public class IntercomReceiver : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    private KTcpListener _listener = null;
    private bool _stopServer;
    private NetworkDiscoveryServer _discoveryServer;
    private IPEndPoint _ipEndPoint;

    private Thread _readThread;

    #region SINGLETON CREATION
    // Singleton
    private static IntercomReceiver _instance;
    private static IntercomReceiver instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gobj = GameObject.Find("Intercom Receiver");
                _instance = gobj.GetComponent<IntercomReceiver>();
                DontDestroyOnLoad(gobj);
            }
            return _instance;
        }
    }
    #endregion

    #region PUBLIC STATIC ACCESSORS
    public static void Initialize() { instance._Init(); }
    #endregion

    private bool _Init()
    {
        var go = new GameObject("Discovery Server").AddComponent<NetworkDiscoveryServer>();
        go.transform.parent = this.gameObject.transform;
        _discoveryServer = go.GetComponent<NetworkDiscoveryServer>();
        StartServer();

        return true;
    }

    private void StartServer()
    {
        _stopServer = false;

        _ipEndPoint = NetworkUtils.FindNextAvailableEndPoint();
        //_address = NetworkUtils.FindServerAddress();

        _listener = new KTcpListener();
        _listener.StartListener(_ipEndPoint, bigEndian: false);

        // create thread for reading UDP messages
        _readThread = new Thread(new ThreadStart(TCPServer));
        _readThread.IsBackground = true;
        _readThread.Start();

        _discoveryServer.StartReceiving("INTERCOM.RECEIVER", _ipEndPoint.Address.ToString(), _ipEndPoint.Port);
        Debug.Log("started Intercom TCP server on " + _ipEndPoint.ToString());
    }

    public void StopServer()
    {
        _stopServer = true;
        if (_listener != null)
        {
            _listener.CloseListener();
            _listener = null;
        }

        _readThread.Abort();
        _discoveryServer.StopReceiving();
        Debug.Log("stopped Intercom TCP listener");
    }

    private void TCPServer()
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
        }
    }

    void ProcessMessage()
    {
        _listener.AcceptTcpClient();

        string command = _listener.ReadString();

        switch (command)
        {
            case "Ping":
                _listener.SendAcknowledgement();
                break;
            //case "Stop":
            //    _listener.SendAcknowledgement();
            //    //_clockSynchronizer.StopSynchronizing();
            //    break;
            //case "Status":
            //    _listener.Write((int)_clockSynchronizer.Status);
            //    break;
            //case "Sync":
            //    var byteArray = new byte[16];
            //    Buffer.BlockCopy(new long[] { receiveTime, HighPrecisionClock.UtcNowIn100nsTicks }, 0, byteArray, 0, 16);
            //    _listener.Write(byteArray);
            //    break;
        }
        _listener.CloseTcpClient();
    }
}
