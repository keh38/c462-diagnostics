using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Inputs
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class SAM : Input
    {
        [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
        [JsonObject(MemberSerialization.OptOut)]
        public class Appearance
        {
            public bool visible;
            public string minlabel;
            public string maxLabel;

            public Appearance()
            {
                visible = true;
            }
        }

        public int color = 0xFFFFFF;

        public Appearance valence = new Appearance();
        public Appearance arousal = new Appearance();
        public Appearance dominance = new Appearance();
        public Appearance loudness = new Appearance();

        public SAM() : base("SAM")
        {
            //color = KLib.Unity.ColorInt(1, 1, 1, 1);
        }

    }
}
