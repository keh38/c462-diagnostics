using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

using KLib;

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

    private bool _audioReady = false;
    private bool _errorAcknowledged = false;
    private string _errorMessage = "";

    #region SINGLETON CREATION
    // Singleton
    private static HardwareInterface _instance;
    private static HardwareInterface instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gobj = GameObject.Find("HardwareInterface");
                _instance = gobj.GetComponent<HardwareInterface>();
                DontDestroyOnLoad(gobj);
            }
            return _instance;
        }
    }
    #endregion

    #region PUBLIC STATIC ACCESSORS
    public static void Initialize() { instance._Init(); }
#if HACKING
    public static AdapterMap AdapterMap { get { return AdapterMap.DefaultStereoMap("HD280"); } }
    public static DigitimerControl Digitimer { get { return null; } }
#else
    public static AdapterMap AdapterMap { get { return instance._adapterMap; } }
    public static DigitimerControl Digitimer { get { return instance._digitimer; } }
#endif
    public static ClockSynchronizer ClockSync { get { return instance._clockSynchronizer; } }
    public static VolumeManager VolumeManager { get { return instance._volumeManager; } }
    public static bool IsReady { get { return instance._audioReady; } }
    public static bool ErrorAcknowledged { get { return instance._errorAcknowledged; } }
    public static string ErrorMessage { get { return instance._errorMessage; } }
    public static void AcknowledgeError() { instance._errorAcknowledged = true; }
    public static LEDController LED { get { return instance._ledController; } }
    public static void CleanUp() => instance._CleanUp();
    #endregion

    #region PRIVATE METHODS

    private bool _Init()
    {
        StringBuilder errors = new StringBuilder(100);

        _volumeManager = new VolumeManager();
        _startVolume = _volumeManager.GetMasterVolume(VolumeManager.VolumeUnit.Scalar);
        _volumeManager.SetMasterVolume(1.0f, VolumeManager.VolumeUnit.Scalar);

        string configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EPL", "HTS");
        string configFile = Path.Combine(configFolder, "HardwareConfiguration.xml");
        if (File.Exists(configFile))
        {
            _hardwareConfig = FileIO.XmlDeserialize<HardwareConfiguration>(Path.Combine(configFolder, "HardwareConfiguration.xml"));
        }
        else
        {
            _hardwareConfig = HardwareConfiguration.GetDefaultConfiguration();
        }
        _adapterMap = _hardwareConfig.GetSelectedMap();
        Debug.Log($"Adapter map contains {_adapterMap.NumChannels} channels");

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

        var clockOK = _clockSynchronizer.Initialize(_hardwareConfig.SyncComPort);
        if (!clockOK)
        {
            errors.AppendLine("- Failed to initialize clock sync serial port");
        }
        _clockNetwork.Initialize();

        if (!string.IsNullOrEmpty(_hardwareConfig.LEDComPort))
        {
            var ledOK = _ledController.Initialize(_hardwareConfig.LEDComPort, _hardwareConfig.LEDGamma);
            if (ledOK)
            {
                _ledController.Clear();
            }
            else
            {
                errors.AppendLine("- Failed to initialize LED serial port");
            }
        }

        if (_hardwareConfig.UsesDigitimer())
        {
            var success = _digitimer.Initialize();
            if (!success)
            {
                errors.AppendLine("- Failed to initialize Digitimer(s)");
            }
        }
        _errorAcknowledged = false;
        _errorMessage = errors.ToString();

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


    #endregion
}
