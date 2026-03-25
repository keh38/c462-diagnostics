using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms.Design;
using System.Xml.Serialization;
using ExtensionMethods;
using KLibU;
using KLib;
using KLib.Expressions;
using KLib.Signals.Calibration;
using Newtonsoft.Json;
using Unity.VisualScripting;

namespace SpeechReception
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ListProperties
    {
        private static TestType _serializationTestType = TestType.OpenSet;
        public static TestType SerializationTestType
        {
            get { return _serializationTestType; }
            set 
            {
                _serializationTestType = value;
                //ListPropertiesConverter.TestType = value; 
            }
        }

        public string Name { get; set; }
        private bool ShouldSerializeName() { return false; }

        public float Level { get; set; } 
        public bool ShouldSerializeLevel() { return _serializationTestType != TestType.QuickSIN; }

        public LevelUnits Units { get; set; }
        public bool ShouldSerializeUnits() { return _serializationTestType != TestType.QuickSIN; }

        public string SNR { get; set; }
        public bool ShouldSerializeSNR() { return _serializationTestType != TestType.QuickSIN; }

        public Masker Masker { get; set; } 
        public bool ShouldSerializeMasker() { return _serializationTestType != TestType.QuickSIN; }

        public ClosedSet ClosedSet { get; set; } 
        public bool ShouldSerializeClosedSet() { return _serializationTestType == TestType.ClosedSet; }

        public Sequence Sequence { get; set; }
        public bool ShouldSerializeSequence() { return _serializationTestType == TestType.ClosedSet; }

        public MatrixTest MatrixTest { get; set; }
        public bool ShouldSerializeMatrixTest() { return _serializationTestType == TestType.Matrix; }

        public List<ListDescription.Sentence> sentences;
        public bool ShouldSerializeSentences() { return _serializeAllFields; }

        public TestType TestType { get; set; }
        public bool ShouldSerializeTestType() { return _serializeAllFields; }

        public string TestSource { get; set; }
        public bool ShouldSerializeTestSource() { return _serializeAllFields; }

        public TestEar TestEar { get; set; }
        public bool ShouldSerializeTestEar() { return _serializeAllFields; }

        public string Title { get; set; }
        public bool ShouldSerializeTitle() { return _serializeAllFields; }

        [XmlIgnore]
        public int listIndex = -1;

        [XmlIgnore]
        public bool ShowFeedback { get { return ClosedSet != null && ClosedSet.Feedback != ClosedSet.FeedbackType.None; } }

        [XmlIgnore]
        public bool MaskerAvailable { get { return Masker != null && !string.IsNullOrEmpty(Masker.Source); } }

        [XmlIgnore]
        public bool UseMasker { get { return Masker != null && !string.IsNullOrEmpty(Masker.Source) && (AnyFiniteSNR || (MatrixTest != null && MatrixTest.Active)); } }

        [XmlIgnore]
        public bool AnyFiniteSNR { get; private set; }

        private bool _serializeAllFields = false;

        public ListProperties()
        {
            Name = "";
            Level = 60;
            Units = LevelUnits.dBSPL;
            SNR = "";
            Masker = new Masker();
            ClosedSet = new ClosedSet();
            Sequence = new Sequence();
            MatrixTest = new MatrixTest();
        }

        public ListProperties(ListProperties that)
        {
            this.Name = that.Name;
            this.Level = that.Level;
            this.Units = that.Units;
            this.SNR = that.SNR;
            this.Masker = that.Masker;
            this.ClosedSet = that.ClosedSet;
            this.Sequence = that.Sequence;
            this.MatrixTest = that.MatrixTest;
        }

        public List<string> GetClosedSetResponses()
        {
            var listDescription = Files.XmlDeserialize<ListDescription>(GetListDescriptionPath(TestSource));
            return listDescription.sentences.Select(o => o.whole).ToList().Unique();
        }

        public void SerializeAllFields(bool serialize)
        {
            _serializeAllFields = serialize;
        }

        public void ApplySequence()
        {
            var listDescription = Files.XmlDeserialize<ListDescription>(GetListDescriptionPath(TestSource));
            float[] snr = null;

            AnyFiniteSNR = false;
            
            if (TestType != TestType.QuickSIN)
            {
                if (Masker == null)
                {
                    snr = new float[] { float.PositiveInfinity };
                }
                else
                {
                    snr = Expressions.Evaluate(SNR);
                    AnyFiniteSNR = snr.Any(o => !float.IsInfinity(o));
                }
                listDescription.SetSequence(Sequence, snr);
            }

            sentences = listDescription.sentences;
        }

        public void Initialize(TestType testType, string testSource, TestEar testEar)
        {
            TestType = testType;
            TestSource = testSource;
            TestEar = testEar;

            var listDescription = Files.XmlDeserialize<ListDescription>(GetListDescriptionPath(TestSource));
            Title = listDescription.title;

            AnyFiniteSNR = false;
        }

        public int GetItemCount()
        {
            var listDescription = Files.XmlDeserialize<ListDescription>(GetListDescriptionPath(TestSource));
            return Sequence.RepeatsPerBlock * Sequence.NumBlocks * (Sequence.choose > 0 ? Sequence.choose : listDescription.sentences.Count);
        }

        private string GetListDescriptionPath(string testSource)
        {
            string fileName = $"SpeechList.{Name}.xml";

            // Check first for project-specific list
            string listDescriptionPath = Path.Combine(FileLocations.LocalResourceFolder("ConfigFiles"), fileName);

            if (!File.Exists(listDescriptionPath))
            {
                // otherwise use the default list installed with the speech test material
                listDescriptionPath = Path.Combine(FileLocations.SpeechWavFolder, testSource, fileName);
            }

            return listDescriptionPath;
        }

    }
}