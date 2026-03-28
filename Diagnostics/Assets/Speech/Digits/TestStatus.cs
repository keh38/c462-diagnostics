using System.Collections.Generic;
using System.IO;

using UnityEngine;

using C462.Shared;

namespace Digits
{
    public class TestStatus
    {
        public string configName;
        public int testNum = 0;
        public int blockNum = 0;
        public int trialNum = 0;
        public int dataFileNum = 0;

        public List<TestSpec> plan = new List<TestSpec>();

        override public string ToString()
        {
            return testNum + " : " + blockNum + " : " + trialNum;
        }

        public int PercentComplete { get { return testNum * 100 / plan.Count; } }

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
                var savedState = KLibU.Files.XmlDeserialize<TestStatus>(StateFile);
                inProgress = savedState.configName == this.configName;
            }

            return inProgress;
        }

        public static TestStatus RestoreSavedState()
        {
            if (File.Exists(StateFile))
            {
                var savedState = KLibU.Files.XmlDeserialize<TestStatus>(StateFile);
                return savedState;
            }
            else
            {
                return null;
            }
        }

        public void Save()
        {
            KLibU.Files.XmlSerialize(this, StateFile);
        }

        private static string StateFile
        {
            get { return Path.Combine(SharedFileLocations.HtsSubjectFolder, "DigitsState.xml"); }
        }

    }

}