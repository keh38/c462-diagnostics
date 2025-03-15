using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

using KLib;
using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;
using KLib.Signals.Modulations;
using KLib.Signals.Waveforms;

namespace KLib.Signals
{
    /// <summary>
    /// Container for a number of audio <see cref="Channel">Channels</see>
    /// </summary>
	[ProtoContract]
	[JsonObject(MemberSerialization.OptIn)]
	public class SignalManager
    {
        [ProtoMember(1)]
        [JsonProperty]
        public string name="";

        [ProtoMember(2)]
        [JsonProperty]
        public List<Channel> channels;

        [ProtoMember(3)]
        [JsonProperty]
        public float SamplingRate_Hz { set; get; }

        [ProtoMember(4)]
        [JsonProperty]
        public int Npts;

        [ProtoMember(5)]
        [JsonProperty]
        public int Noutputs;

        private List<Channel> _shadows = new List<Channel>();

        private int _maxCalls = -1;
        private int _numCalls = 0;
        private float _dt = 0;

        private bool _paused;
        private bool _pausePending;
        private bool _unpausePending;

        private float[] _currentAmplitudes;

        public SignalManager()
		{
            Noutputs = 2;
            SamplingRate_Hz = 44100;
            channels = new List<Channel>();
        }

        public SignalManager(AdapterMap adapterMap)
        {
            Noutputs = 2;
            SamplingRate_Hz = 44100;
            channels = new List<Channel>();
            AdapterMap = adapterMap;
            Noutputs = AdapterMap.NumChannels;
        }

        public SignalManager(float Fs, int Npts)
        {
            Noutputs = 2;
            SamplingRate_Hz = Fs;
            this.Npts = Npts;
            channels = new List<Channel>();
        }

