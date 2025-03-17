using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KLib.Signals;
using KLib.Signals.Waveforms;

namespace Turandot.Interactive
{
    public class InteractiveSettings
    {
        public string Name { set; get; }
        public SignalManager SigMan { get; set; }

        public InteractiveSettings()
        {
            Name = "Defaults";
            SigMan = CreateDefaultSignalManager();
        }

        private SignalManager CreateDefaultSignalManager()
        {
            var ch = new Channel()
            {
                Name = "Audio",
                Modality = KLib.Signals.Enumerations.Modality.Audio,
                Laterality = Laterality.Diotic,
                Location = "Site 2",
                waveform = new Sinusoid()
                {
                    Frequency_Hz = 500
                },
                modulation = new KLib.Signals.Modulations.SinusoidalAM()
                {
                    Frequency_Hz = 40,
                    Depth = 1
                },
                gate = new Gate()
                {
                    Active = true,
                    Duration_ms = 250,
                    Period_ms = 1000
                },
                level = new Level()
                {
                    Units = LevelUnits.dB_SPL,
                    Value = 50
                }
            };

            var sigMan = new SignalManager();
            sigMan.AddChannel(ch);

            return sigMan;
        }

    }
}