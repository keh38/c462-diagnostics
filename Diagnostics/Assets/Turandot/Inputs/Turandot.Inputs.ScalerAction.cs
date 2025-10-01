using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Turandot.Inputs
{
    public class ScalerAction : Input
    {
        public float StartValue { get; set; }
        private bool ShouldSerializeStartValue() { return false; }

        public ScalerAction() : base("Scaler")
        {
            StartValue = 0.5f;
        }
    }
}
