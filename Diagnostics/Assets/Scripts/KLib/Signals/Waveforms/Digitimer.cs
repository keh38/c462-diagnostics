using UnityEngine;
using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;

namespace KLib.Signals.Waveforms
{
    [Serializable]
    [ProtoContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class Digitimer : Waveform
    {
        [ProtoMember(1, IsRequired = true)]
        [JsonProperty]
        public float PulseRate_Hz;

        [ProtoMember(2, IsRequired = true)]
        [JsonProperty]
        public float PulseMode;

        [ProtoMember(3, IsRequired = true)]
        [JsonProperty]
        public float PulsePolarity;

        [ProtoMember(4, IsRequired = true)]
        [JsonProperty]
        public float Width;

        [ProtoMember(5, IsRequired = true)]
        [JsonProperty]
        public float Recovery;

        [ProtoMember(6, IsRequired = true)]
        [JsonProperty]
        public float Dwell;

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

            shape = Waveshape.Digitimer;
            _shortName = "Digitimer";
            HandlesModulation = true;
        }

        public override List<string> GetSweepableParams()
        {
            return new List<string>();
        }

        override public Action<float> GetParamSetter(string paramName)
        {
            Action<float> setter = null;
            //switch (paramName)
            //{
            //    case "Frequency":
            //        setter = x => this.Frequency_Hz = x;
            //        break;
            //    case "Phase":
            //        setter = x => this.Phase_cycles = x;
            //        break;
            //}

            return setter;
        }

        override public string SetParameter(string paramName, float value)
        {
            switch (paramName)
            {
                case "PulseRate_Hz":
                    this.PulseRate_Hz = value;
                    _nptsPeriod = Mathf.RoundToInt(1 / (dt*PulseRate_Hz));
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
            }

            return float.NaN;
        }

        override public List<string> GetValidParameters()
        {
            List<string> plist = new List<string>();
            plist.Add("PulseRate_Hz");
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
            _channel.modulation.Apply(_amBuffer);

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
