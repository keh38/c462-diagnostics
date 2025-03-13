using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Turandot.Screen
{
    public class AvailableCues : AvailableElements
    {
        [JsonIgnore]
        [XmlIgnore]
        override public List<string> ValidValues
        {
            get
            {
                return new List<string>(new string[]
                {
                "Counter",
                "Fixation point",
                "Image",
                "Help",
                "LED",
                "Message",
                "Progress bar",
                "Scoreboard"
                }
                );
            }
        }
    }
}