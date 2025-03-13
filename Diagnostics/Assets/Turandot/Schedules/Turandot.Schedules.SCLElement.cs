using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Schedules
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class SCLElement
    {
        public int block;
        public int track;
        public int trial;
        public string group = "";
        public string adapt = "";
        public TrialType trialType;
        public int ix;
        public int iy;
        public List<PropValPair> propValPairs = new List<PropValPair>();

        public SCLElement() { }

        public SCLElement(int block, int trial)
        {
            this.block = block;
            this.trial = trial;
        }
    }

    public class StimConList : List<SCLElement> { }

}