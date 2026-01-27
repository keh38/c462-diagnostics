using System.Collections.Generic;
using System.IO;
using System.Linq;

using ExtensionMethods;
using KLib;
using KLib.Signals.Enumerations;
    
namespace SpeechReception
{
    public class ListProperties
    {
        public string file = "";
        public Laterality laterality = Laterality.Binaural;

        public float level = 60;
        public LevelUnits units = LevelUnits.dBSPL;
        public float[] SNRs = null;

        public Masker masker = null;
        public Sequence sequence = null;
        public ClosedSet closedSet = null;
        public MatrixTest matrixTest = null;

        public bool applyOptions = false;
        public bool applyCustom = true;

        public int listIndex = -1;

        private string _testType;
        public List<ListDescription.Sentence> sentences;
        private string _title;

        public ListProperties() { }
        public ListProperties(ListProperties that)
        {
            this.file = that.file;
            this.laterality = that.laterality;
            this.level = that.level;
            this.units = that.units;
            this.SNRs = that.SNRs==null ? null : (float[]) that.SNRs.Clone();
            this.masker = that.masker;
            this.sequence = that.sequence;
            this.closedSet = that.closedSet;
            this.matrixTest = that.matrixTest;
            this.applyCustom = that.applyCustom;
            this.applyOptions = that.applyOptions;
        }

        public string Title
        {
            get { return _title; }
        }

        public string TestType
        {
            get { return _testType; }
        }

        public bool ShowFeedback
        {
            get { return closedSet != null && closedSet.feedback != ClosedSet.FeedbackType.None; }
        }

        public int GetItemCount()
        {
            var listDescription = FileIO.XmlDeserialize<ListDescription>(GetListDescriptionPath(_testType));
            return sequence.repeatsPerBlock * sequence.numBlocks * (sequence.choose > 0 ? sequence.choose : listDescription.sentences.Count);
        }

        public List<string> GetClosedSetResponses()
        {
            var listDescription = FileIO.XmlDeserialize<ListDescription>(GetListDescriptionPath(_testType));
            return listDescription.sentences.Select(o => o.whole).ToList().Unique();
        }

        public void SetSequence()
        {
            var listDescription = FileIO.XmlDeserialize<ListDescription>(GetListDescriptionPath(_testType));
            float[] snr = (masker != null) ? SNRs : new float[] { float.PositiveInfinity};
            listDescription.SetSequence(sequence, snr);
            sentences = listDescription.sentences;
        }

        public void Initialize(string testType, Laterality laterality, string option)
        {
            _testType = testType;

            UnityEngine.Debug.Log(GetListDescriptionPath(_testType));
            var listDescription = FileIO.XmlDeserialize<ListDescription>(GetListDescriptionPath(_testType));
            _title = listDescription.title;

            if (laterality != Laterality.Binaural) this.laterality = laterality;

            ApplyOption(option);

            if (sequence == null) sequence = new Sequence();
            if (SNRs == null) SNRs = new float[] { float.PositiveInfinity };
        }

        public void ApplyOption(string option)
        {
            if (string.IsNullOrEmpty(option)) return;

            string[] parts = option.Split('=');
            float value = float.Parse(parts[1]);
            switch (parts[0].Trim())
            {
                case "SNR":
                    SNRs = new float[] { value };
                    break;
            }
        }

        public bool MaskerAvailable
        {
            get
            {
                return masker != null && !string.IsNullOrEmpty(masker.Source);
            }
        }

        public bool UseMasker
        {
            get
            {
                return masker != null && !string.IsNullOrEmpty(masker.Source) && (AnyFiniteSNR || (matrixTest!=null && matrixTest.active));
            }
        }

        private bool AnyFiniteSNR
        {
            get
            {
                bool any = false;

                foreach (float v in SNRs)
                {
                    if (!float.IsInfinity(v))
                    {
                        any = true;
                        break;
                    }
                }

                return any;
            }
        }


        private string GetListDescriptionPath(string testType)
        {
            string listDescriptionPath;
            if (file.StartsWith("SpeechList"))
                listDescriptionPath = FileLocations.ConfigFile(file);
            else
                listDescriptionPath = Path.Combine(FileLocations.SpeechWavFolder, testType, "SpeechList." + file + ".xml");

            return listDescriptionPath;
        }

    }
}