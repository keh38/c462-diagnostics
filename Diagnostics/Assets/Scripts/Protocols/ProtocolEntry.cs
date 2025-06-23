using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using KLib;

namespace Protocols
{
    public class ProtocolEntry
    {
        public string Title { get; set; }
        public string Scene { get; set; }
        public string Settings { get; set; }
        public string Instructions { get; set; }
        public bool AutoAdvance { get; set; }
        public bool HideOutline { get; set; }

        public ProtocolEntry()
        {
            AutoAdvance = false;
            HideOutline = false;
        }

    }
}