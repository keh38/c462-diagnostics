﻿using System.Collections.Generic;

using Turandot.Cues;

namespace Turandot.Screen
{
    public class ScreenElements
    {
        public List<CueLayout> Cues { get; set; }
        public List<InputLayout> Inputs { get; set; }
        public bool ApplyCustomScreenColor { get; set; }
        public string finalPrompt = "";
        public bool ApplyParamSpecificScreenColor { get; set; }
        public int Color {  get; set; } 

        public ScreenElements()
        {
            Cues = new List<CueLayout>();
            Inputs = new List<InputLayout>();
            ApplyCustomScreenColor = false;
            ApplyParamSpecificScreenColor = false;
            Color = -1;
        }
    }
}