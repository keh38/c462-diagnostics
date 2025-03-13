namespace Turandot.Schedules
{
    public class AdaptHistory
    {
        public float value;
        public string response = "";
        public string log = "";
        public bool reversal = false;
        public float catchValue = float.NaN;

        public AdaptHistory()
        {
        }

        public AdaptHistory(float value, string response)
        {
            this.value = value;
            this.response = response;
        }
    }
}