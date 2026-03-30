using UnityEngine;

using KLib;
using KLib.Signals;

using C462.Shared;

public class UnitTests : MonoBehaviour
{
    private SignalManager _signalManager;
    private bool _audioInitialized = false;

    void Start()
    {
        //var acal = AcousticCalibration.Load("", "HD280", "Left");

        //var cal = CalibrationFactory.Load(KLib.Signals.LevelUnits.dB_SPL, "HD280", "Left");
        //float max = cal.GetMax(1000);
        //Debug.Log($"max = {max}");

   }

    public void OnWavFileButtonClick()
    {
        Debug.Log("initializing");
        InitializeStimulusGeneration();
        Debug.Log("done");
    }

    private void InitializeStimulusGeneration()
    {
        _audioInitialized = false;

        var channel = new Channel()
        {
            Active = true,
            Name = "File",
            Modality = KLib.Signals.Modality.Audio,
            Laterality = Laterality.Left,
            Waveform = new UserFile()
            {
                Filename = @"C:\Users\kehan\OneDrive\Documents\EPL\HTS\Projects\Scratch\Resources\Wav Files\measure_50_1.wav"
            },
            Level = new Level()
            {
                Units = LevelUnits.dB_SPL,
                Value = 30
            }
        };

        var config = AudioSettings.GetConfiguration();
        _signalManager = new SignalManager();
        _signalManager.AddChannel(channel);
        _signalManager.Initialize(config.sampleRate, config.dspBufferSize, SessionContext.Signal);

        _audioInitialized = true;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_audioInitialized)
        {
            _signalManager.Synthesize(data);
        }
    }
}
