using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;

namespace Turandot.Inputs
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ScalerAction : Input
    {
        //public int length = 1600;
        //public uint bgColor = 0;
        //public bool thumb = false;
        //public TickSpex tickSpex = new TickSpex();
        //public ScaleReference scaleReference = new ScaleReference();

        public ScalerAction() : base("Scaler")
        {
            //bgColor = 0;
        }
    }
}
