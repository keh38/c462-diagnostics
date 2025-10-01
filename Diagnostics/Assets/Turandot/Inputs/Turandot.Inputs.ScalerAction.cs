using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Turandot.Inputs
{
    public class ScalerAction : Input
    {
        public bool Enabled { get; set; }
        private bool ShouldSerializeEnabled() { return false; }

        public float StartValue { get; set; }
        private bool ShouldSerializeStartValue() { return false; }

        public ScalerAction() : base("Scaler")
        {
            Enabled = true;
            StartValue = 0.5f;
        }
    }
}
