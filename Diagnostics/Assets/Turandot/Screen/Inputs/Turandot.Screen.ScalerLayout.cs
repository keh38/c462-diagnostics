using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;


namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class ScalerLayout : InputLayout
    {
        public int Width { get; set; }
        private bool ShouldSerializeWidth() { return false; }

        public int Height { get; set; }
        private bool ShouldSerializeHeight() { return false; }

        public int FontSize { get; set; }
        private bool ShouldSerializeFontSize() { return false; }

        public bool ShowThumb { set; get; }
        private bool ShouldSerializeShowThumb() { return false; }

        public bool ShowFill { set; get; }
        private bool ShouldSerializeShowFill() { return false; }

        public bool BarClickable { set; get; }
        private bool ShouldSerializeBarClickable() { return false; }

        public string MinLabel { get; set; }
        private bool ShouldSerializeMinLabel() { return false; }

        public string MaxLabel { get; set; }
        private bool ShouldSerializeMaxLabel() { return false; }

        public float MinValue { get; set; }
        private bool ShouldSerializeMinValue() { return false; }

        public float MaxValue { get; set; }
        private bool ShouldSerializeMaxValue() { return false; }

        public bool WholeNumbers { get; set; }
        private bool ShouldSerializeWholeNumbers() { return false; }

        public bool ShowTicks { get; set; }
        private bool ShouldSerializeShowTicks() { return false; }

        public bool LabelTicks { get; set; }
        private bool ShouldSerializeLabelTicks() { return false; }


        public ScalerLayout()
        {
            Name = "Scaler";
            Width = 1000;
            Height = 125;
            FontSize = 48;
            MinLabel = "";
            MaxLabel = "";
            ShowFill = true;
            ShowThumb = true;
            BarClickable = false;
            MinValue = 0;
            MaxValue = 1;
            WholeNumbers = false;
            ShowTicks = false;
            LabelTicks = false;
        }
    }
}
