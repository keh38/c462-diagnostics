using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;
using ProtoBuf;

using OrderedPropertyGrid;
using KLib.TypeConverters;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    [TypeConverter(typeof(SortableTypeConverter))]
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
        [PropertyOrder(0)]
        [DisplayName("Min")]
        public string MinLabel { get; set; }
        private bool ShouldSerializeMinLabel() { return false; }

        [Category("Labels")]
        [PropertyOrder(1)]
        [DisplayName("Max")]
        public string MaxLabel { get; set; }
        private bool ShouldSerializeMaxLabel() { return false; }

        [Category("Scale")]
        [DisplayName("Min")]
        [PropertyOrder(0)]
        public float MinValue { get; set; }
        private bool ShouldSerializeMinValue() { return false; }

        [Category("Scale")]
        [DisplayName("Max")]
        [PropertyOrder(1)]
        public float MaxValue { get; set; }
        private bool ShouldSerializeMaxValue() { return false; }

        [Category("Scale")]
        [DisplayName("Whole numbers")]
        [PropertyOrder(1)]
        public bool WholeNumbers { get; set; }
        private bool ShouldSerializeWholeNumbers() { return false; }

        [Category("Ticks")]
        [DisplayName("Show")]
        [PropertyOrder(0)]
        public bool ShowTicks { get; set; }
        private bool ShouldSerializeShowTicks() { return false; }

        [Category("Ticks")]
        [PropertyOrder(1)]
        [DisplayName("Label")]
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
