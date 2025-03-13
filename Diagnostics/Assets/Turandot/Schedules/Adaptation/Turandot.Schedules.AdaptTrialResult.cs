namespace Turandot.Schedules
{
    public class AdaptTrialResult
    {
        public bool valueChange = false;
        public bool reversal = false;
        public bool trackFinished = false;
        public bool outOfRange = false;
        public string log = "";
        public bool userStop = false;

        public AdaptTrialResult()
        {
        }
    }
}