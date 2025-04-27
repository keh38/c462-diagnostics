using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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

    private Thread _readThreadTCP;
    private Thread _readThreadUDP;

    private AudioConfiguration _audioConfig;

    private UdpClient _udpClient;
    private int _udpPort = 52247;

    private Queue<float[]> _audioQueue = new Queue<float[]>();
    private int _bytesPerBuffer;
        
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
    public static void CleanUp() { instance.StopServers(); }
    #endregion

    private void OnDestroy()
    {
        //Debug.Log("Destroy intercom");
        //StopServers();
    }

    private bool _Init()
    {
        _audioConfig = AudioSettings.GetConfiguration();
        _bytesPerBuffer = _audioConfig.dspBufferSize * 4;

        var go = new GameObject("Discovery Server").AddComponent<NetworkDiscoveryServer>();
        go.transform.parent = this.gameObject.transform;
        _discoveryServer = go.GetComponent<NetworkDiscoveryServer>();

        StartTCPServer();
        //StartReceivingUDP();

        return true;
    }

    private void StartTCPServer()
    {
        _stopServer = false;

        _ipEndPoint = NetworkUtils.FindNextAvailableEndPoint();
        //_address = NetworkUtils.FindServerAddress();

        _listener = new KTcpListener();
        _listener.StartListener(_ipEndPoint, bigEndian: false);

        // create thread for reading UDP messages
        _readThreadTCP = new Thread(new ThreadStart(TCPServer));
        _readThreadTCP.IsBackground = true;
        _readThreadTCP.Start();

        _discoveryServer.StartReceiving("INTERCOM.RECEIVER", _ipEndPoint.Address.ToString(), _ipEndPoint.Port);
        Debug.Log("started Intercom TCP server on " + _ipEndPoint.ToString());
    }

    private void StopServers()
    {
        _stopServer = true;
        if (_listener != null)
        {
            _listener.CloseListener();
            _listener = null;
        }

        _readThreadTCP.Abort();
        //_readThreadUDP.Abort();
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
            case "GetConfig":
                _listener.WriteStringAsByteArray($"{_audioConfig.sampleRate}:{_audioConfig.dspBufferSize}");
                break;
            case "Talk":
                Talk();
                break;
        }
        _listener.CloseTcpClient();
    }

    public void StartReceivingUDP()
    {
        _readThreadUDP = new Thread(new ThreadStart(ReceiveData));
        _readThreadUDP.IsBackground = true;
        _readThreadUDP.Start();
    }

    private void Talk()
    {
        while (true)
        {
            try
            {
                byte[] data = _listener.ReadByteArray();

                if (data == null || data.Length == 4)
                {
                    Debug.Log("done");
                    break;
                }

                var numBuffers = data.Length / _bytesPerBuffer;

                int offset = 0;
                for (int k = 0; k < numBuffers; k++)
                {
                    var audioBuffer = new float[_audioConfig.dspBufferSize];
                    Buffer.BlockCopy(data, offset, audioBuffer, 0, _bytesPerBuffer);
                    offset += _bytesPerBuffer;
                    _audioQueue.Enqueue(audioBuffer);
                }
            }
            catch (Exception ex) { }
        }
    }


    private void ReceiveData()
    {
        _udpClient = new UdpClient(_udpPort);
        _udpClient.Client.ReceiveTimeout = 1000;
        //IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, _udpPort);
        //IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("169.254.10.78"), _udpPort);

        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("169.254.10.78"), _udpPort);
                // receive bytes
                byte[] data = _udpClient.Receive(ref anyIP);

                var numBuffers = data.Length / _bytesPerBuffer;

                int offset = 0;
                for (int k = 0; k < numBuffers; k++)
                {
                    var audioBuffer = new float[_audioConfig.dspBufferSize];
                    Buffer.BlockCopy(data, offset, audioBuffer, 0, _bytesPerBuffer);
                    offset += _bytesPerBuffer;
                    _audioQueue.Enqueue(audioBuffer);
                }
            }
            catch (Exception ex) { }
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_audioQueue.Count > 0)
        {
            var buffer = _audioQueue.Dequeue();
            int offset = 0;
            for (int k=0; k<buffer.Length; k++)
            {
                data[offset] = buffer[k];
                data[offset + 1] = buffer[k];
                offset += channels;
            }
        }
    }

}
