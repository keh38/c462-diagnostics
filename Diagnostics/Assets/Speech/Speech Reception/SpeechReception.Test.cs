using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using KLib;
using KLib.TypeConverters;
using OrderedPropertyGrid;

namespace SpeechReception
{
    public enum TestEar { SubjectDefault, Left, Right, Binaural}

    public enum TestType
    {
        QuickSIN,
        OpenSet,
        ClosedSet,
        Matrix
    }

    [TypeConverter(typeof(SpeechTestConverter))]
    public class SpeechTest
    {
        [Category("About")]
        [PropertyOrder(0)]
        [DisplayName("Name")]
        public string TestName { get; set; }
        private bool ShouldSerializeTestName() { return false; }

        [Category("About")]
        [PropertyOrder(1)]
        [DisplayName("Type")]
        public TestType TestType { get; set; }
        private bool ShouldSerializeTestType() { return false; }

        [Category("About")]
        [PropertyOrder(2)]
        public string TestSource { get; set; }
        private bool ShouldSerializeTestSource() { return false; }

        [Category("Instructions")]
        [DisplayName("Font size")]
        [PropertyOrder(0)]
        public int InstructionFontSize { get; set; }
        private bool ShouldSerializeInstructionFontSize() { return false; }

        [Category("Instructions")]
        [PropertyOrder(1)]
        [TypeConverter(typeof(InstructionFileCollectionConverter))]
        public List<InstructionFile> Instructions { get; set; }
        private bool ShouldSerializeInstructions() { return false; }

        [Category("Timing")]
        [PropertyOrder(2)]
        [DisplayName("Min delay")]
        public float MinDelay_s { get; set; }
        private bool ShouldSerializeMinDelay_s() { return false; }

        [Category("Timing")]
        [PropertyOrder(3)]
        [DisplayName("Max delay")]
        public float MaxDelay_s { get; set; }
        private bool ShouldSerializeMaxDelay_s() { return false; }

        [Category("Timing")]
        [PropertyOrder(4)]
        [DisplayName("Duration")]
        public float SentenceDuration_s { get; set; }
        private bool ShouldSerializeSentenceDuration_s() { return false; }

        [Category("Timing")]
        [PropertyOrder(5)]
        [DisplayName("Masker tail")]
        public float MaskerTail_s { get; set; }
        private bool ShouldSerializeMaskerTail_s() { return false; }

        [Category("Procedure")]
        [PropertyOrder(0)]
        public bool BypassDataStreams { get; set; }
        private bool ShouldSerializeBypassDataStreams() { return false; }

        [Category("Procedure")]
        [PropertyOrder(6)]
        [Description("Number of recordings to have subject review")]
        public int NumToReview { get; set; }
        public bool ShouldSerializeNumToReview() { return TestType == TestType.OpenSet || TestType == TestType.QuickSIN; }

        [Category("Procedure")]
        [PropertyOrder(7)]
        public int GiveBreakEvery { get; set; }
        private bool ShouldSerializeGiveBreakEvery() { return false; }

        [Category("Procedure")]
        [PropertyOrder(17)]
        public bool UseAudioCues { get; set; }
        private bool ShouldSerializeUseAudioCues() { return false; }

        [Category("Sequence")]
        [PropertyOrder(8)]
        public int NumPracticeLists { get; set; }
        private bool ShouldSerializeNumPracticeLists() { return false; }

        [Category("Sequence")]
        [PropertyOrder(9)]
        public int NumPerSNR { get; set; }
        private bool ShouldSerializeNumPerSNR() { return false; }

        [Category("Sequence")]
        [PropertyOrder(10)]
        public List<TestEar> TestEars { get; set; }
        private bool ShouldSerializeTestEars() { return false; }

        [Category("Sequence")]
        [PropertyOrder(11)]
        public string EarOrder { get; set; }
        private bool ShouldSerializeEarOrder() { return false; }

        [Category("Lists")]
        [PropertyOrder(14)]
        [Description("Total number of lists available")]
        public int NumListsAvailable { get; set; }
        private bool ShouldSerializeNumListsAvailable() { return false; }

        [Category("Lists")]
        [PropertyOrder(15)]
        [Description("Number of lists to choose at random")]
        public int Choose { get; set; }
        private bool ShouldSerializeChoose() { return false; }

