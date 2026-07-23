using Newtonsoft.Json;

using KLib.Logging;

namespace Tapping
{
    [JsonObject(MemberSerialization.OptOut)]
    public class TappingTrialData
    {
        public TappingTrial trial;
        public AudioDspEventLog pacerLog;
        public AudioDspEventLog distractorLog;

        public TappingTrialData() { }
        public TappingTrialData(TappingTrial trial, AudioDspEventLog pacerLog, AudioDspEventLog distractorLog)
        {
            this.trial = trial;
            this.pacerLog = pacerLog;
            this.distractorLog = distractorLog;
        }
    }
}