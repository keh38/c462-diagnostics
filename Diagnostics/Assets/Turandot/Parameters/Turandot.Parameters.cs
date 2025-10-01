using UnityEngine;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using Turandot.Screen;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Parameters
    {
        public int version = _currentVersion;
        public Instructions instructions = new Instructions();
        public ScreenElements screen = new ScreenElements();
        public List<InputEvent> inputEvents = new List<InputEvent>();
        public List<FlowElement> flowChart = new List<FlowElement>();
        public string firstState = "";
        public List<Flag> flags = new List<Flag>();
        public Schedules.Schedule schedule = new Schedules.Schedule();
        public Schedules.Adaptation adapt = new Schedules.Adaptation();
        public string tag = "";
        public string wavFolder = "";
        public TrialLogOption trialLogOption = TrialLogOption.Upload;
        public bool allowExpertOptions = false;
        public string matlabFunction = "";

        private static int _currentVersion = 5;

        public Parameters()
        { 
        }

        public FlowElement this[string name]
        {
            get { return flowChart.Find(fe => fe.name == name); }
        }

        public void CreateFlowElements(params string[] names)
        {
            firstState = names[0];
            foreach (string n in names)
            {
                flowChart.Add(new FlowElement(n));
            }
        }

        public InputEvent AddInputEvent(string name)
        {
            InputEvent ie = new InputEvent(name);
            inputEvents.Add(ie);
            return ie;
        }

        public string SetParameter(string paramName, float value)
        {
            string error = "";

            string[] s = paramName.Split(new char[] { '.' }, 3);

            if (s.Length != 3)
            {
                error = "Invalid parameter format: " + paramName;
                return error;
            }

            string state = s[0];
            string chanName = s[1];
            string remainder = s[2];

            if (state == "{Flag}")
            {
                flags.Find(f => f.name == remainder).value = (int)value;
            }
            else
            {
                FlowElement fe = flowChart.Find(e => e.name == state);
                if (fe == null)
                {
                    error = "State not found: " + state;
                }
                else
                {
                    if (chanName == "---")
                    {
                        fe.SetParameter(remainder, value);
                    }
                    else if (chanName == "Cues")
                    {
                        error = fe.SetCueProperty(remainder, value);
                    }
                    else
                    {
                        error = fe.sigMan.SetParameter(chanName, remainder, value);
                    }
                }
            }
            return error;
        }

        public List<string> GetPostTrialProperties(List<string> existingProps)
        {
            List<string> props = new List<string>();

            foreach (string s in existingProps)
            {
                string[] parts = s.Split('=');
                string[] names = parts[0].Split('.');

                FlowElement fe = flowChart.Find(o => o.name == names[0]);
                if (fe != null && fe.sigMan != null && names.Length > 1)
                {
                    KLib.Signals.Channel ch = fe.sigMan[names[1]];
                    if (ch != null && names.Length > 2)
                    {
                        //if (names[2] == "Level" && ch.level.Units == KLib.Signals.Enumerations.LevelUnits.pctDR)
                        //{
                        //    props.Add(parts[0] + "_dBSPL=" + ch.ConvertSLToSPL);
                        //}
                    }
                }
            }

            return props;
        }

        public void ClearExpertOptions()
        {
            foreach (var fe in flowChart)
            {
                if (fe.sigMan != null) fe.sigMan.ClearExpertOptions();
                foreach (var t in fe.term) t.flagExpr = "";
            }
        }

        public void Initialize()
        {
            foreach (var fe in flowChart.FindAll(x => x.isAction && !string.IsNullOrEmpty(x.actionFamily)))
            {
                var fam = schedule.families.Find(x => x.name == fe.actionFamily);
                if (fam == null)
                    throw new System.Exception("Action '" + fe.name + "': Action family '" + fe.actionFamily + "' not found");

                fe.InitializeSCL(fam, schedule.numBlocks);
                fe.AdvanceSequence();
                schedule.families.Remove(fam);
            }

            if (schedule.families.Count == 0)
            {
                //schedule.numBlocks = 1;
            }
        }

        public void ApplyDefaultWavFolder(string project)
        {
            foreach (FlowElement f in flowChart)
            {
                if (f.sigMan != null)
                {
                    if (string.IsNullOrEmpty(wavFolder))
                    {
                        f.sigMan.WavFolder = FileLocations.LocalResourceFolder(project, "Wav Files");
                    }
                    else
                    {
                        f.sigMan.WavFolder = System.IO.Path.Combine(FileLocations.UserWavFolder, wavFolder);
                    }
                }
            }
        }

        public void OrFlag(string flag, int value)
        {
            var f = flags.Find(o => o.name.Equals(flag));
            if (f != null)
            {
                f.value |= value;
            }
        }

        public void SetFlag(string flag, int value)
        {
            var f = flags.Find(o => o.name.Equals(flag));
            if (f != null)
            {
                f.value = value;
            }

            //Debug.Log($"{flag} = {value}");
        }

        public void IncrementFlag(string flag)
        {
            var f = flags.Find(o => o.name.Equals(flag));
            if (f != null)
            {
                f.value++;
                //Debug.Log($"{flag} = {f.value}");
            }

        }

    }
}
