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

        private float _pulseCarrierFreq = 1000;
        private float[] _pulse;
        private int _nptsPeriod;
        private int _pulseIndex;
        private int _nextPulseIndex;

        private int _runningTimeIndex;

        public Digitimer()
        {
            PulseRate_Hz = 20;

            shape = Waveshape.Digitimer;
            _shortName = "Digitimer";
        }

        public override List<SweepableParam> GetSweepableParams()
        {
            List<SweepableParam> par = new List<SweepableParam>();

            return par;
        }

        override public Action<float> ParamSetter(string paramName)
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

        override public void Initialize(float Fs, int N, Gate gate, Level level)
        {
            base.Initialize(Fs, N, gate, level);

            int npts = Mathf.RoundToInt(Fs / _pulseCarrierFreq);
            _pulse = new float[npts];
            for (int k=0; k<npts; k++)
            {
                _pulse[k] = Mathf.Sin(2 * Mathf.PI * k / Fs * _pulseCarrierFreq);
            }

            _nptsPeriod = Mathf.RoundToInt(Fs / PulseRate_Hz);
            _nextPulseIndex = (_gate.DelaySamples > 0) ? _gate.DelaySamples : 0;
            _pulseIndex = 0;
            _runningTimeIndex = 0;
        }

        public override float GetMaxLevel(Level level, float Fs)
        {
            return 1;// (level.Cal == null) ? float.NaN : level.Cal.GetMax(Frequency_Hz);
        }

        override public References Create(float[] data)
        {
            for (int k = 0; k < data.Length; k++)
            {
                if (_runningTimeIndex >= _nextPulseIndex)
                {
                    data[k] = _pulse[_pulseIndex];
                    _pulseIndex++;
                    if (_pulseIndex == _pulse.Length)
                    {
                        _pulseIndex = 0;
                        _nextPulseIndex += _nptsPeriod;
                    }
                }
                _runningTimeIndex++;
                if (_gate.TotalSamples > 0 && _runningTimeIndex == _gate.TotalSamples)
                {
                    _runningTimeIndex = 0;
                    _nextPulseIndex = _gate.DelaySamples;
                }
            }

            var r = new References();
            r.maxVal = 1;
            r.refVal = 1;
            return r;// _calib.GetAllReferences(Frequency_Hz);
        }

    }
}
