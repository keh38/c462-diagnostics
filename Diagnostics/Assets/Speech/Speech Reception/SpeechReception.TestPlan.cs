using System.Collections.Generic;

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

        public TestPlan() { }

        public ListProperties GetNextList()
        {
            return tests[currentTestIndex].Lists[currentListIndex];   
        }
    }
}