using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using KLib;

namespace Protocols
{
    public class Appearance
    {
        public int ListFontSize { get; set; }
        public int InstructionFontSize { get; set; }
        public Appearance()
        {
            ListFontSize = 60;
            InstructionFontSize = 48;
        }

    }
}