using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Turandot.Screen
{
    public class AvailableInputs : AvailableElements
    {
        [JsonIgnore]
        [XmlIgnore]
        override public List<string> ValidValues
        {
            get
            {
                return new List<string>(new string[]
                {
                "Categorizer",
                "Grapher",
                "Keypad",
                "Param slider",
                "Pupillometer",
                "Random process",
                "SAM",
                "Scaler",
                "Thumb slider",
                "Xbox"
                }
                );
            }
        }
    }
}