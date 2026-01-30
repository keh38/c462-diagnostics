using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpeechReception
{
    public class TestPlan
    {
        public List<SpeechTest> tests = new List<SpeechTest>();

        public string name;
        public TestType testType;
        public string source;
        public int currentTestIndex = 0;
        public int currentListIndex = 0;
        public int currentSentenceIndex = 0;

        public int totalNumSentences;
        public int numSentencesDone = 0;
        public int numLists = 0;

        public TestPlan() { }

        public ListProperties GetNextList()
        {
            return tests[currentTestIndex].Lists[currentListIndex];   
        }

        public void IncrementSentence()
        {
            numSentencesDone++;
            currentSentenceIndex++;
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
        }

        public bool IsFinished()
        {
            return currentTestIndex >= tests.Count;
        }

        public int PercentComplete { get { return Mathf.RoundToInt(100f *  currentSentenceIndex / totalNumSentences); } }

    }
}