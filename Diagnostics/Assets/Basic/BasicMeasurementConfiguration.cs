//using System.Drawing.Design;
using System.Xml.Serialization;

using Newtonsoft.Json;

using C462.Shared;

namespace BasicMeasurements
{
    [XmlInclude(typeof(Audiograms.AudiogramMeasurementSettings))]
    [XmlInclude(typeof(Bekesy.BekesyMeasurementSettings))]
    [XmlInclude(typeof(DigitsTest.DigitsTestSettings))]
    [XmlInclude(typeof(LDL.LDLMeasurementSettings))]
    [XmlInclude(typeof(Questionnaires.Questionnaire))]
    [JsonObject(MemberSerialization.OptOut)]
    public class BasicMeasurementConfiguration
    {
        public string Name { get; set; }
        private bool ShouldSerializeName() { return false; }

        public bool BypassDataStreams { get; set; }
        private bool ShouldSerializeBypassDataStreams() { return false; }

        public bool UseDefaultInstructions { get; set; }
        private bool ShouldSerializeUseDefaultInstructions() { return false; }

        public int InstructionFontSize { get; set; }
        private bool ShouldSerializeInstructionFontSize() { return false; }

        //[Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string InstructionMarkdown { get; set; }
        private bool ShouldSerializeInstructionMarkdown() { return false; }

        public BasicMeasurementConfiguration()
        {
            Name = "Defaults";
            BypassDataStreams = false;

            UseDefaultInstructions = true;
            InstructionFontSize = 48;
            InstructionMarkdown = "";
        }
    }
}