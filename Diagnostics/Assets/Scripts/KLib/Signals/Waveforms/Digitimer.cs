using UnityEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;

using OrderedPropertyGrid;

namespace KLib.Signals.Waveforms
{
    [Serializable]
    [ProtoContract]
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(DigitimerConverter))]
    public class Digitimer : Waveform
    {
        public enum DemandSource { Internal, External};

        [ProtoMember(1, IsRequired = true)]
        [JsonProperty]
        [PropertyOrder(1)]
        [DisplayName("Pulse rate")]
        [Description("Pulse rate in pulse per second (pps)")]
        public float PulseRate_Hz { get; set; }
        private bool ShouldSerializePulseRate_Hz() { return false; }

        [ProtoMember(2, IsRequired = true)]
        [JsonProperty]
        [PropertyOrder(2)]
        [DisplayName("Mode")]
        [Description("")]
        [TypeConverter(typeof(DigitimerModeConverter))]
        public float PulseMode { get; set; }
        private bool ShouldSerializePulseMode() { return false; }

        [ProtoMember(3, IsRequired = true)]
        [JsonProperty]
        [PropertyOrder(3)]
        [DisplayName("Polarity")]
        [TypeConverter(typeof(DigitimerPolarityConverter))]
        public float PulsePolarity { get; set; }
        private bool ShouldSerializePulsePolarity() { return false; }

        private float _width;
        [ProtoMember(4, IsRequired = true)]
        [JsonProperty]
        [PropertyOrder(4)]
        [Description("Pulse width in us (50-2000)")]
        public float Width
        {
            get { return _width; }
            set
            {
                _width = value;
                if (_width < 50) _width = 50;
                if (_width > 2000) _width = 2000;

                _width = Mathf.Round(_width / 10f) * 10f;
            }
        }
        private bool ShouldSerializeWidth() { return false; }

        private float _recovery;
        [ProtoMember(5, IsRequired = true)]
        [JsonProperty]
        [PropertyOrder(5)]
        [Description("Duration of recovery phase as % of pulse width (10-100)")]
        public float Recovery
        {
            get { return _recovery; }
            set
            {
                _recovery = value;
                if (_recovery < 10) _recovery = 10;
                if (_recovery > 100) _recovery = 100;

                _recovery = Mathf.Round(_recovery);
            }
        }
        private bool ShouldSerializeRecovery() { return false; }

        private float _dwell;
        [ProtoMember(6, IsRequired = true)]
        [JsonProperty]
        [PropertyOrder(6)]
        [Description("Interphase gap in us (1 to 990 in steps of 10)")]
        public float Dwell
        {
            get { return _dwell; }
            set
            {
                _dwell = value;
                if (_dwell < 1) _dwell = 1;
                if (_dwell > 990) _dwell = 990;
            }
        }
        private bool ShouldSerializeDwell() { return false; }

        [ProtoMember(7, IsRequired = true)]
        [JsonProperty]
        [PropertyOrder(7)]
        public DemandSource Source { get; set; }
        private bool ShouldSerializeSource() { return false; }

        private float _demand;
        [ProtoMember(8, IsRequired = true)]
        [JsonProperty]
        [PropertyOrder(8)]
        [Description("Pulse amplitude (0-1000 mA)")]
        public float Demand
        {
            get { return _demand; }
            set
            {
                _demand = value;
                if (_demand < 0) _demand = 0;
                if (_dwell > 1000) _demand = 1000;
            }
        }
        private bool ShouldSerializeDemand() { return false; }

        private float _pulseCarrierFreq = 1000;
        private float[] _pulse;
        private int _nptsPeriod;
        private int _pulseIndex;
        private int _nextPulseIndex;

        private float[] _amBuffer;
        private float _currentModulationValue;

        private int _runningTimeIndex;

        public Digitimer()
        {
            PulseRate_Hz = 20;
            PulseMode = 1;
            PulsePolarity = 2;
            Width = 200;
            Recovery = 100;
            Dwell = 1;
            Demand = 0;
            Source = DemandSource.Internal;

            shape = Waveshape.Digitimer;
            _shortName = "Digitimer";
            HandlesModulation = true;
        }

        public override List<string> GetSweepableParams()
        {
            var items = new List<string>()
            {
                "PulseWidth_us"
            };

            if (Source == DemandSource.Internal)
            {
                items.Add("Demand_mA");
            }
            return items;
        }

        override public Action<float> GetParamSetter(string paramName)
        {
            Action<float> setter = null;
            switch (paramName)
            {
                case "PulseRate_Hz":
                    setter = x => { this.PulseRate_Hz = x; _nptsPeriod = Mathf.RoundToInt(1 / (dt * PulseRate_Hz)); };

                    break;
                case "Demand_mA":
                    setter = x => this.Demand = x;
                    break;
                case "PulseWidth_us":
                    setter = x => this.Width = x;
                    break;
            }

            return setter;
        }

        override public string SetParameter(string paramName, float value)
        {
            //Debug.Log($"{paramName} = {value}");
            switch (paramName)
            {
                case "PulseRate_Hz":
                    this.PulseRate_Hz = value;
                    _nptsPeriod = Mathf.RoundToInt(1 / (dt * PulseRate_Hz));
                    break;
                case "PulseWidth_us":
                    this.Width = value;
                    break;
                case "Demand_mA":
                    Demand = value;
                    break;
            }

            return "";
        }

        override public float GetParameter(string paramName)
        {
            switch (paramName)
            {
                case "PulseRate_Hz":
                    return PulseRate_Hz;
                case "PulseWidth_us":
                    return Width;
                case "Demand_mA":
                    return Demand;
            }

            return float.NaN;
        }

        override public List<string> GetValidParameters()
        {
            List<string> plist = new List<string>();
            plist.Add("PulseRate_Hz");
            plist.Add("PulseWidth_us");
            if (Source == DemandSource.Internal)
            {
                plist.Add("Demand_mA");
            }
            return plist;
        }

        override public void ResetSweepables()
        {
            //lastFreq = Frequency_Hz;
            //phase_radians = 2 * Mathf.PI * Phase_cycles;
        }

        override public void Initialize(float Fs, int N, Channel channel)
        {
            base.Initialize(Fs, N, channel);

            int npts = Mathf.RoundToInt(Fs / _pulseCarrierFreq);
            _pulse = new float[npts];
            for (int k=0; k<npts; k++)
            {
                _pulse[k] = Mathf.Sin(2 * Mathf.PI * k / Fs * _pulseCarrierFreq);
            }

            _nptsPeriod = Mathf.RoundToInt(Fs / PulseRate_Hz);
            _nextPulseIndex = (_channel.gate.DelaySamples > 0) ? _channel.gate.DelaySamples : 0;
            _pulseIndex = 0;
            _runningTimeIndex = 0;

            _amBuffer = new float[N];
            _currentModulationValue = 0;
        }

        public override float GetMaxLevel(Level level, float Fs)
        {
            return 1;// (level.Cal == null) ? float.NaN : level.Cal.GetMax(Frequency_Hz);
        }

        override public References Create(float[] data)
        {

            for (int k = 0; k < _amBuffer.Length; k++) _amBuffer[k] = 1;
            _channel.modulation.Apply(_amBuffer, -1);

            for (int k = 0; k < data.Length; k++)
            {
                if (_runningTimeIndex == _nextPulseIndex)
                {
                    _currentModulationValue = _amBuffer[k];
                }

                if (_runningTimeIndex >= _nextPulseIndex)
                {
                    data[k] = _pulse[_pulseIndex] * _currentModulationValue;
                    _pulseIndex++;
                    if (_pulseIndex == _pulse.Length)
                    {
                        _pulseIndex = 0;
                        _nextPulseIndex += _nptsPeriod;
                    }
                }
                _runningTimeIndex++;
                if (_channel.gate.TotalSamples > 0 && _runningTimeIndex == _channel.gate.TotalSamples)
                {
                    _runningTimeIndex = 0;
                    _nextPulseIndex = _channel.gate.DelaySamples;
                }
            }

            return new References();
        }

    }
}
