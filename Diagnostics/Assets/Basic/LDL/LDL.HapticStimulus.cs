using System.ComponentModel;

using KLib.Signals;

namespace LDL
{
    public class HapticStimulus
    {
        public bool Active { get; set; }
        public string DelayExpression { get; set; }
        public Channel Channel { get; set; }

        public HapticStimulus()
        {
            Active = false;
            DelayExpression = "";
            Channel = new Channel();
        }

        private bool ShouldSerializeActive() { return false; }
        private bool ShouldSerializeDelayExpression() { return false;  }
        private bool ShouldSerializeChannel() { return false; }
    }
}