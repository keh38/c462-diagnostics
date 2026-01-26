namespace Digits
{
    internal class TestStatus
    {
        public int testNum = 0;
        public int blockNum = 0;
        public int trialNum = 0;
        public int dataFileNum = 0;

        override public string ToString()
        {
            return testNum + " : " + blockNum + " : " + trialNum;
        }
    }

}