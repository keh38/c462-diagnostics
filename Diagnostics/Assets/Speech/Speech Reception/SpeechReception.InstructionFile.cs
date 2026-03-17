using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

using OrderedPropertyGrid;
using KLib.TypeConverters;
using Newtonsoft.Json;

namespace SpeechReception
{
    [TypeConverter(typeof(SortableTypeConverter))]
    [JsonObject(MemberSerialization.OptOut)]
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

}