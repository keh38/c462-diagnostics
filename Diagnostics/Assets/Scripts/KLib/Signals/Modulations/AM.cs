using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Enumerations;

namespace KLib.Signals.Modulations
{
    [System.Serializable]
	[ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
	[JsonObject(MemberSerialization.OptOut)]
    [XmlInclude(typeof(SinusoidalAM))]
    [ProtoInclude(500, typeof(SinusoidalAM))]
    public class AM
    {
        [ProtoMember(1, IsRequired = true)]
        public bool ApplyLevelCorrection = true;

		protected float _samplingRate_Hz;
		protected float _dt;
		protected int _npts;
		protected float _T;

        protected AMShape _shape = AMShape.None;
        [ProtoIgnore]
        public AMShape Shape
        {
            get { return _shape; }
        }

        protected string _shortName = "None";
        [ProtoIgnore]
        public string ShortName
        {
            get { return _shortName; }
        }
        [ProtoIgnore]
        public string LongName
        {
            get { return _shape.ToString().Replace('_', ' '); }
        }

        public AM()
        {
        }

        public virtual List<string> GetSweepableParams()
        {
            return new List<string>();
        }

        public virtual Action<float> GetParamSetter(string paramName)
        {
            return null;
        }

        virtual public List<string> GetValidParameters()
        {
            List<string> p = new List<string>();
            return p;
        }

        virtual public string SetParameter(string paramName, float value)
        {
            return "";
        }

        virtual public float GetParameter(string paramName)
        {
            return float.NaN;
        }

        public virtual void ResetSweepables()
        {
        }

        public virtual bool Initialize(float Fs, int N)
        {
			_dt = 1.0f / Fs;
			_samplingRate_Hz = Fs;
			_npts = N;
			_T = N * _dt;
            return true;
        }

        public virtual float Apply(float[] data)
        {
            return 0;
        }

        [ProtoIgnore]
        public virtual float LevelCorrection
        {
            get
            {
                return 0;
            }
        }

    }
}
