using System;
using System.Collections.Concurrent;   // CHANGED: was System.Collections.Generic Queue
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

using KLibU.Legacy.Network;
using KLibU.Audio;                     // ADDED
// REMOVED: using Microsoft.Graph;     <-- stray IDE auto-using, delete it

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
    private int _leftOffset = 0;
    private int _rightOffset = 1;

    private UdpClient _udpClient;
    private int _udpPort = 52247;

    // CHANGED: plain Queue<T> is not thread-safe. This was being written from the
    // TCP thread and read from the audio thread with no synchronisation.
    private ConcurrentQueue<float[]> _audioQueue = new ConcurrentQueue<float[]>();
    private int _bytesPerBuffer;

    // ================= ADDED: instrumentation state =================
    private AudioChainMonitor _monitor;

    private volatile int _queueDepth;          // approximate; for logging only
    private long _talkStartBuffer;
    private volatile bool _talkActive;
    private volatile int _talkLoopExceptions;  // silent-catch counter, see Talk()
    private volatile bool _offsetsValid;
    // ===============================================================

    #region SINGLETON CREATION
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null) return;

        var prefab = Resources.Load<GameObject>("Intercom Receiver");
        var go = Instantiate(prefab);
        DontDestroyOnLoad(go);
    }
    private static IntercomReceiver _instance;
    #endregion

    private void OnDestroy()
    {
        if (_instance == this)
        {
            Debug.Log("Destroy intercom");
            AudioWatchdog.Unregister(_monitor);      // ADDED
            StopServers();
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _monitor = AudioWatchdog.Register("Intercom");   // ADDED
    }

    private void Start()
    {
        _Init();
    }

    private bool _Init()
    {
        _audioConfig = AudioSettings.GetConfiguration();
        _bytesPerBuffer = _audioConfig.dspBufferSize * 4;

        var go = new GameObject("Discovery Server").AddComponent<NetworkDiscoveryServer>();
        go.transform.parent = this.gameObject.transform;
        _discoveryServer = go.GetComponent<NetworkDiscoveryServer>();

        _leftOffset = HardwareInterface.AdapterMap.Items.FindIndex(item => item.modality == "Audio" && item.location == "Left");
        _rightOffset = HardwareInterface.AdapterMap.Items.FindIndex(item => item.modality == "Audio" && item.location == "Right");

        // ================= ADDED: guard the FindIndex result =================
        // FindIndex returns -1 when not found. data[offset + (-1)] throws
        // IndexOutOfRange on the audio thread, every buffer, forever — and
        // nothing in the original code would ever have told you.
        _offsetsValid = _leftOffset >= 0 && _rightOffset >= 0
                        && _leftOffset < _audioConfig.speakerMode.ChannelCountGuess()
                        && _rightOffset < _audioConfig.speakerMode.ChannelCountGuess();

        if (!_offsetsValid)
        {
            Debug.LogError($"AUDIT INTERCOM adapter map lookup failed: " +
                           $"leftOffset={_leftOffset} rightOffset={_rightOffset}. " +
                           $"Intercom will render silence.");
        }
        else
        {
            Debug.Log($"AUDIT INTERCOM leftOffset={_leftOffset} rightOffset={_rightOffset} " +
                      $"rate={_audioConfig.sampleRate} dspBuffer={_audioConfig.dspBufferSize} " +
                      $"bytesPerBuffer={_bytesPerBuffer}");
        }
        // ====================================================================

        // ================= ADDED: make the chain free-run ====================
        // The chain must count buffers continuously to be usable as a witness
        // for the cross-process comparison. Driving it from Talk() means it
        // only runs while the researcher is speaking.
        StartFreeRunning();
        // ====================================================================

        // ================= ADDED: react to device renegotiation ==============
        AudioSettings.OnAudioConfigurationChanged += OnAudioConfigChanged;
        // ====================================================================

        StartTCPServer();

        return true;
    }

    // ================= ADDED =================
    private void StartFreeRunning()
    {
        // A 1-second silent looping clip keeps the source playing, which keeps
        // OnAudioFilterRead firing whether or not anyone is talking.
        var silence = AudioClip.Create("IntercomSilence",
                                       _audioConfig.sampleRate, 1,
                                       _audioConfig.sampleRate, false);
        _audioSource.clip = silence;
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f;   // 2D — never distance-attenuated
        _audioSource.priority = 0;        // highest — never virtualised
        _audioSource.bypassEffects = false;
        _audioSource.volume = 1f;
        _audioSource.Play();

        Debug.Log("AUDIT INTERCOM chain free-running");
    }

    private void OnAudioConfigChanged(bool deviceWasChanged)
    {
        // The original code cached _audioConfig at Start and never revisited it.
        // If Unity renegotiates the device, dspBufferSize can change and every
        // downstream size assumption in this file becomes wrong.
        var old = _audioConfig;
        _audioConfig = AudioSettings.GetConfiguration();
        _bytesPerBuffer = _audioConfig.dspBufferSize * 4;

        // Anything already queued is sized for the old buffer.
        float[] discard;
        int dropped = 0;
        while (_audioQueue.TryDequeue(out discard)) dropped++;
        _queueDepth = 0;

        Debug.LogWarning($"AUDIT INTERCOM configchange deviceWasChanged={deviceWasChanged} " +
                         $"buffer {old.dspBufferSize}->{_audioConfig.dspBufferSize} " +
                         $"rate {old.sampleRate}->{_audioConfig.sampleRate} " +
                         $"droppedQueued={dropped}");

        // AudioSources do NOT auto-resume after a config change.
        StartFreeRunning();
    }
    // =========================================

    private void StartTCPServer()
    {
        _stopServer = false;

        _ipEndPoint = NetworkUtils.FindNextAvailableEndPoint();

        _listener = new KTcpListener();
        _listener.StartListener(_ipEndPoint, bigEndian: false);

        _readThreadTCP = new Thread(new ThreadStart(TCPServer));
        _readThreadTCP.IsBackground = true;
        _readThreadTCP.Start();

        _discoveryServer.StartReceiving("INTERCOM.RECEIVER", _ipEndPoint.Address.ToString(), _ipEndPoint.Port);
        Debug.Log("started Intercom TCP server on " + _ipEndPoint.ToString());
    }

    private void StopServers()
    {
        _stopServer = true;
        AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigChanged;   // ADDED

        if (_listener != null)
        {
            _listener.CloseListener();
            _listener = null;
        }

        // NOTE: Thread.Abort() is unreliable on Mono and unsupported on .NET 5+.
        // _stopServer plus a bounded Talk() loop is enough to let the thread exit.
        if (_readThreadTCP != null && !_readThreadTCP.Join(500))
            Debug.LogWarning("AUDIT INTERCOM TCP thread did not exit cleanly");

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
            else
            {
                Thread.Sleep(10);
            }
        }
        Debug.Log("AUDIT INTERCOM TCP thread exiting");   // ADDED
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
        // NOTE: no _audioSource.Play() here any more — the chain free-runs.
        // (Play() was also being called from a background thread, which is not
        //  something Unity guarantees.)

        _talkActive = true;
        _talkLoopExceptions = 0;
        _talkStartBuffer = _monitor.Read().BufferCount;
        var t0 = DateTime.UtcNow;
        long buffersReceived = 0;

        Debug.Log($"AUDIT INTERCOM talk started utc={t0:O} dsp={AudioSettings.dspTime:F4}");

        const int MaxConsecutiveExceptions = 20;   // ADDED: escape hatch
        int consecutiveExceptions = 0;

        while (!_stopServer)
        {
            try
            {
                byte[] data = _listener.ReadByteArray();
                consecutiveExceptions = 0;

                if (data == null || data.Length == 4)
                {
                    break;
                }

                var numBuffers = data.Length / _bytesPerBuffer;

                // ADDED: reject misaligned payloads instead of silently truncating
                if (numBuffers * _bytesPerBuffer != data.Length)
                {
                    Debug.LogWarning($"AUDIT INTERCOM misaligned payload len={data.Length} " +
                                     $"bytesPerBuffer={_bytesPerBuffer}");
                }

                int offset = 0;
                for (int k = 0; k < numBuffers; k++)
                {
                    var audioBuffer = new float[_audioConfig.dspBufferSize];
                    Buffer.BlockCopy(data, offset, audioBuffer, 0, _bytesPerBuffer);
                    offset += _bytesPerBuffer;
                    _audioQueue.Enqueue(audioBuffer);
                    Interlocked.Increment(ref _queueDepthRaw);
                    buffersReceived++;
                }
                _queueDepth = _queueDepthRaw;
            }
            catch (Exception ex)
            {
                // ================= CHANGED =================
                // The original swallowed this silently and looped forever. If
                // ReadByteArray throws persistently (socket reset, listener
                // closed), the loop never breaks, the TCP thread never returns
                // to ProcessMessage, and no further Talk command is ever
                // accepted — the intercom is dead until the process restarts.
                _talkLoopExceptions++;
                consecutiveExceptions++;
                if (consecutiveExceptions == 1)
                    Debug.LogWarning($"AUDIT INTERCOM talk loop exception: {ex.Message}");

                if (consecutiveExceptions >= MaxConsecutiveExceptions)
                {
                    Debug.LogError($"AUDIT INTERCOM talk loop abandoned after " +
                                   $"{consecutiveExceptions} consecutive exceptions — " +
                                   $"last: {ex}");
                    break;
                }
                // ==========================================
            }
            Thread.Sleep(10);
        }

        _talkActive = false;
        var buffersRendered = _monitor.Read().BufferCount - _talkStartBuffer;
        Debug.Log($"AUDIT INTERCOM talk finished utc={DateTime.UtcNow:O} " +
                  $"durationMs={(DateTime.UtcNow - t0).TotalMilliseconds:F0} " +
                  $"buffersReceived={buffersReceived} buffersRendered={buffersRendered} " +
                  $"queueDepthAtEnd={_queueDepthRaw} exceptions={_talkLoopExceptions}");
    }

    private int _queueDepthRaw;   // ADDED

    private void ReceiveData()
    {
        _udpClient = new UdpClient(_udpPort);
        _udpClient.Client.ReceiveTimeout = 1000;

        while (!_stopServer)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(0, 0);
                byte[] data = _udpClient.Receive(ref anyIP);

                var numBuffers = data.Length / _bytesPerBuffer;

                int offset = 0;
                for (int k = 0; k < numBuffers; k++)
                {
                    var audioBuffer = new float[_audioConfig.dspBufferSize];
                    Buffer.BlockCopy(data, offset, audioBuffer, 0, _bytesPerBuffer);
                    offset += _bytesPerBuffer;
                    _audioQueue.Enqueue(audioBuffer);
                    Interlocked.Increment(ref _queueDepthRaw);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"AUDIT INTERCOM UDP receive: {ex.Message}");   // CHANGED: was silent
            }
            Thread.Sleep(10);
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        try
        {
            if (!_offsetsValid) return;

            float[] buffer;
            if (_audioQueue.TryDequeue(out buffer))
            {
                Interlocked.Decrement(ref _queueDepthRaw);

                // ADDED: the queued buffer may predate a config change
                int n = Math.Min(buffer.Length, data.Length / channels);

                int offset = 0;
                for (int k = 0; k < n; k++)
                {
                    data[offset + _leftOffset] = buffer[k];
                    data[offset + _rightOffset] = buffer[k];
                    offset += channels;
                }
            }
        }
        catch (Exception ex)
        {
            _monitor.NoteException(ex);
        }
        finally
        {
            // Always runs, even on the silent free-running path. This is what
            // makes the chain a witness: n advances at ~sampleRate/dspBufferSize
            // for as long as Unity's audio graph is alive, talking or not.
            _monitor.NoteBuffer(data, channels);
        }
    }

    // ADDED: expose queue depth to the 1 Hz tick
    private void Update()
    {
        _queueDepth = _queueDepthRaw;
    }
}

// Helper referenced above — put wherever it belongs in your codebase.
internal static class SpeakerModeExtensions
{
    public static int ChannelCountGuess(this AudioSpeakerMode mode)
    {
        switch (mode)
        {
            case AudioSpeakerMode.Mono: return 1;
            case AudioSpeakerMode.Stereo: return 2;
            case AudioSpeakerMode.Quad: return 4;
            case AudioSpeakerMode.Surround: return 5;
            case AudioSpeakerMode.Mode5point1: return 6;
            case AudioSpeakerMode.Mode7point1: return 8;
            default: return 2;
        }
    }
}
