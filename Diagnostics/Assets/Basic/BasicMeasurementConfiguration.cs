using System.ComponentModel;
using System.Xml.Serialization;

[XmlInclude(typeof(Audiograms.AudiogramMeasurementSettings))]
[XmlInclude(typeof(LDL.LDLMeasurementSettings))]
public class BasicMeasurementConfiguration
{
    [Category("Config")]
    [Description("This sets the filename")]
    public string Name { get; set; }
    private bool ShouldSerializeName() {  return false; }
}