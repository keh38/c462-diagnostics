using System.Collections.Generic;

using Turandot.Cues;

namespace Turandot.Screen
{
    public class ScreenElements
    {
        public List<CueLayout> Cues { get; set; }
        public string finalPrompt = "";

        public AvailableCues cues = new AvailableCues();
        public AvailableInputs inputs = new AvailableInputs();
        public ScreenElements()
        {
            Cues = new List<CueLayout>();
        }
    }
}