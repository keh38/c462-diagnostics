using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using KLib.Network;

public class ClockNetworkInterface : MonoBehaviour
{
    [SerializeField] private ClockSynchronizer _clockSynchronizer;

    private bool _listenerReady = false;

    private KTcpListener _listener = null;
    private bool _stopServer;
    private NetworkDiscoveryServer _discoveryServer;
    private IPEndPoint _ipEndPoint;

    private string _currentSceneName = "";
    private bool _remoteConnected = false;
    private IPEndPoint _remoteEndPoint = null;

    private Thread _readThread;

    private void Start()
    {
        var go = new GameObject("Discovery Server").AddComponent<NetworkDiscoveryServer>();
        go.transform.parent = this.gameObject.transform;
        _discoveryServer = go.GetComponent<NetworkDiscoveryServer>();
        StartServer();
    }

    private void StartServer()
    {
        _remoteConnected = false;
        _stopServer = false;

        _ipEndPoint = NetworkUtils.FindNextAvailableEndPoint();
        //_address = NetworkUtils.FindServerAddress();

        _listener = new KTcpListener();
        _listener.StartListener(_ipEndPoint, bigEndian: false);

        // create thread for reading UDP messages
        _readThread = new Thread(new ThreadStart(TCPServer));
        _readThread.IsBackground = true;
        _readThread.Start();

        _discoveryServer.StartReceiving("HEARING.TEST.SUITE.SYNC", _ipEndPoint.Address.ToString(), _ipEndPoint.Port);
        Debug.Log("started HTS Sync TCP server on " + _ipEndPoint.ToString());
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
        Debug.Log("stopped HTS Sync TCP listener");
    }

    void OnDestroy()
    {
        StopServer();
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
        var receiveTime = HighPrecisionClock.UtcNowIn100nsTicks;

        _listener.AcceptTcpClient();

        string input = _listener.ReadString();

        var parts = input.Split(new char[] { ':' }, 2);
        string command = parts[0];
        string data = null;
        if (parts.Length > 1)
        {
            data = parts[1];
        }

        switch (command)
        {
            case "Record":
                _listener.SendAcknowledgement();
                //_clockSynchronizer.StartSynchronizing(data);
                break;
            case "Stop":
                _listener.SendAcknowledgement();
                //_clockSynchronizer.StopSynchronizing();
                break;
            case "Status":
                _listener.Write((int)_clockSynchronizer.Status);
                break;
            case "Sync":
                _listener.Write(receiveTime);
                _listener.Write(HighPrecisionClock.UtcNowIn100nsTicks);
                break;
        }
        _listener.CloseTcpClient();
    }

}