        [Category("Lists")]
        [PropertyOrder(16)]
        [Description("Lists to exclude from random selection")]
        public string Exclude { get; set; }
        private bool ShouldSerializeExclude() { return false; }

        [Category("Lists")]
        [PropertyOrder(19)]
        [TypeConverter(typeof(ListPropertiesCollectionConverter))]
        public List<ListProperties> Lists { get; set; }
        private bool ShouldSerializeLists() { return false; }

        private bool _isQuiet = true;
        private int _numSentences = 0;

        public SpeechTest()
        {
            TestName = "untitled";
            TestType = TestType.OpenSet;
            TestSource = "Unknown";

            Instructions = new List<InstructionFile>();
            InstructionFontSize = 60;

            MinDelay_s = 0f;
            MaxDelay_s = 0f;
            SentenceDuration_s = 0f;
            MaskerTail_s = 0f;
            NumToReview = 3;
            GiveBreakEvery = -1;
            NumPracticeLists = 0;
            NumPerSNR = 0;
            TestEars = new List<TestEar>() { TestEar.Binaural };
            EarOrder = "Random";
            NumListsAvailable = 1;
            Choose = 0;
            Exclude = "";
            UseAudioCues = false;

            Lists = new List<ListProperties>();
        }

        [XmlIgnore]
        [Browsable(false)]
        public bool UseMicrophone { get { return TestType == TestType.QuickSIN || TestType == TestType.OpenSet; } }

        [XmlIgnore]
        [Browsable(false)]
        public bool ReviewResponses { get { return UseMicrophone && NumToReview > 0; } }

        [XmlIgnore]
        [Browsable(false)]
        public int NumSentences { get { return _numSentences; } }

        public void Initialize(TestEar testEar, UserCustomization customization)
        {
            _numSentences = 0;

            int numSNR = 0;
            if (customization != null)
            {
                numSNR = ApplyCustomization(customization);
            }

            // A nonzero value of "Choose" is the indication to choose one or more lists at random
            if (NumListsAvailable > 1 && Choose > 0)
            {
                UnityEngine.Debug.Log("Choose = " + Choose);
                UnityEngine.Debug.Log("NumPerSNR = " + NumPerSNR);
                if (NumPerSNR > 0)
                {
                    Choose = NumPracticeLists + NumPerSNR * numSNR;
                }
                UnityEngine.Debug.Log("Choose = " + Choose);

                SelectRandomLists();
            }

            foreach (ListProperties listProps in Lists)
            {
                listProps.Initialize(TestType, TestSource, testEar);
                _numSentences += listProps.GetItemCount();
            }
        }

        private int ApplyCustomization(UserCustomization customization)
        {
            int numSNR = 0;
            var newListProps = new List<ListProperties>();

            foreach (var list in Lists)
            {
                var lp = new ListProperties(list);
                lp.Level = customization.level;

                if (lp.MaskerAvailable)
                {
                    foreach (var snr in customization.snr)
                    {
                        var maskedList = new ListProperties(lp);
                        maskedList.SNR = snr.ToString();
                        newListProps.Add(maskedList);
                        numSNR++;
                    }
                }
                else
                {
                    newListProps.Add(lp);
                }
            }

            Lists = newListProps;
            return numSNR;
        }

