using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using KLib;

public class HardwareInterface : MonoBehaviour
{
    [SerializeField] private ClockSynchronizer _clockSynchronizer;
    [SerializeField] private DigitimerControl _digitimer;

    private VolumeManager _volumeManager;
    private float _startVolume;

    private HardwareConfiguration _hardwareConfig;
    private AdapterMap _adapterMap;

    private bool _audioReady = false;
    private bool _errorAcknowledged = false;

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
    public static AdapterMap AdapterMap { get { return instance._adapterMap; } }
    public static ClockSynchronizer ClockSync { get { return instance._clockSynchronizer; } }
    public static DigitimerControl Digitimer { get { return instance._digitimer; } }
    public static VolumeManager VolumeManager { get { return instance._volumeManager; } }
    public static bool IsReady { get { return instance._audioReady; } }
    public static bool ErrorAcknowledged { get { return instance._errorAcknowledged; } }
    public static void AcknowledgeError() { instance._errorAcknowledged = true; }
    #endregion

    #region PRIVATE METHODS
    private void OnDestroy()
    {
        if (_volumeManager != null)
        {
            _volumeManager.SetMasterVolume(_startVolume, VolumeManager.VolumeUnit.Scalar);
        }
    }

    private bool _Init()
    {
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
                _audioReady = false;
            }
        }

        _clockSynchronizer.Initialize(_hardwareConfig.SyncComPort);

        if (_hardwareConfig.UsesDigitimer())
        {
            _digitimer.Initialize();
        }
        _errorAcknowledged = false;

        return true;
    }
    #endregion
}
