 using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

using Newtonsoft.Json;

using KLib.Signals;

namespace Turandot.Inputs
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ParamSliderAction : Input
    {
        internal class ContextItems
        {
            internal static string _selectedChannel;
            internal static string[] _listOfChannels;
            internal static string[] _listOfProperties;
            internal static List<ChannelProperties> _validProperties;
        }

        public enum SliderScale { Linear, Log }

        public bool reset;
        public float startRange = 0;
        public float shrinkFactor = 0;
        public bool showButton = true;

        [Category("Appearance")]
        public string Label { set; get; }
        private bool ShouldSerializeLabel() { return false; }

        [Category("Appearance")]
        public string MinLabel { set; get; }
        private bool ShouldSerializeMinLabel() { return false; }

        [Category("Appearance")]
        public string MaxLabel { set; get; }
        private bool ShouldSerializeMaxLabel() { return false; }

        [Category("Appearance")]
        public int FontSize { set; get; }
        private bool ShouldSerializeFontSize() { return false; }

        [Category("Behavior")]
        [Description("Hides the slider fill")]
        public bool ThumbOnly { set; get; }
        private bool ShouldSerializeThumbOnly() { return false; }

        [Category("Behavior")]
        public bool BarClickable { set; get; }
        private bool ShouldSerializeBarClickable() { return false; }

        [Category("Behavior")]
        public bool ThumbTogglesSound { set; get; }
        private bool ShouldSerializeThumbTogglesSound() { return false; }

        [Category("Scale")]
        public float Min { set; get; }
        private bool ShouldSerializeMin() { return false; }

        [Category("Scale")]
        public float Max { set; get; }
        private bool ShouldSerializeMax() { return false; }

        [Category("Scale")]
        public SliderScale Scale { set; get; }
        private bool ShouldSerializeScale() { return false; }

        [Category("Parameter")]
        public float StartValue { get; set; }
        private bool ShouldSerializeStartValue() { return false; }

        public ParamSliderAction() : base("Param Slider")
        {
            Label = "";
            MinLabel = "";
            MaxLabel = "";
            ThumbOnly = false;
            BarClickable = false;
            ThumbTogglesSound = true;
            FontSize = 48;
        }

        // https://www.codeproject.com/Articles/9517/PropertyGrid-and-Drop-Down-properties
        private string _channel;
        [Category("Parameter")]
        [TypeConverter(typeof(ChannelConverter))]
        public string Channel
        {
            get
            {
                string channel = "";
                if (_channel != null)
                {
                    channel = _channel;
                }
                else
                {
                    channel = "";
                    if (ContextItems._listOfChannels != null && ContextItems._listOfChannels.Length > 0)
                    {
                        //Sort the list before displaying it
                        Array.Sort(ContextItems._listOfChannels);
                        channel = ContextItems._listOfChannels[0];
                        _channel = channel;
                    }
                }
                return channel;
            }
            set
            {
                _channel = value;
                ContextItems._selectedChannel = _channel;
                if (ContextItems._validProperties != null)
                {
                    ContextItems._listOfProperties = ContextItems._validProperties.Find(x => x.channelName == _channel).properties.ToArray();
                }
            }
        }
        private bool ShouldSerializeChannel() { return false; }

        private string _parameter;
        [Category("Parameter")]
        [TypeConverter(typeof(PropertyConverter))]
        public string Property
        {
            get
            {
                string parameter = "";
                if (_parameter != null)
                {
                    parameter = _parameter;
                }
                else
                {
                    _parameter = "";
                    if (ContextItems._listOfChannels != null && ContextItems._listOfProperties.Length > 0)
                    {
                        //Sort the list before displaying it
                        Array.Sort(ContextItems._listOfProperties);
                        parameter = ContextItems._listOfProperties[0];
                        _parameter = parameter;
                    }
                }
                return parameter;
            }
            set
            {
                _parameter = value;
            }
        }
        private bool ShouldSerializeProperty() { return false; }

        public void SetDataForContext(List<ChannelProperties> validProperties)
        {
            ContextItems._listOfChannels = validProperties.Select(x => x.channelName).ToArray();
            if (ContextItems._listOfChannels.Length > 0)
            {
                ContextItems._selectedChannel = ContextItems._listOfChannels[0];
                ContextItems._listOfProperties = validProperties[0].properties.ToArray();
            }
            ContextItems._validProperties = validProperties;
        }

        public class ChannelConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                //true means show a combobox
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                //true will limit to list. false will show the list, 
                //but allow free-form entry
                return true;
            }

            public override System.ComponentModel.TypeConverter.StandardValuesCollection
                   GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(ContextItems._listOfChannels);
            }
        }

        public class PropertyConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }
            public override System.ComponentModel.TypeConverter.StandardValuesCollection
                   GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(ContextItems._listOfProperties);
            }
        }


    }
}
