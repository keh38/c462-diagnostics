using UnityEngine;

using KLib.Signals;
using KLib.Signals.Waveforms;
using KLib.Signals.Calibration;
using KLib.Signals.Waveforms;
using KLib;

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
            active = true,
            Name = "File",
            Modality = KLib.Signals.Enumerations.Modality.Audio,
            Laterality = Laterality.Left,
            waveform = new UserFile()
            {
                fileName = @"C:\Users\kehan\OneDrive\Documents\EPL\HTS\Projects\Scratch\Resources\Wav Files\measure_50_1.wav"
            },
            level = new Level()
            {
                Units = LevelUnits.dB_SPL,
                Value = 30
            }
        };

        var config = AudioSettings.GetConfiguration();
        _signalManager = new SignalManager(config.sampleRate, config.dspBufferSize);
        _signalManager.AdapterMap = AdapterMap.DefaultStereoMap("HD280");
        _signalManager.AddChannel(channel);
        _signalManager.Initialize();

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
