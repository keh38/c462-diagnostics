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
    public class ParamSliderAction : Input
    {
        public bool reset;
        public string channel;
        public string parameter;
        public float minVal;
        public float maxVal;
        public bool isLog;
        public float startVal;
        public float startRange = 0;
        public float shrinkFactor = 0;
        public bool showButton = true;
        public bool thumbTogglesSound = true;
        public string leftLabel = "";
        public string rightLabel = "";

        public ParamSliderAction() : base("Param slider")
        {
        }

    }
}
