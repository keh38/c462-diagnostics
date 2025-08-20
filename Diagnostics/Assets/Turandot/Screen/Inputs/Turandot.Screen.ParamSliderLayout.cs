using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;

namespace Turandot.Screen
{
    public enum ParamSliderButtonStyle { Fixed, Mobile }

    [JsonObject(MemberSerialization.OptOut)]
    public class ParamSliderLayout : InputLayout
    {
        [Category("Appearance")]
        public int Width { get; set; }
        private bool ShouldSerializeWidth() { return false; }

        [Category("Appearance")]
        public int Height { get; set; }
        private bool ShouldSerializeHeight() { return false; }

        [Category("Appearance")]
        public ParamSliderButtonStyle ButtonStyle { set; get; }
        private bool ShouldSerializeButtonStyle() { return false; }


        public ParamSliderLayout()
        {
            Name = "Param Slider";
            Width = 1000;
            Height = 50;
            ButtonStyle = ParamSliderButtonStyle.Fixed;
        }
    }
}
