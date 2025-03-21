using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;

namespace KLib.Signals.Waveforms
{
    /// <summary>
    /// Waveform base class.
    /// </summary>
    [System.Serializable]
	[ProtoContract]
    [XmlInclude(typeof(MovingRippleNoise))]
    [ProtoInclude(500, typeof(MovingRippleNoise))]
    [XmlInclude(typeof(FM))]
    [ProtoInclude(501, typeof(FM))]
    [XmlInclude(typeof(Noise))]
    [ProtoInclude(502, typeof(Noise))]
    [XmlInclude(typeof(Sinusoid))]
    [ProtoInclude(504, typeof(Sinusoid))]
    [XmlInclude(typeof(ToneCloud))]
    [ProtoInclude(506, typeof(ToneCloud))]
    [XmlInclude(typeof(UserFile))]
    [ProtoInclude(507, typeof(UserFile))]
    [XmlInclude(typeof(RippleNoise))]
    [ProtoInclude(508, typeof(RippleNoise))]
    [XmlInclude(typeof(Digitimer))]
    [ProtoInclude(509, typeof(Digitimer))]
    [JsonObject(MemberSerialization.OptIn)]
    public class Waveform
    {
        [ProtoMember(1, IsRequired = true)]
        [JsonProperty]
        protected Waveshape shape;

        public Waveshape Shape
        {
            get { return shape; }
        }

        protected string _shortName = "None";
        //[ProtoMember(2)]
        //[JsonProperty]
        public string ShortName
        {
            get { return _shortName; }
        }
        public string LongName
        {
            get { return shape.ToString().Replace('_', ' '); }
        }

        public bool HandlesModulation { get; set; } = false;

        /// <summary>
        /// Local record of the sampling rate.
        /// </summary>
        /// <seealso cref="Initialize"/>
        protected float samplingRate_Hz;

        /// <summary>
        /// Sampling interval (s), computed from <paramref name="samplingRate_Hz"/>.
        /// </summary>
        /// <seealso cref="Initialize"/>
        protected float dt;

        /// <summary>
        /// Local record of the audio frame size (buffer length).
        /// </summary>
        /// <seealso cref="Initialize"/>
        protected int Npts;

        /// <summary>
        /// Audio frame duration (s), computed from <paramref name="samplingRate_Hz"/> and <paramref name="Npts"/>.
        /// </summary>
        /// <seealso cref="Initialize"/>
        protected float T;

        protected Channel _channel;
        protected CalibrationData _calib;

        protected string _name;

        [ProtoIgnore]
        [JsonIgnore]
        public string ChannelName
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Instantiates Waveform object.
        /// </summary>
        public Waveform()
        {
			shape = Waveshape.None;
        }

		virtual public List<string> GetSweepableParams()
		{
			return new List<string>();
		}

		virtual public Action<float> GetParamSetter(string paramName)
		{
			Action<float> setter = null;
			return setter;
		}

        virtual public void ResetSweepables()
        {
        }

        virtual public string SetParameter(string paramName, float value)
        {
            return "";
        }

        virtual public float GetParameter(string paramName)
        {
            return float.NaN;
        }

        virtual public List<string> GetValidParameters()
        {
            return new List<string>();
        }

        /// <summary>
        /// Waveform base class initialization.
        /// </summary>
        /// <param name="Fs">Sampling rate (Hz)</param>
        /// <param name="N">Audio frame size (buffer length)</param>
        /// <returns>Returns true if successful</returns>
        /// <remarks>
        /// Saves local copy of sampling rate and buffer lengths. For convenience, pre-computes corresponding values of sampling interval and buffer duration.
        /// Derived classes should override this function to provide class-specific initialization, i.e.: <code>base.Initialize(Fs, N);</code>
        /// </remarks>
        virtual public void Initialize(float Fs, int N, Channel channel)
        {
            dt = 1.0f / Fs;
            samplingRate_Hz = Fs;
            Npts = N;
            T = N * dt;

            _channel = channel;
            if (_channel.level.Cal == null)
            {
            //    throw new ApplicationException("Null calibration data");
            }

            _calib = _channel.level.Cal;
        }

        virtual public float GetMaxLevel(Level level, float Fs)
        {
            return 0;
        }

		virtual public References Create(float[] data)
		{
            return new References();
        }
		
        /// <summary>
        /// Returns a time vector (in ms) for the current sampling rate and buffer size (convenient for plotting)
        /// </summary>
        /// <param name="t0">Vector start time (ms). Useful for concatenating buffers.</param>
        /// <returns></returns>
        public float[] GetTimeVector(float t0)
        {
            float[] t = new float[Npts];
            float cur_t = t0;
            for (int k = 0; k < Npts; k++)
            {
                t[k] = cur_t;
                cur_t += dt*1000;
            }

            return t;
        }
    }

}
