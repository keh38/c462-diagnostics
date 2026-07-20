using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

using KLib;
using KLibU;

using C462.Shared;

public class HardwareInterface : MonoBehaviour
{
    [SerializeField] private ClockSynchronizer _clockSynchronizer;
    [SerializeField] private ClockNetworkInterface _clockNetwork;
    [SerializeField] private DigitimerControl _digitimer;
    [SerializeField] private LEDController _ledController;
    
    private VolumeManager _volumeManager;
    private float _startVolume;

    private HardwareConfiguration _hardwareConfig;
    private AdapterMap _adapterMap;

    // HardwareInterface
    private bool _didInit = false;

    private bool _audioReady = false;
    private bool _hardwareReady = false;
    private bool _errorAcknowledged = false;
    private string _errorMessage = "";

    #region SINGLETON CREATION
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null) return;

        var prefab = Resources.Load<GameObject>("Hardware Interface");
        var go = Instantiate(prefab);
        DontDestroyOnLoad(go);
        // _instance gets set in Awake as usual
    }
    // Singleton
    private static HardwareInterface _instance;
    #endregion

    #region PUBLIC STATIC ACCESSORS
#if HACKING
    public static AdapterMap AdapterMap { get { return AdapterMap.DefaultStereoMap("HD280"); } }
    public static DigitimerControl Digitimer { get { return null; } }
#else
    public static AdapterMap AdapterMap { get { return _instance._adapterMap; } }
    public static DigitimerControl Digitimer { get { return _instance._digitimer; } }
#endif
    public static ClockSynchronizer ClockSync { get { return _instance._clockSynchronizer; } }
    public static VolumeManager VolumeManager { get { return _instance._volumeManager; } }
    public static bool IsReady { get { return _instance._audioReady && _instance._hardwareReady; } }
    public static bool ErrorAcknowledged { get { return _instance._errorAcknowledged; } }
    public static string ErrorMessage { get { return _instance._errorMessage; } }
    public static void AcknowledgeError() { _instance._errorAcknowledged = true; }
    public static LEDController LED { get { return _instance._ledController; } }
    public static bool LockWindowInCenter { get { return _instance._hardwareConfig.LockWindowInCenter; } }
    public static void EnsureInitialized() => _instance._EnsureInitialized();
    public static void CleanUp() => _instance._CleanUp();
    public static bool Yield() => _instance._Yield();
    public static void Resume() => _instance._Resume();
    #endregion

    private void OnDestroy()
    {
        if (_instance == this)
        {
            Debug.Log("OnDestroy hardware interface");
            _CleanUp();
        }
    }

    #region PRIVATE METHODS
    private void Awake()
    {
        // Guard against duplicates (e.g. if Bootstrap already ran)
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _EnsureInitialized();   // fallback path for arbitrary-scene launches
    }

    private void _EnsureInitialized()
    {
        if (_didInit) return;
        _didInit = true;
        _Init();
    }

    private bool _Init()
    {
        StringBuilder errors = new StringBuilder(100);

        _volumeManager = new VolumeManager();
        _startVolume = _volumeManager.GetMasterVolume(VolumeManager.VolumeUnit.Scalar);
#if !UNITY_EDITOR
        _volumeManager.SetMasterVolume(1.0f, VolumeManager.VolumeUnit.Scalar);
#endif

        string configFile = SharedFileLocations.HardwareConfigFile;
        if (File.Exists(configFile))
        {
            _hardwareConfig = Files.XmlDeserialize<HardwareConfiguration>(configFile);
        }
        else
        {
            _hardwareConfig = HardwareConfiguration.GetDefaultConfiguration();
        }
        _adapterMap = _hardwareConfig.GetSelectedMap();
        Debug.Log($"Adapter map contains {_adapterMap.NumChannels} channels");
        SessionContext.Initialize(_adapterMap);

        _audioReady = true;
        var config = AudioSettings.GetConfiguration();
        Debug.Log(AudioSettings.driverCapabilities);
        if (_adapterMap.NumChannels == 8)
        {
            if (AudioSettings.driverCapabilities == AudioSpeakerMode.Mode7point1)
            {
                config.speakerMode = AudioSpeakerMode.Mode7point1;
                AudioSettings.Reset(config);
            }
            else
            {
                Debug.Log("Audio adapter not configured for 7.1");
                errors.AppendLine("- Audio adapter not configured for 7.1");
                _audioReady = false;
            }
        }

        _hardwareReady = true;
        var clockOK = _clockSynchronizer.Initialize(_hardwareConfig.SyncComPort);
        if (!clockOK)
        {
            errors.AppendLine("- Failed to initialize clock sync serial port");
            _hardwareReady = false;
        }
        _clockNetwork.Initialize();

        if (!string.IsNullOrEmpty(_hardwareConfig.LEDComPort))
        {
            var ledOK = _ledController.Initialize(_hardwareConfig.LEDComPort, _hardwareConfig.LEDType, _hardwareConfig.NumPixels, _hardwareConfig.LEDGamma);
            if (ledOK)
            {
                _ledController.Clear();
            }
            else
            {
                _hardwareReady = false;
                errors.AppendLine("- Failed to initialize LED serial port");
            }
        }

        if (_hardwareConfig.UsesDigitimer())
        {
            //_digitimer.Inject(new NullD128ExAPI());
            var success = _digitimer.Initialize();
            if (!success)
            {
                _hardwareReady = false;
                errors.AppendLine("- Failed to initialize Digitimer(s)");
            }
            success = _digitimer.InitializeTrigger(_hardwareConfig.DigitimerComPort);
            if (!success)
            {
                _hardwareReady = false;
                errors.AppendLine("- Failed to initialize Digitimer trigger");
            }
        }
        _errorAcknowledged = false;
        _errorMessage = errors.ToString();

        if (!string.IsNullOrEmpty(_errorMessage))
        {
            Debug.LogError($"Hardware initialization errors:\n{_errorMessage}");
        }

        return true;
    }

    public void _CleanUp()
    {
        if (_volumeManager != null)
        {
            _volumeManager.SetMasterVolume(_startVolume, VolumeManager.VolumeUnit.Scalar);
        }

        _clockNetwork.StopServer();
        _digitimer.CleanUp();
        _ledController.CleanUp();
    }

    private bool _Yield()
    {
        if (!_hardwareConfig.UsesDigitimer())
        {
            return true;    // nothing to disable, so we're good to go
        }

        Debug.Log("Yielding hardware control");

        // TENS trigger disable should already be in effect;
        // this is a safety confirmation, not a command.
        bool safe = _digitimer.VerifyTriggersDisabled();
        _digitimer.CloseHandle();   // release SDK handle regardless
        return safe;           // false → log warning, proceed anyway
    }

    private void _Resume()
    {
        if (_hardwareConfig.UsesDigitimer())
        {
            Debug.Log("Retaking hardware control");
            _digitimer.OpenHandle();
        }
    }

    #endregion
}