        private void SelectRandomLists()
        {
            ListHistory history = null;

            var excluded = KLib.Expressions.EvaluateToInt(Exclude);

            string historyFile = Path.Combine(FileLocations.SubjectMetaFolder, TestType + "_History.xml");
            if (File.Exists(historyFile))
                history = FileIO.XmlDeserialize<ListHistory>(historyFile);
            else
            {
                history = new ListHistory();
                history.Order = KMath.Permute(NumListsAvailable);
                history.LastCompleted = -1;
                FileIO.XmlSerialize(history, historyFile);
            }

            if (excluded.Length > 0)
            {
                history.Order = KMath.SetDiff(history.Order, excluded);
                FileIO.XmlSerialize(history, historyFile);
                NumListsAvailable = history.Order.Length;
                UnityEngine.Debug.Log("excluded " + KLib.Expressions.ToVectorString(excluded) + " from " + TestType + " history");
            }

            List<ListProperties> newListProps = new List<ListProperties>();

            int listIndex = history.LastCompleted + 1;
            if (listIndex >= NumListsAvailable) listIndex = 0;

            for (int k = 0; k < NumPracticeLists; k++)
            {
                var lp = new ListProperties(Lists[0]);

                lp.listIndex = listIndex++;
                if (listIndex >= NumListsAvailable) listIndex = 0;

                int listNum = history.Order[lp.listIndex];
                lp.Name = String.Format(lp.Name, listNum + 1);

                newListProps.Add(lp);
            }

            int firstMasked = NumPracticeLists > 0 ? 1 : 0;
            int nmasked = Lists.Count - firstMasked;
            int nperList = UnityEngine.Mathf.FloorToInt((float)(Choose - NumPracticeLists) / nmasked);

            for (int klist = 0; klist < nmasked; klist++)
            {
                for (int k = 0; k < nperList; k++)
                {
                    var lp = new ListProperties(Lists[firstMasked + klist]);

                    lp.listIndex = listIndex++;
                    if (listIndex == NumListsAvailable) listIndex = 0;

                    int listNum = history.Order[lp.listIndex];
                    lp.Name = String.Format(lp.Name, listNum + 1);

                    newListProps.Add(lp);
                }
            }
            Lists = newListProps;
        }

        public int SelectRandomList(out int listIndex)
        {
            ListHistory history = null;

            string historyFile = Path.Combine(FileLocations.SubjectMetaFolder, TestType + "_History.xml");
            if (File.Exists(historyFile))
                history = FileIO.XmlDeserialize<ListHistory>(historyFile);
            else
            {
                history = new ListHistory();
                history.Order = KMath.Permute(NumListsAvailable);
                history.LastCompleted = -1;
                FileIO.XmlSerialize(history, historyFile);
            }

            listIndex = history.LastCompleted + 1;
            if (listIndex >= NumListsAvailable) listIndex = 0;

            int listNum = history.Order[listIndex];

            return listNum;
        }

        public float GetReference(string transducer, SpeechReception.LevelUnits units)
        {
            References references = null;

            string refPath = Path.Combine(FileLocations.LocalResourceFolder("Calibration"), TestSource + "_References.xml");
            if (File.Exists(refPath))
            {
                var r = new References(refPath);
                if (r.HasReferenceFor(transducer, units.ToString()))
                {
                    references = r;
                }
            }

            if (references == null)
            {
                refPath = Path.Combine(FileLocations.SpeechWavFolder, TestSource, TestSource + "_References.xml");
                references = new References(refPath);
            }

            return references.GetReference(transducer, units.ToString());
        }

        /// <summary>
        /// Creates a deep copy of this SpeechTest instance.
        /// </summary>
        public SpeechTest Clone()
        {
            var clone = new SpeechTest
            {
                TestName = this.TestName,
                TestType = this.TestType,
                TestSource = this.TestSource,
                InstructionFontSize = this.InstructionFontSize,
                Instructions = this.Instructions,
                MinDelay_s = this.MinDelay_s,
                MaxDelay_s = this.MaxDelay_s,
                SentenceDuration_s = this.SentenceDuration_s,
                MaskerTail_s = this.MaskerTail_s,
                BypassDataStreams = this.BypassDataStreams,
                NumToReview = this.NumToReview,
                GiveBreakEvery = this.GiveBreakEvery,
                UseAudioCues = this.UseAudioCues,
                NumPracticeLists = this.NumPracticeLists,
                NumPerSNR = this.NumPerSNR,
                TestEars = this.TestEars != null
                    ? new List<TestEar>(this.TestEars)
                    : null,
                EarOrder = this.EarOrder,
                NumListsAvailable = this.NumListsAvailable,
                Choose = this.Choose,
                Exclude = this.Exclude,
                Lists = this.Lists != null
                    ? new List<ListProperties>(this.Lists.Count)
                    : null
            };

            // Deep copy Lists
            if (this.Lists != null)
            {
                foreach (var list in this.Lists)
                {
                    clone.Lists.Add(new ListProperties(list));
                }
            }

            return clone;
        }
    }
}