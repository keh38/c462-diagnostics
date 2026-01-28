using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms.Design;
using ExtensionMethods;
using KLib;
using KLib.TypeConverters;
using OrderedPropertyGrid;
    
namespace SpeechReception
{
    [TypeConverter(typeof(ListPropertiesConverter))]
    public class ListProperties
    {
        [Category("List properties")]
        [PropertyOrder(0)]
        public string Name { get; set; }
        private bool ShouldSerializeName() { return false; }

        [Category("List properties")]
        [PropertyOrder(1)]
        public float Level { get; set; } 
        private bool ShouldSerializeLevel() { return false; }

        [Category("List properties")]
        [PropertyOrder(2)]
        public LevelUnits Units { get; set; } 
        private bool ShouldSerializeUnits() { return false; }

        [Category("List properties")]
        [PropertyOrder(3)]
        public string SNR { get; set; }
        private bool ShouldSerializeSNR() { return false; }

        [Category("List properties")]
        [PropertyOrder(4)]
        public Masker Masker { get; set; } 
        private bool ShouldSerializeMasker() { return false; }

        [Category("List properties")]
        [PropertyOrder(5)]
        public ClosedSet ClosedSet { get; set; } 
        private bool ShouldSerializeClosedSet() { return false; }

        [Category("List properties")]
        [PropertyOrder(6)]
        public Sequence Sequence { get; set; } 
        private bool ShouldSerializeSequence() { return false; }

        [Category("List properties")]
        [PropertyOrder(7)]
        public MatrixTest MatrixTest { get; set; }
        private bool ShouldSerializeMatrixTest() { return false; }

        public bool applyOptions = false;
        public bool applyCustom = true;

        public int listIndex = -1;

        private string _testType;
        public List<ListDescription.Sentence> sentences;
        private string _title;

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
            //this.file = that.file;
            //this.laterality = that.laterality;
            //this.level = that.level;
            //this.units = that.units;
            //this.SNRs = that.SNRs==null ? null : (float[]) that.SNRs.Clone();
            //this.masker = that.masker;
            //this.sequence = that.sequence;
            //this.closedSet = that.closedSet;
            //this.matrixTest = that.matrixTest;
            //this.applyCustom = that.applyCustom;
            //this.applyOptions = that.applyOptions;
        }

        [Browsable(false)]
        public string Title
        {
            get { return _title; }
        }

        [Browsable(false)]
        public string TestType
        {
            get { return _testType; }
        }

        [Browsable(false)]
        public bool ShowFeedback
        {
            get { return ClosedSet != null && ClosedSet.Feedback != ClosedSet.FeedbackType.None; }
        }

        public int GetItemCount()
        {
            var listDescription = FileIO.XmlDeserialize<ListDescription>(GetListDescriptionPath(_testType));
            return Sequence.RepeatsPerBlock * Sequence.NumBlocks * (Sequence.choose > 0 ? Sequence.choose : listDescription.sentences.Count);
        }

        public List<string> GetClosedSetResponses()
        {
            var listDescription = FileIO.XmlDeserialize<ListDescription>(GetListDescriptionPath(_testType));
            return listDescription.sentences.Select(o => o.whole).ToList().Unique();
        }

        public void SetSequence()
        {
            //var listDescription = FileIO.XmlDeserialize<ListDescription>(GetListDescriptionPath(_testType));
            //float[] snr = (masker != null) ? SNRs : new float[] { float.PositiveInfinity};
            //listDescription.SetSequence(sequence, snr);
            //sentences = listDescription.sentences;
        }

        public void Initialize(string testType, Laterality laterality, string option)
        {
            _testType = testType;

            UnityEngine.Debug.Log(GetListDescriptionPath(_testType));
            var listDescription = FileIO.XmlDeserialize<ListDescription>(GetListDescriptionPath(_testType));
            _title = listDescription.title;

            //if (laterality != Laterality.Binaural) this.laterality = laterality;

            ApplyOption(option);

            if (Sequence == null) Sequence = new Sequence();
            //if (SNRs == null) SNRs = new float[] { float.PositiveInfinity };
        }

        public void ApplyOption(string option)
        {
            if (string.IsNullOrEmpty(option)) return;

            string[] parts = option.Split('=');
            float value = float.Parse(parts[1]);
            //switch (parts[0].Trim())
            //{
            //    case "SNR":
            //        SNRs = new float[] { value };
            //        break;
            //}
        }

        [Browsable(false)]
        public bool MaskerAvailable
        {
            get
            {
                return Masker != null && !string.IsNullOrEmpty(Masker.Source);
            }
        }

        [Browsable(false)]
        public bool UseMasker
        {
            get
            {
                return Masker != null && !string.IsNullOrEmpty(Masker.Source) && (AnyFiniteSNR || (MatrixTest!=null && MatrixTest.active));
            }
        }

        private bool AnyFiniteSNR
        {
            get
            {
                bool any = false;

                //foreach (float v in SNRs)
                //{
                //    if (!float.IsInfinity(v))
                //    {
                //        any = true;
                //        break;
                //    }
                //}

                return any;
            }
        }


        private string GetListDescriptionPath(string testType)
        {
            string listDescriptionPath;
            if (Name.StartsWith("SpeechList"))
                listDescriptionPath = FileLocations.ConfigFile(Name);
            else
                listDescriptionPath = Path.Combine(FileLocations.SpeechWavFolder, testType, "SpeechList." + Name + ".xml");

            return listDescriptionPath;
        }

    }
}