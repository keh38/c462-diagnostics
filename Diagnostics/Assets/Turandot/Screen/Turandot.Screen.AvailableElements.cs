using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;


namespace Turandot.Screen
{
    public class AvailableElements
    {
        public List<string> elements = new List<string>();

		[JsonIgnore]
		[XmlIgnore]
        public virtual List<string> ValidValues { get { return null; } }
    }
}