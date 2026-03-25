 using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
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

        public string Label { set; get; }
        private bool ShouldSerializeLabel() { return false; }

        public string MinLabel { set; get; }
        private bool ShouldSerializeMinLabel() { return false; }

        public string MaxLabel { set; get; }
        private bool ShouldSerializeMaxLabel() { return false; }

        public int FontSize { set; get; }
        private bool ShouldSerializeFontSize() { return false; }

        public bool ThumbOnly { set; get; }
        private bool ShouldSerializeThumbOnly() { return false; }

        public bool BarClickable { set; get; }
        private bool ShouldSerializeBarClickable() { return false; }

        public bool ThumbTogglesSound { set; get; }
        private bool ShouldSerializeThumbTogglesSound() { return false; }

        public float Min { set; get; }
        private bool ShouldSerializeMin() { return false; }

        public float Max { set; get; }
        private bool ShouldSerializeMax() { return false; }

        public SliderScale Scale { set; get; }
        private bool ShouldSerializeScale() { return false; }

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


    }
}
