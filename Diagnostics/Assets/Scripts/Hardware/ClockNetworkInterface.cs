using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using KLibU.Net;
using System.Collections.Concurrent;
using HTS.Unity.Tcp;
using static ClockSynchronizer;

public class ClockNetworkInterface : MonoBehaviour
{
    [SerializeField] private ClockSynchronizer _clockSynchronizer;

    private bool _listenerReady = false;

    private KTcpListener _listener = null;
    private bool _stopServer;
    NetworkDiscoveryBeacon _discoveryBeacon;
    private IPEndPoint _ipEndPoint;

    private string _currentSceneName = "";
    private bool _remoteConnected = false;
    private IPEndPoint _remoteEndPoint = null;

    private Thread _readThread;
    private static readonly ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();

    public void Initialize()
    {
        StartServer();
    }

    private void StartServer()
    {
        _remoteConnected = false;
        _stopServer = false;

        _ipEndPoint = Discovery.FindNextAvailableEndPoint();

        _listener = new KTcpListener();
        _listener.StartListener(_ipEndPoint);

        // create thread for reading UDP messages
        _readThread = new Thread(new ThreadStart(TCPServer));
        _readThread.IsBackground = true;
        _readThread.Start();

        Debug.Log("started HTS Sync TCP server on " + _ipEndPoint.ToString());

        _discoveryBeacon = gameObject.AddComponent<NetworkDiscoveryBeacon>();
        _discoveryBeacon.StartBroadcasting(
            name: $"HEARING.TEST.SUITE.SYNC",
            address: _ipEndPoint.Address.ToString(),
            port: _ipEndPoint.Port);

    }

    public void StopServer()
    {
        Debug.Log("Stop sync server");
        _stopServer = true;
        if (_listener != null)
        {
            _listener.CloseListener();
            _listener = null;
        }

        _discoveryBeacon.StopBroadcast();

        if (_readThread != null && _readThread.IsAlive)
        {
            _readThread.Abort();
            Debug.Log("stopped HTS Sync TCP listener");
        }
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

    private void Update()
    {
        while (_mainThreadActions.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }

    void ProcessMessage()
    {
        var receiveTime = HighPrecisionClock.UtcNowIn100nsTicks;

        _listener.AcceptTcpClient();

        var request = _listener.ReadRequest();

        switch (request.Command)
        {
            case "Record":
                var filename = request.GetPayload<string>();
                _listener.WriteResponse(TcpMessage.Ok());
//                _syncStatus = ClockSynchronizer.SyncStatus.Recording;
                // Schedule for main thread
                _mainThreadActions.Enqueue(() => _clockSynchronizer.StartSynchronizing(filename));
                break;
            case "Stop":
                _listener.WriteResponse(TcpMessage.Ok());
                _mainThreadActions.Enqueue(() => _clockSynchronizer.StopSynchronizing());
                //_clockSynchronizer.StopSynchronizing();
                break;
            case "Status":
                _listener.WriteResponse(TcpMessage.Ok(new DataStreamStatusPayload()
                {
                    Status = (int)_clockSynchronizer.Status
                }));
                break;
            case "Sync":
                var clockSyncData = new ClockSyncPayload()
                {
                    T1 = receiveTime,
                    T2 = HighPrecisionClock.UtcNowIn100nsTicks
                };
                _listener.WriteResponse(TcpMessage.Ok(clockSyncData));
                break;
        }
        _listener.CloseTcpClient();
    }

}
