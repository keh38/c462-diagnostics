using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using KLib;

using OrderedPropertyGrid;

namespace SpeechReception
{
    public enum Laterality { Left, Right, Binaural}

    public class SpeechTest
    {
        [Category("About")]
        [PropertyOrder(0)]
        [DisplayName("Type")]
        public string TestType { get; set; }
        private bool ShouldSerializeTestType() { return false; }

        [Category("About")]
        [PropertyOrder(1)]
        [DisplayName("Name")]
        public string TestName { get; set; }
        private bool ShouldSerializeTestName() { return false; }

        [Category("Instructions")]
        [PropertyOrder(0)]
        public int InstructionFontSize { get; set; }
        private bool ShouldSerializeInstructionFontSize() { return false; }

        [Category("Instructions")]
        [PropertyOrder(1)]
        public Instructions Instructions { get; set; }
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
        [PropertyOrder(6)]
        public int NumToReview { get; set; }
        private bool ShouldSerializeNumToReview() { return false; }

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
        public string TestEars { get; set; }
        private bool ShouldSerializeTestEars() { return false; }

        [Category("Sequence")]
        [PropertyOrder(11)]
        public string EarOrder { get; set; }
        private bool ShouldSerializeEarOrder() { return false; }

        [Category("Sequence")]
        [PropertyOrder(12)]
        public Laterality laterality
        {
            get { return _laterality; }
            set { _laterality = value; }
        }
        private bool ShouldSerializelaterality() { return false; }

        [Browsable(false)]
        public string MenuOptions { get; set; }
        private bool ShouldSerializeMenuOptions() { return false; }

        [Category("Lists")]
        [PropertyOrder(14)]
        [Description("Total number of lists available")]
        public int NumLists { get; set; }
        private bool ShouldSerializeNumLists() { return false; }

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
        public List<ListProperties> Lists { get; set; }
        private bool ShouldSerializeLists() { return false; }

        private Laterality _laterality = Laterality.Binaural;
        private string _option = "";

        private bool _isQuiet = true;
        private int _numSentences = 0;

        public SpeechTest()
        {
            TestType = null;
            TestName = "untitled";

            Instructions = null;
            InstructionFontSize = 60;

            MinDelay_s = 0f;
            MaxDelay_s = 0f;
            SentenceDuration_s = 0f;
            MaskerTail_s = 0f;
            NumToReview = 3;
            GiveBreakEvery = -1;
            NumPracticeLists = 0;
            NumPerSNR = 0;
            TestEars = "";
            EarOrder = "Random";
            // laterality uses backing field _laterality which already defaults to Both
            MenuOptions = null;
            NumLists = 1;
            Choose = 0;
            Exclude = "";
            UseAudioCues = false;

            Lists = new List<ListProperties>();
        }

        public static SpeechTest Deserialize(string fileSpec, UserCustomizations customizations)
        {
            string configFile = fileSpec;
            string option = "";

            string[] parts = fileSpec.Split('.');
            if (parts.Length == 3)
            {
                configFile = parts[0] + "." + parts[1];
                option = parts[2];
            }

            SpeechTest srt = FileIO.XmlDeserialize<SpeechTest>(FileLocations.ConfigFile(configFile));

            var c = customizations.Get(srt.TestType);
            srt.Initialize(option, c);

            return srt;
        }

        public string Option
        {
            get { return _option; }
        }

        public int NumSentences
        {
            get { return _numSentences; }
        }

        public List<Laterality> Ears
        {
            get
            {
                List<Laterality> ears = new List<Laterality>();
                List<string> names = new List<string>(Enum.GetNames(typeof(Laterality)));
                if (!string.IsNullOrEmpty(TestEars))
                {
                    foreach(string e in TestEars.Split(','))
                    {
                        int idx = names.FindIndex(o => o == e.Trim());
                        if (idx >= 0) ears.Add((Laterality)idx);
                    }
                }
                return ears;
            }
        }

        private void Initialize(string option, UserCustomization customization)
        {
            // option: expression passed in from menu
            _option = option;

            _numSentences = 0;

            int numSNR = 0;
            if (customization != null)
            {
                numSNR = ApplyCustomization(customization);
            }

            // A nonzero value of "Choose" is the indication to choose one or more lists at random
            if (Choose > 0)
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
                listProps.Initialize(TestType, laterality, option);
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
                lp.level = customization.level;

                if (lp.MaskerAvailable)
                {
                    foreach (var snr in customization.snr)
                    {
                        var maskedList = new ListProperties(lp);
                        maskedList.SNRs = new float[] { snr };
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
                history.Order = KMath.Permute(NumLists);
                history.LastCompleted = -1;
                FileIO.XmlSerialize(history, historyFile);
            }

            if (excluded.Length > 0)
            {
                history.Order = KMath.SetDiff(history.Order, excluded);
                FileIO.XmlSerialize(history, historyFile);
                NumLists = history.Order.Length;
                UnityEngine.Debug.Log("excluded " + KLib.Expressions.ToVectorString(excluded) + " from " + TestType + " history");
            }

            List<ListProperties> newListProps = new List<ListProperties>();

            int listIndex = history.LastCompleted + 1;
            if (listIndex >= NumLists) listIndex = 0;

            for (int k = 0; k < NumPracticeLists; k++)
            {
                var lp = new ListProperties(Lists[0]);

                lp.listIndex = listIndex++;
                if (listIndex >= NumLists) listIndex = 0;

                int listNum = history.Order[lp.listIndex];
                lp.file = String.Format(lp.file, listNum + 1);

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
                    if (listIndex == NumLists) listIndex = 0;

                    int listNum = history.Order[lp.listIndex];
                    lp.file = String.Format(lp.file, listNum + 1);

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
                history.Order = KMath.Permute(NumLists);
                history.LastCompleted = -1;
                FileIO.XmlSerialize(history, historyFile);
            }

            listIndex = history.LastCompleted + 1;
            if (listIndex >= NumLists) listIndex = 0;

            int listNum = history.Order[listIndex];

            return listNum;
        }

        public float GetReference(string transducer, SpeechReception.LevelUnits units)
        {
            References r = new References(Path.Combine(FileLocations.SpeechWavFolder, TestType), TestType);
            return r.GetReference(transducer, units.ToString());
        }

    }
}