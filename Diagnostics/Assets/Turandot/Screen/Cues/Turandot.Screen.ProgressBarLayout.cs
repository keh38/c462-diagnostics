using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;
using System.Xml.Serialization;
using Turandot.Cues;

namespace Turandot.Screen
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class ProgressBarLayout : CueLayout
    {
        public int Width { get; set; }
        private bool ShouldSerializeWidth() { return false; }

        public int Height { get; set; }
        private bool ShouldSerializeHeight() { return false; }

        [XmlIgnore]
        public System.Drawing.Color WindowsColor
        {
            get { return System.Drawing.Color.FromArgb(Color); }
            set { Color = value.ToArgb(); }
        }
        private bool ShouldSerializeWindowsColor() { return false; }

        public int Color { set; get; }

        public ProgressBarLayout()
        {
            Name = "ProgressBar";
            X = 0.5f;
            Y = 0.1f;
            Width = 750;
            Height = 75;
            Color = KLib.ColorTranslator.ColorInt(58, 80, 90, 255);
        }

        public ProgressBarAction GetDefaultCue()
        {
            return new ProgressBarAction()
            {
                BeginVisible = true,
                EndVisible = true
            };
        }

    }
}
