using System;
using System.IO;

using C462.Shared;

namespace Tapping
{
    public class TappingMeasurementState
    {
        public DateTime startTime;
        public string settingsFileName;
        public string trialListFileName;
        public int trialIndex;

        public TappingMeasurementState()
        {
            settingsFileName = "";
            trialListFileName = "";
            trialIndex = 0;
        }

        public void Advance()
        {
            trialIndex++;
            SaveState();
        }

        private void SaveState()
        {
            string path = SettingsFileName;
            KLibU.Files.XmlSerialize(this, path);
        }

        public static TappingMeasurementState LoadState()
        {
            if (File.Exists(SettingsFileName))
            {
                return KLibU.Files.XmlDeserialize<TappingMeasurementState>(SettingsFileName);
            }
            else
            {
                return new TappingMeasurementState();
            }
        }

        public bool CanResume(string settingsFile, string trialListFile)
        {
            return (settingsFileName == settingsFile) && (trialListFileName == trialListFile);
        }

        public void Initialize(string settingsFile, string trialListFile, int trialIndex)
        {
            startTime = DateTime.Now;
            settingsFileName = settingsFile;
            trialListFileName = trialListFile;
            this.trialIndex = trialIndex;
            SaveState();
        }

        public void ClearState()
        {
            if (File.Exists(SettingsFileName))
            {
                File.Delete(SettingsFileName);
            }
        }

        private static string SettingsFileName => Path.Combine(SharedFileLocations.HtsSubjectFolder, "TappingMeasurementState.xml");

    }
}