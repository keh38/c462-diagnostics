using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

using OrderedPropertyGrid;

namespace SpeechReception
{
    public class Instructions
    {
        public class InstructionFile
        {
            [PropertyOrder(0)]
            [DisplayName("File name")]
            public string Name { get; set; }
            private bool ShouldSerializeName() { return false; }

            [PropertyOrder(1)]
            [DisplayName("Insert before")]
            [Description("Show these instructions before which list")]
            public int Before { get; set; }
            private bool ShouldSerializeBefore() { return false; }
        }

        public List<InstructionFile> Files { get; set; }

        public Instructions()
        {
            Files = new List<InstructionFile>();
        }

        public string Find(int listNum)
        {
            string value = null;

            var f = Files.Find(o => o.Before == listNum);
            if (f != null) value = f.Name;

            return value;
        }

    }
}