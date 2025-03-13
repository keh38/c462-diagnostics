using UnityEngine;
using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;

namespace KLib.Signals
{
    #region ENUMERATIONS
    public enum LevelReference
    {
        Total_power,
        Spectrum_level,
    }
    public enum LevelUnits
    {
        Volts,
        dB_attenuation,
        dB_Vrms,
        dB_SPL,
        dB_HL,
        dB_SL,
        PercentDR,
        dB_SPL_noLDL
    };
    #endregion

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Level
    {
        [ProtoMember(1, IsRequired = true)]
        public LevelUnits Units { set; get; }

        [ProtoMember(2, IsRequired = true)]
        public LevelReference Reference { set; get; }

        [ProtoMember(3, IsRequired = true)]
        public float Value { set; get; }

        [ProtoMember(4, IsRequired = true)]
        public float VolumeControldB { set; get; }

        [ProtoIgnore]
        [JsonIgnore]
        public float Atten { get; private set; }

        [ProtoIgnore]
        [JsonIgnore]
        public CalibrationData Cal { get { return _calib; } }

        //float Fs;
        //float refVal;
        float _lastAtten;
        float _lastAmplitude;
        float _vmax = 1;
        string _transducer;
        float _rove = 0;
        string _name = "";
        bool _clampToMax = false;

        float _sl_to_spl;

        private string _channel = "";
        private CalibrationData _calib = null;

        [ProtoIgnore]
        [JsonIgnore]
        public string Destination
        {
            get { return _channel; }
        }

        [ProtoIgnore]
        [JsonIgnore]
        public bool SPLBasedUnits
        {
            get { return Units == LevelUnits.dB_SPL || Units == LevelUnits.dB_HL || Units == LevelUnits.dB_SL || Units == LevelUnits.dB_SPL_noLDL; }
        }

        [ProtoIgnore]
        [JsonIgnore]
        public float SL_to_SPL
        {
            get { return _sl_to_spl; }
        }

        [ProtoIgnore]
        [JsonIgnore]
        public float Rove
        {
            get { return _rove; }
            set { _rove = value; }
        }

        [ProtoMember(5, IsRequired = true)]
        public bool ClampToMax
        {
            get { return _clampToMax; }
            set { _clampToMax = value; }
        }

        [ProtoIgnore]
        [JsonIgnore]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [ProtoIgnore]
        [JsonIgnore]
        public string Transducer
        {
            get { return _transducer; }
        }

        public static List<SweepableParam> GetSweepableParams()
        {
            List<SweepableParam> par = new List<SweepableParam>();
            par.Add(new SweepableParam("Level", "dB", -6));

            return par;
        }

        public Action<float> ParamSetter(string paramName)
        {
            Action<float> setter = null;
            switch (paramName)
            {
                case "Level":
                    setter = x => this.Value = x;
                    break;
            }

            return setter;
        }

        public void ResetSweepables()
        {
            _lastAtten = float.NaN;
            _lastAmplitude = float.NaN;
        }

        public Level()
        {
            Units = LevelUnits.dB_attenuation;
            Reference = LevelReference.Total_power;
            Value = 0;
            _lastAtten = 0;
            _lastAmplitude = 0;
            VolumeControldB = 0;
        }

        public void Copy(Level that)
        {
            this.Units = that.Units;
            this.Value = that.Value;
            this.VolumeControldB = that.VolumeControldB;
            this._lastAtten = float.NaN;
        }

        public List<string> GetValidParameters()
        {
            return new List<string>(new string[] { "Level", "Level.Rove" });
        }

        public void Initialize(string endpoint, float vmax)
        {
            var parts = endpoint.Split('.');
            _transducer = parts[0];
            _channel = parts[1];
            _vmax = vmax;

            _calib = CalibrationFactory.Load(Units, _transducer, _channel);

            ResetSweepables();
        }

        public void Apply(float[] data, References r)
        {
            float tmpValue = Value;
            int N = data.Length;
            _sl_to_spl = float.NaN;

            if (Units == LevelUnits.Volts)
            {
                r.refVal = r.maxVal = _vmax;
            }
            else if (Units == LevelUnits.dB_attenuation)
            {
                r.refVal = r.maxVal = 0;
            }
            else if (Units == LevelUnits.PercentDR)
            {
                Value *= r.dynamicRange * 0.01f;
                _sl_to_spl = Value + r.thrSPL;
                Value += _rove;
            }

            if (float.IsNaN(Value) || Value > r.maxVal + float.Epsilon)
            {
                if (_clampToMax)
                {
                    //Debug.Log(Name + " CLAMPED");
                    Value = r.maxVal;
                }
                else
                {
                    Debug.Log("Ref = " + r.refVal.ToString("F1") + ", Max = " + r.maxVal.ToString("F1") +
                              " => Value = " + Value.ToString("F1") + " " + Units);

                    if (Units == LevelUnits.PercentDR) Value = tmpValue;

                    throw new System.Exception("Level over range.");
                }
            }

            float curAtten = ValueToAtten(Mathf.Min(Value, r.maxVal), r.refVal) - VolumeControldB;

            //Debug.Log("Ref = " + r.refVal.ToString("F1") + ", Max = " + r.maxVal.ToString("F1") +
            //          " => Value = " + Mathf.Min(Value, r.maxVal).ToString("F1") + ", Atten = " + curAtten.ToString("F1"));

            if (Units == LevelUnits.PercentDR) Value = tmpValue;

            if (float.IsNaN(_lastAtten))
                _lastAtten = curAtten;

            if (Units == LevelUnits.Volts)
            {
                float dy = (Value - _lastAmplitude) / N;
                for (int k = 0; k < N; k++)
                {
                    _lastAmplitude += dy;
                    data[k] *= _lastAmplitude;
                }
                _lastAmplitude = Value;
            }
            else
            {
                float dy = (curAtten - _lastAtten) / N;
                float atten = _lastAtten;
                float voltageScale = (Units == LevelUnits.dB_attenuation) ? _vmax : 1;

                for (int k = 0; k < N; k++)
                {
                    atten += dy;
                    data[k] *= Mathf.Pow(10, atten / 20) * voltageScale;
                }

                _lastAtten = curAtten;
            }
        }

        float ValueToAtten(float val, float refVal)
        {
            Atten = 0;
            if (Units == LevelUnits.dB_attenuation)
                Atten = val;
            else
                Atten = val - refVal;

            //Debug.Log("Ref = " + refVal.ToString("F1") + " => Atten = " + _atten.ToString("F1"));

            return Atten;
        }
    }
}
