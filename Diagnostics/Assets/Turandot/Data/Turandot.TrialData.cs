using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class TrialData
    {
        public int block = -1;
        public int track = -1;
        public int trial = -1;
        public string result = "";
        public float reactionTime = float.NaN;
        public string family = "";
        public Schedules.TrialType type = Schedules.TrialType.GoNoGo;
        public List<string> properties = new List<string>();

        public TrialData()
        {
        }

        public void NewTrial(int block, int trial, string family)
        {
            this.block = block;
            this.trial = trial;
            this.family = family;
            properties.Clear();
            result = "";
        }

        public void NewTrial(int block, int track, int trial, string family)
        {
            NewTrial(block, trial, family);
            this.track = track;
        }

        public string ToJSONString()
        {
            string json = "{\"block\":" + block + ",";
            if (track > -1) json += "\"track\":" + track + ",";
            json += "\"trial\":" + trial + ",";
            switch (type)
            {
                case Schedules.TrialType.GoNoGo:
                    json += "\"type\":\"GoNoGo\",";
                    break;
                case Schedules.TrialType.CSplus:
                    json += "\"type\":\"CS+\",";
                    break;
                case Schedules.TrialType.CSminus:
                    json += "\"type\":\"CS-\",";
                    break;
            }

            json += "\"result\":";
            if (result.Contains("trace"))
            {
                json += "\"" + "trace" + "\",";
            }
            else
            {
                string[] subResults = result.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (subResults.Length > 0 && subResults[0].Contains("="))
                {
                    json += "{";
                    for (int k = 0; k < subResults.Length; k++)
                    {
                        string[] eqparts = subResults[k].Split(new char[] { '=' });
                        json += "\"" + eqparts[0] + "\":" + eqparts[1];
                        if (k < subResults.Length - 1) json += ",";
                    }
                    json += "},";
                }
                else
                {
                    json += "\"" + result + "\",";
                }
            }
            json += "\"reactionTime_ms\":" + (1000f*reactionTime).ToString("F2") + ",";
            json += "\"family\":\"" + family + "\"";

            string propJSON = "";
            foreach (PropertyBranch b in CreatePropertyTree(properties))
            {
                propJSON = b.AddToJSON(propJSON, true);
            }

            json += propJSON + "}" + System.Environment.NewLine;

            return json;
        }

        public string Test()
        {
            string json = "";
            foreach (PropertyBranch b in CreatePropertyTree(properties))
            {
                json = b.AddToJSON(json, true);
            }
            return json;
        }

        List<PropertyBranch> CreatePropertyTree(List<string> properties)
        {
            PropertyBranch tree = new PropertyBranch("root");

            foreach (string s in properties) tree.Add(s);

            return tree.children;
        }

        internal class PropertyBranch
        {
            public string name;
            public float value = float.NaN;
            public List<PropertyBranch> children = new List<PropertyBranch>();

            public PropertyBranch() {}
            public PropertyBranch(string name)
            {
                this.name = name;
            }
            public PropertyBranch(string name, float value)
            {
                this.name = name;
                this.value = value;
            }

            public void Add(string property)
            {
                if (property.StartsWith("---")) property = property.Remove(0, 4);

                string[] parts = property.Split('=');
                string[] names = parts[0].Split('.');

                if (names.Length == 1)
                {
                    children.Add(new PropertyBranch(parts[0], float.Parse(parts[1])));
                }
                else
                {
                    PropertyBranch b = children.Find(o => o.name == names[0]);
                    if (b == null)
                    {
                        b = new PropertyBranch(names[0]);
                        children.Add(b);
                    }
                    b.Add(property.Remove(0, names[0].Length + 1));
                }
            }

            public string AddToJSON(string json, bool comma)
            {
                string n = name.Replace("{", "");
                n = n.Replace("}", "");

                json += (comma?",":"") + "\"" + n + "\":";


                if (children.Count == 0)
                {
                    json += value.ToString();
                }
                else
                {
                    json += "{";
                    for (int k = 0; k < children.Count; k++) json = children[k].AddToJSON(json, k>0);
                    json += "}";
                }

                return json;
            }

        }

    }

}