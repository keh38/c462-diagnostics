using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace SpeechReception
{
    public class TestPlan
    {
        public string name;
        public TestType testType;
        public string source;
        public int currentTestIndex = 0;
        public int currentListIndex = 0;
        public int currentSentenceIndex = 0;

        public int totalNumSentences;
        public int numSentencesDone = 0;

        public List<SpeechTest> tests = new List<SpeechTest>();

        [XmlIgnore]
        public int PercentComplete { get { return Mathf.RoundToInt(100f * currentSentenceIndex / totalNumSentences); } }
        [XmlIgnore]
        public bool IsFinished { get { return currentTestIndex >= tests.Count; } }

        public TestPlan() { }

        public ListProperties GetNextList()
        {
            return tests[currentTestIndex].Lists[currentListIndex];   
        }

        public void Save()
        {
            foreach (var t in tests) t.MakeListsSerializable(true);
            KLib.FileIO.XmlSerialize(this, StateFile);
        }

        public void Finish()
        {
            if (File.Exists(StateFile))
                File.Delete(StateFile); 
        }

        public bool IsRunInProgress()
        {
            bool inProgress = false;

            if (File.Exists(StateFile))
            {
                var savedState = KLib.FileIO.XmlDeserialize<TestPlan>(StateFile);
                inProgress = 
                    !savedState.IsFinished &&
                    savedState.name == this.name &&
                    savedState.testType == this.testType &&
                    savedState.source == this.source;
            }

            return inProgress;
        }

        public static TestPlan RestoreSavedState()
        {
            if (File.Exists(StateFile))
            {
                var savedState = KLib.FileIO.XmlDeserialize<TestPlan>(StateFile);

                return savedState;
            }
            else
            {
                return null;
            }
        }

        public ListProperties GetCurrentList()
        {
            return tests[currentTestIndex].Lists[currentListIndex];
        }

        public void IncrementSentence()
        {
            numSentencesDone++;
            currentSentenceIndex++;
            Save();
        }

        public void IncrementList()
        {
            currentListIndex++;
            currentSentenceIndex = 0;

            if (currentListIndex == tests[currentTestIndex].Lists.Count)
            {
                currentTestIndex++;
                currentListIndex = 0;
            }
            Save();
        }

        private static string StateFile
        {
            get { return Path.Combine(Application.persistentDataPath, "SpeechState.xml"); }
        }


    }
}