        #region Properties
        [ProtoIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public string Name { set; get; }

        [ProtoIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public AdapterMap AdapterMap { set; get; }

        [ProtoIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public bool HaveException { get { return LastException != null; } }

        [ProtoIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public string LastErrorMessage { get; private set; }

        [ProtoIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public Exception LastException { get; private set; }

        [ProtoIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public string CalibrationFolder { set; get; }

        [ProtoIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public string WavFolder { set; get; }

        [ProtoIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public float Vmax { set; get; } = 1;

        [ProtoIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public float[] CurrentAmplitudes { get { return _currentAmplitudes; } }

        #endregion

        public void AddChannel(string name, Waveform wf)
		{
			channels.Add(new Channel(name, wf));
		}

        public void AddChannel(Channel chan)
        {
            channels.Add(chan);
        }

        public List<Channel> FindAllChannels(string name)
        {
            return _shadows.FindAll(c => c.Name == name);
        }
        public Channel GetChannel(string name)
        {
            return channels.Find(c => c.Name == name);
        }

        public Channel this[string name]
        {
            get { return GetChannel(name); }
        }
        private float GetUserFileDuration(float defaultValue)
        {
            float val = 0;
            foreach (Channel ch in channels)
            {
                if (ch.waveform is UserFile) val = Mathf.Min(val, (ch.waveform as UserFile).Duration);
            }

            return val > 0 ? val : defaultValue;
        }

        //public void Activate(List<Turandot.Flag> flags)
        //{
        //    foreach (var ch in _shadows)
        //    {
        //        ch.InitializeIntramural(flags);
        //    }
        //}

        public float GetMaxTime(float defaultValue)
        {
            float tmax = -1;
            foreach (var ch in channels)
            {
                if (ch.waveform is UserFile) tmax = Mathf.Max(tmax, (ch.waveform as UserFile).Duration);
                if (!string.IsNullOrEmpty(ch.intramural)) tmax = Mathf.Max(tmax, ch.MaxIntramuralTime);
            }

            return tmax > 0 ? tmax : defaultValue;
        }

        public float GetMinTime(float defaultValue)
        {
            float tmin = float.PositiveInfinity;
            foreach (var ch in channels)
            {
                if (ch.waveform is UserFile)
                {
                    var uf = ch.waveform as UserFile;
                    var dur = uf.Duration * uf.SamplingRate / SamplingRate_Hz;
                    tmin = Mathf.Min(tmin, dur);
                }
                if (!string.IsNullOrEmpty(ch.intramural)) tmin = Mathf.Min(tmin, ch.MaxIntramuralTime);
            }

            return (!float.IsPositiveInfinity(tmin)) ? tmin : defaultValue;
        }

        public float GetMaxInterval(float defaultValue)
        {
            float tmax = -1;
            foreach (var ch in channels)
            {
                if (ch.gate.Active && !float.IsPositiveInfinity(ch.gate.Period_ms)) tmax = Mathf.Max(tmax, ch.gate.Period_ms);
            }

            return tmax > 0 ? tmax : defaultValue;
        }

        public void SetMasterVolume(float vol_dB)
        {
            foreach (Channel ch in channels)
            {
                ch.level.VolumeControldB = vol_dB;
            }
            foreach (var ch in _shadows)
            {
                ch.level.VolumeControldB = vol_dB;
            }
        }

        public float MinAtten()
        {
            float amin = float.NegativeInfinity;

            foreach (Channel ch in channels)
            {
                ch.Create();
                amin = Mathf.Max(amin, ch.level.Atten);
            }
            return amin;
        }

        public Action<float> ParamSetter(string paramSpec)
		{
			Action<float> setter = null;
			
			string[] s = paramSpec.Split('.');
			if (s.Length != 2)
			{
				throw new ApplicationException("Parameter specification is not in form '<channelName>.<paramName>'.");
			}
			
			string dest = s[0];
			string par = s[1];
			
			// NullReferenceException is thrown if there is no channel with the specified name
            setter = MyParamSetter(par);
            if (setter == null) setter = channels.Find(ch => ch.Name==dest).ParamSetter(par);

            if (setter == null)
            {
                throw new System.ApplicationException("Parameter '" + par + "' does not exist for channel '" + dest + "'.");
            }
			
			return setter;
		}

        Action<float> MyParamSetter(string paramName)
        {
            Action<float> setter = null;
            //switch (paramName)
            //{
            //    case "Balance":
            //        setter = x => this.balance = x;
            //        break;
            //}

            return setter;
        }

        public string SetParameter(string chanName, string paramName, float value)
        {
            string error = "";

            Channel ch = channels.Find(c => c.Name == chanName);
            if (ch == null)
            {
                error = "Channel not found: " + chanName;
                return error;
            }

            return ch.SetParameter(paramName, value);
        }

        public float GetParameter(string chanName, string paramName)
        {
            Channel ch = channels.Find(c => c.Name == chanName);
            if (ch == null)
            {
                return float.NaN;
            }

            return ch.GetParameter(paramName);
        }

        public void ResetSweepables()
        {
            foreach (Channel ch in _shadows)
                ch.ResetSweepables();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Fs"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        public bool Initialize(float Fs, int N)
        {
            SamplingRate_Hz = Fs;
            Npts = N;

            return Initialize();
        }

        public bool Initialize()
        {
            bool success = true;
            string lastChannel = "";
            LastErrorMessage = "";
            LastException = null;
            _numCalls = 0;
            _paused = true;
            _pausePending = false;
            _unpausePending = false;
            _dt = (float)Npts / SamplingRate_Hz;

            CalibrationFactory.DefaultFolder = CalibrationFolder;
            UserFile.WavFolder = WavFolder;

            if (AdapterMap == null)
            {
                LastErrorMessage = "No adapter map specified.";
                throw new Exception("No adapter map specified");
            }

            try
            {
                _shadows.Clear();
                foreach (Channel ch in channels)
                {
                    lastChannel = ch.Name;
                    ch.ContraSide = null;

                    for (int k = 0; k < ch.NumCopies; k++)
                    {
                        Channel c = ch; 
                        if (k == 1)
                        {
                            c = Channel.FromProtoBuf(ch.ToProtoBuf());
                            ch.ContraSide = c;
                            c.Name += " (contra)";
                            c.level.Name = c.Name;
                            c.waveform.ChannelName = c.Name;
                            ch.waveform.ChannelName = ch.Name;
                        }

                        ch.SetEndpoint(k);

                        c.outputNum = AdapterMap.GetAdapterIndex(c.Modality.ToString(), c.GetLocation(k));
                        c.Vmax = Vmax;

                        _shadows.Add(c);
                    }
                }

                foreach (Channel ch in _shadows)
                {
                    lastChannel = ch.LongName;
                    ch.Initialize(SamplingRate_Hz, Npts);
                }

                foreach (Channel ch in channels)
                {
                    if (ch.Laterality == Laterality.Binaural)
                    {
                        ch.ApplyBinauralProperties();
                    }
                }
                _currentAmplitudes = new float[_shadows.Count];
            }
            catch (Exception ex)
            {
                //LastErrorMessage = "Initializing " + lastChannel + " " + ":\n" + ex.Message;
                LastErrorMessage = ex.Message;
                throw new Exception(LastErrorMessage);
            }
			return success;
        }

        public void SetTimeout(float timeout)
        {
            _maxCalls = Mathf.CeilToInt(timeout * SamplingRate_Hz / (float)Npts);
            _numCalls = 0;
            //Debug.Log("timeout = " + timeout + " (" + _maxCalls + " calls)");
        }

        public bool TimedOut
        {
            get { return _maxCalls > 0 && _numCalls >= _maxCalls; }
        }

        public float ElapsedTime
        {
            get { return _numCalls * _dt; }
        }

        public void ClearExpertOptions()
        {
            foreach (var ch in channels) ch.ClearExpertOptions();
        }

        public void Pause()
        {
            _unpausePending = false;
            _pausePending = true;
        }

        public void StartPaused()
        {
            _unpausePending = false;
            _pausePending = false;
            _paused = true;
        }

        public void Unpause()
        {
            _pausePending = false;
            _unpausePending = true;
        }

        public bool Synth { set; get; }
        public void Synthesize(float[] data)
        {
            string lastChannel = "";

            try
            {
//                ++_numCalls;

                int idx = 0;
                for (int k = 0; k < Npts; k++)
                {
                    for (int j = 0; j < _shadows.Count; j++)
                    {
                        data[idx + _shadows[j].outputNum] = 0;
                    }
                    idx += Noutputs;
                }

                if (!_unpausePending && _paused) return;

                Synth = true;
                for (int kch=0; kch<_shadows.Count; kch++)
                {
                    var ch = _shadows[kch];
                    float maxVal = 0;

                    lastChannel = ch.LongName;

                    ch.Create();

                    if (_pausePending && !_paused)
                    {
                        Gate.RampDown(ch.Data);
                    }
                    if (_unpausePending && _paused)
                    {
                        Gate.RampUp(ch.Data);
                    }

                    if (ch.Laterality == Laterality.Diotic)
                    {
                        idx = 0;
                        for (int k = 0; k < Npts; k++)
                        {
                            data[idx + ch.outputNum] += ch.Data[k] * ch.ALeft;
                            data[idx + ch.outputNum + 1] += ch.Data[k] * ch.ARight;
                            idx += Noutputs;
                            maxVal = (maxVal > ch.Data[k]) ? maxVal : ch.Data[k];
                        }
                    }
                    else
                    {
                        idx = ch.outputNum;
                        for (int k = 0; k < Npts; k++)
                        {
                            data[idx] += ch.Data[k];
                            idx += Noutputs;
                            maxVal = (maxVal > ch.Data[k]) ? maxVal : ch.Data[k];
                        }
                    }
                    _currentAmplitudes[kch] = maxVal;
                }
                Synth = false;

                if (_pausePending && !_paused)
                {
                    for (int kch = 0; kch < _currentAmplitudes.Length; kch++) _currentAmplitudes[kch] = 0;
                    _pausePending = false;
                    _paused = true;
                }

                if (_unpausePending && _paused)
                {
                    _unpausePending = false;
                    _paused = false;
                }
                ++_numCalls;
            }
            catch (Exception ex)
            {
                LastErrorMessage = "";
                if (!string.IsNullOrEmpty(Name))
                {
                    LastErrorMessage = Name + " -> ";
                }

                LastErrorMessage += lastChannel + ":\n" + ex.Message;
                LastException = new Exception(LastErrorMessage);
                throw LastException;
            }
        }

        public void Synthesize(float[,] data)
        {
            string lastChannel = "";

            try
            {
                if (!_unpausePending && _paused) return;

                Synth = true;
                foreach (Channel ch in _shadows)
                {
                    lastChannel = ch.LongName;

                    ch.Create();

                    if (_pausePending && !_paused)
                    {
                        Gate.RampDown(ch.Data);
                    }
                    if (_unpausePending && _paused)
                    {
                        Gate.RampUp(ch.Data);
                    }

                    if (ch.Laterality == Laterality.Diotic)
                    {
                        for (int k = 0; k < Npts; k++)
                        {
                            data[ch.outputNum, k] += ch.Data[k] * ch.ALeft;
                            data[ch.outputNum + 1, k] += ch.Data[k] * ch.ARight;
                        }
                    }
                    else
                    {
                        for (int k = 0; k < Npts; k++)
                        {
                            data[ch.outputNum, k] += ch.Data[k];
                        }
                    }
                }
                Synth = false;

                if (_pausePending && !_paused)
                {
                    _pausePending = false;
                    _paused = true;
                }

                if (_unpausePending && _paused)
                {
                    _unpausePending = false;
                    _paused = false;
                }
                ++_numCalls;
            }
            catch (Exception ex)
            {
                LastErrorMessage = "";
                if (!string.IsNullOrEmpty(Name))
                {
                    LastErrorMessage = Name + " -> ";
                }

                LastErrorMessage += lastChannel + ":\n" + ex.Message;
                LastException = new Exception(LastErrorMessage);
                throw LastException;
            }
        }

        public AudioClip CreateClip()
        {
            return CreateClip("clip");
        }
        public AudioClip CreateClip(string name)
        {
            ResetSweepables();

            float[] data = new float[Noutputs * Npts];
            Synthesize(data);

            AudioClip clip = AudioClip.Create(name, Npts, Noutputs, (int)SamplingRate_Hz, false);

            clip.SetData(data, 0);

            return clip;
        }

        /// <summary>
        /// Returns a time vector (in ms) for the current sampling rate and buffer size (convenient for plotting)
        /// </summary>
        /// <param name="t0">Vector start time (ms). Useful for concatenating buffers.</param>
        /// <returns></returns>
        public float[] GetTimeVector()
        {
            float dt = 1000 / SamplingRate_Hz;
            float[] t = new float[Npts];
            float cur_t = 0;
            for (int k = 0; k < Npts; k++)
            {
                t[k] = cur_t;
                cur_t += dt;
            }

            return t;
        }

    }
}
