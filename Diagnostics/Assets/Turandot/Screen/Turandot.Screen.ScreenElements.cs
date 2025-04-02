using System.Collections.Generic;

using Turandot.Cues;

namespace Turandot.Screen
{
    public class ScreenElements
    {
        public List<CueLayout> Cues { get; set; }
        public List<InputLayout> Inputs { get; set; }
        public string finalPrompt = "";

        public ScreenElements()
        {
            Cues = new List<CueLayout>();
            Inputs = new List<InputLayout>();
        }
    }
}