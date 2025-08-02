using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using KLib;
using KLib.Signals;
using KLib.Signals.Waveforms;

public class CalibrationController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private InputActionAsset _actions;
    [SerializeField] private GameObject _quitPanel;

    private SignalManager _signalManager;
    private bool _audioInitialized = false;

    private string _mySceneName = "Calibration";

    private InputAction _abortAction;

    private void Awake()
    {
        _abortAction = _actions.FindAction("Abort");
        _abortAction.Enable();
        _abortAction.performed += OnAbortAction;
    }

    private void Start()
    {
        HTS_Server.SetCurrentScene(_mySceneName, this);
        InitializeStimulusGeneration();
    }

    private void InitializeStimulusGeneration()
    {
        _signalManager = new SignalManager();
        _signalManager.AdapterMap = HardwareInterface.AdapterMap;

        var toneChannel = new Channel()
        {
            Name = "ToneLeft",
            Modality = KLib.Signals.Enumerations.Modality.Audio,
            Laterality = Laterality.Left,
            active = false,
            waveform = new Sinusoid(),
            level = new Level()
            {
                Units = LevelUnits.dB_Vrms,
                Value = -40
            }
        };

        var noiseChannel = new Channel()
        {
            Name = "NoiseLeft",
            Modality = KLib.Signals.Enumerations.Modality.Audio,
            Laterality = Laterality.Binaural,
            waveform = new Noise(),
            active = false,
            level = new Level()
            {
                Units = LevelUnits.dB_Vrms,
                Value = -40
            }
        };

        _signalManager.AddChannel(toneChannel);

        var rightTone = toneChannel.Clone();
        rightTone.Name = "ToneRight";
        rightTone.Laterality = Laterality.Right;
        _signalManager.AddChannel(rightTone);

        _signalManager.AddChannel(noiseChannel);
        var rightNoise = noiseChannel.Clone();
        rightNoise.Name = "NoiseRight";
        rightNoise.Laterality = Laterality.Right;
        _signalManager.AddChannel(rightNoise);

        var audioConfig = AudioSettings.GetConfiguration();
        _signalManager.Initialize(audioConfig.sampleRate, audioConfig.dspBufferSize);

        _audioInitialized = true;
    }

    void OnAbortAction(InputAction.CallbackContext context)
    {
        _abortAction.Disable();

        _quitPanel.SetActive(true);
    }

    public void OnQuitConfirmButtonClick()
    {
        Return();
    }

    public void OnQuitCancelButtonClick()
    {
        _quitPanel.SetActive(false);
        _abortAction.Enable();
    }

    private void Return()
    {
        SceneManager.LoadScene("Home");
    }

    void PlayTone(string data)
    {
        var parts = data.Split(':');
        var ear = parts[0];
        var level = float.Parse(parts[1]);
        var freq = float.Parse(parts[2]);


        string chName = $"Tone{ear}";

        (_signalManager[chName].waveform as Sinusoid).Frequency_Hz = freq;
        _signalManager[chName].level.Value = level;
        _signalManager[chName].SetActive(true);
    }

    void PlayNoise(string data)
    {
        var parts = data.Split(':');
        var ear = parts[0];
        var level = float.Parse(parts[1]);


        string chName = $"Noise{ear}";

        _signalManager[chName].level.Value = level;
        _signalManager[chName].SetActive(true);
    }

    void Stop()
    {
        foreach (var channel in _signalManager.channels)
        {
            channel.SetActive(false);
        }
    }

    void IRemoteControllable.ProcessRPC(string command, string data)
    {
        switch (command)
        {
            case "Noise":
                PlayNoise(data);
                break;
            case "Tone":
                PlayTone(data);
                break;
            case "Stop":
                Stop();
                break;
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_audioInitialized)
        {
            _signalManager.Synthesize(data);
        }
    }

}
