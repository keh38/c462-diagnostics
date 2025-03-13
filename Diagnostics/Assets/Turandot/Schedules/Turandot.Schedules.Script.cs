using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Schedules
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Script
    {
        public class Config
        {
            public string File = "";
            public string Condition = "";

            public bool Uses(float value)
            {
                return true;
            }
        }

        public List<string> ConfigFiles = new List<string>();
//        public List<Config> ConfigFiles = new List<Config>();
        public string Values;
        public string Groups;
        public VarDimension Dim = VarDimension.X;
        public Order order = Order.Sequential;
        public int Repeats = 1;
        public string TestEars = "";
        public Instructions SecondaryInstructions = null;
        public string linkTo = "";
        public int SplitAfter = 0;
        public bool oneRun = false;

        public Script() { }

    }
}