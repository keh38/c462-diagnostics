using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class ScalerLayout : InputLayout
    {
        [Category("Appearance")]
        public int Width { get; set; }
        private bool ShouldSerializeWidth() { return false; }

        [Category("Appearance")]
        public int Height { get; set; }
        private bool ShouldSerializeHeight() { return false; }

        [Category("Appearance")]
        public int FontSize { get; set; }
        private bool ShouldSerializeFontSize() { return false; }

        [Category("Behavior")]
        [Description("Shows the thumb")]
        public bool ShowThumb { set; get; }
        private bool ShouldSerializeShowThumb() { return false; }

        [Category("Behavior")]
        [Description("Shows the slider fill")]
        public bool ShowFill { set; get; }
        private bool ShouldSerializeShowFill() { return false; }

        [Category("Behavior")]
        public bool BarClickable { set; get; }
        private bool ShouldSerializeBarClickable() { return false; }

        [Category("Labels")]
        [DisplayName("Min")]
        public string MinLabel { get; set; }
        private bool ShouldSerializeMinLabel() { return false; }

        [Category("Labels")]
        [DisplayName("Max")]
        public string MaxLabel { get; set; }
        private bool ShouldSerializeMaxLabel() { return false; }


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
        }
    }
}
