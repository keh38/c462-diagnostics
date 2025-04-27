using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;
using KLib.Signals.Modulations;
using KLib.Signals.Waveforms;

namespace KLib.Signals
{
    class WaveformException : System.ApplicationException { }

    [Serializable]
    public enum Laterality
    {
        None,
        Left,
        Right,
        Binaural,
        Diotic
    }

/// <summary>
/// Audio channel class.
/// </summary>
[Serializable]
	[ProtoContract]
	[JsonObject(MemberSerialization.OptIn)]
	public class Channel
    {
		/// <summary>
		/// Context-specific name, e.g. "Signal" or "Masker"
		/// </summary>
		[ProtoMember(1, IsRequired = true)]
		[JsonProperty]
		public String Name { get; set; }

		[ProtoMember(2, IsRequired = true)]
		[JsonProperty]
		public Waveform waveform;
        
		[ProtoMember(3, IsRequired = true)]
		[JsonProperty]
		public Gate gate;

        [ProtoMember(4, IsRequired = true)]
        [JsonProperty]
        public Level level;

        [ProtoMember(5, IsRequired = true)]
        [JsonProperty]
        public AM modulation;

        [ProtoMember(6, IsRequired = true)]
        [JsonProperty]
        [XmlIgnore]
        public bool active;

        [ProtoMember(7, IsRequired = true)]
        [JsonProperty]
        public Laterality Laterality { set; get; } = Laterality.None;

        [ProtoMember(9, IsRequired = true)]
        public Modality Modality { set; get; } = Modality.Audio;

        [ProtoMember(10, IsRequired = true)]
        public string Location { set; get; }

        [ProtoMember(11, IsRequired = true)]
        [JsonProperty]
        public BinauralProperties binaural = new BinauralProperties();

        [JsonProperty]
        public string intramural = "";

        [ProtoIgnore]
        [XmlIgnore]
        public float Vmax = 1;

        private bool _gateClosed;
        private bool _deactivated;
        private int _nptsPerInterval;
        private int _nptsDelay;

        internal class IntramuralVariable
        {
            public string parameter;
            public float[] values;
            public int index;
            public IntramuralVariable(string parameter, float[] values)
            {
                this.parameter = parameter;
                this.values = values;
                index = 0;
            }
            public float GetNext()
            {
                float val = values[index++];
                if (index == values.Length) index = 0;
                return val;
            }
        }
        private List<IntramuralVariable> _intramuralVariables = new List<IntramuralVariable>();

        [ProtoIgnore]
        [XmlIgnore]
        public float SamplingRateHz { set; get; }

        [ProtoIgnore]
        [XmlIgnore]
        public SinusoidalAM SAM { get { return modulation as SinusoidalAM; } }

        [ProtoIgnore]
        [XmlIgnore]
        public Digitimer Digitimer { get { return waveform as Digitimer; } }

        [ProtoIgnore]
        [XmlIgnore]
        public AdapterMap.Endpoint MyEndpoint { get; set; }

        [ProtoIgnore]
        [XmlIgnore]
        public int OutputNum { get { return MyEndpoint.index; } }

        [JsonIgnore]
        [XmlIgnore]
        public float[] Data { get; private set; }

        [ProtoIgnore]
        [XmlIgnore]
        public int LoopOffset { get { return gate.Active ? gate.LoopOffset : -1; } }

        [ProtoIgnore]
        [XmlIgnore]
        public bool IsFinished { get { return gate.Active ? gate.State == GateState.Finished : false; } }

        [ProtoIgnore]
        [XmlIgnore]
        public string LongName { get { return Name + (MyEndpoint != null ? " (" + MyEndpoint + ")" : ""); } }

        [ProtoIgnore]
        [XmlIgnore]
        public float ALeft { get; private set; }

        [ProtoIgnore]
        [XmlIgnore]
        public float ARight { get; private set; }

        [ProtoIgnore]
        [XmlIgnore]
        public Channel ContraSide { set; get; }

        [ProtoIgnore]
        [XmlIgnore]
        public int NumCopies { get { return (Laterality == Laterality.Binaural) ? 2 : 1; } }

        int _numPts;
        private RampState _rampState;

        public Channel()
		{
            waveform = new Waveform();
            gate = new Gate();
            level = new Level();
            modulation = new AM();

            _numPts = 0;

            active = true;
		}

        /// <summary>
        /// 
        /// </summary>
        public Channel(Waveform wf)
        {
            waveform = wf ?? new Waveform();
            gate = new Gate();
            level = new Level();
			modulation = new AM();

            SamplingRateHz = -1;
            _numPts = 0;

            active = true;
        }

        public Channel(string name) : this(name, null)
        {
        }

		public Channel(string name, Waveform wf):this(wf)
		{
			this.Name = name;
            this.level.Name = name;
		}
		
		public Channel(string name, Waveform wf, SinusoidalAM modul):this(wf)
		{
			this.Name = name;
			this.modulation = modul;
		}

        public void SetEndpoint(AdapterMap.Endpoint endpoint, int copyNum)
        {
            MyEndpoint = endpoint;
            MyEndpoint.location = GetLocation(copyNum);
        }

        public string GetLocation(int copyNum)
        {
            string location = "";

            if (Modality == Modality.Audio)
            {
                if (Laterality == Laterality.None)
                {
                    location = "";
                }
                else if (Laterality == Laterality.Left)
                    location = "Left";
                else if (Laterality == Laterality.Right)
                    location = "Right";
                else
                {
                    location += (copyNum == 0) ? "Left" : "Right";
                }
            }
            else if (Modality == Modality.Haptic || Modality == Modality.Electric)
            {
                location = Location;
            }
            return location;
        }


        public void SetActive(bool active)
        {
            this.active = active;
            if (active)
                this.gate.Reset();

            if (ContraSide != null)
            {
                ContraSide.active = active;
                if (active)
                    ContraSide.gate.Reset();
            }
        }

        public void ClampLevelToMax(bool clamp)
        {
            level.ClampToMax = clamp;
            if (ContraSide != null)
            {
                ContraSide.level.ClampToMax = clamp;
            }
        }

        public byte[] ToProtoBuf()
        {
            byte[] pbuf;
            using (var ms = new System.IO.MemoryStream())
            {
                Serializer.Serialize<Channel>(ms, this);
                pbuf = ms.ToArray();
            }
            return pbuf;
        }

        public static Channel FromProtoBuf(byte[] pbuf)
        {
            Channel ch = null;
            using (var ms = new System.IO.MemoryStream(pbuf))
            {
                ch = Serializer.Deserialize<Channel>(ms);
            }

            return ch;
        }

        public List<string> GetSweepableParams()
        {
            var sp = new List<string>();

            if (waveform.Shape != Waveshape.None)
            {
                foreach (string p in waveform.GetSweepableParams())
                {
                    sp.Add(waveform.ShortName + "." + p);
                }
                sp.AddRange(modulation.GetSweepableParams());
                sp.AddRange(gate.GetSweepableParams());

                var digitimer = waveform as Digitimer;
                if (digitimer == null || digitimer.Source == Digitimer.DemandSource.External)
                {
                    sp.AddRange(Level.GetSweepableParams());
                }
                if (Laterality == Laterality.Binaural)
                {
                    sp.Add("MBL");
                    sp.Add("ILD");
                    if (waveform.Shape == Waveshape.Sinusoid)
                    {
                        sp.Add("IPD");
                    }
                }
            }
            return sp;
        }

        public Action<bool> GetActiveSetter()
        {
            return x => SetActive(x);
        }

		public Action<float> GetParamSetter(string paramName)
		{
            Action<float> setter = null;

            string[] s = paramName.Split('.');
            if (s.Length != 2 && s[0] != "Level")
            {
                return null;
            }

            switch (s[0])
            {
                case "Gate":
                    setter = gate.GetParamSetter(s[1]);
                    break;

                case "Mod":
                case "SAM":
                    setter = modulation.GetParamSetter(s[1]);
                    break;

                case "Level":
                    setter = level.GetParamSetter("Level");
                    break;

                default:
                    setter =  waveform.GetParamSetter(s[1]);
                    break;
            }


            if (setter != null && Laterality == Laterality.Binaural && ContraSide != null)
            {
                setter += ContraSide.GetParamSetter(paramName);
                return setter;
            }

            if (setter == null)
            {
                switch (s[0])
                {
                    case "MBL":
                        setter = x => this.ChangeMBL(x);
                        break;
                    case "ILD":
                        setter = x => this.ChangeILD(x);
                        break;
                    case "IPD":
                        setter = x => this.ChangeIPD(x);
                        break;
                }

            }

            return setter;
		}

        public string SetParameter(string paramName, float value)
        {
            string error = "";

            string[] s = paramName.Split(new char[] { '.' });
            if (s.Length != 2 && s[0] != "Level" && s[0] != "Ear")
            {
                error = Name + ": invalid parameter: " + paramName;
                return error;
            }

            switch (s[0])
            {
                case "Ear":
                    //Destination = (Laterality)(value + 2);
                    break;

                case "Gate":
                    error = gate.SetParameter(s[1], value);
                    break;

                case "Mod":
                case "SAM":
                    error = modulation.SetParameter(s[1], value);
                    break;

                case "Level":
                    if (s.Length > 1)
                    {
                        level.Rove = value;
                    }
                    else
                    {
                        level.Value = value;
                    }
                    break;

                case "MBL":
                    binaural.MBL = value;
                    break;

                case "ILD":
                    binaural.ILD = value;
                    break;

                case "IPD":
                    binaural.IPD = value;
                    break;

                default:
                    error = waveform.SetParameter(s[1], value);
                    break;
            }

            if (error=="" && Laterality == Laterality.Binaural && ContraSide != null)
            {
                ContraSide.SetParameter(paramName, value);
            }

            return error;
        }

        public float GetParameter(string paramName)
        {
            string[] s = paramName.Split('.');
            if (s.Length != 2 && s[0] != "Level" && s[0] != "Ear")
            {
                return float.NaN;
            }

            switch (s[0])
            {
                case "Ear":
                    //Destination = (Laterality)(value + 2);
                    break;

                case "Gate":
                    return gate.GetParameter(s[1]);

                case "Mod":
                case "SAM":
                    return modulation.GetParameter(s[1]);

                case "Level":
                    return level.Value;

                case "MBL":
                    return binaural.MBL;

                case "ILD":
                    return binaural.ILD;

                case "IPD":
                    return binaural.IPD;

                default:
                    return waveform.GetParameter(s[1]);
            }

            return float.NaN;
        }
        
        public List<string> GetValidParameters()
        {
            List<string> plist = new List<string>();

            foreach (string p in waveform.GetValidParameters())
            {
                plist.Add(waveform.ShortName + "." + p);
            }
            plist.AddRange(gate.GetValidParameters());
            plist.AddRange(modulation.GetValidParameters());
            plist.AddRange(level.GetValidParameters());
            plist.Add("Ear");

            if (Laterality == Laterality.Binaural)
            {
                plist.Add("MBL");
                plist.Add("ILD");
                if (waveform is Sinusoid) plist.Add("IPD");
            }


            return plist;
        }

        public void ResetSweepables()
        {
            active = waveform != null; // active gets reset on one-shot?!?
            _gateClosed = false; // --> better this way

            waveform.ResetSweepables();
    
            if (modulation != null)
                modulation.ResetSweepables();

            level.ResetSweepables();
            gate.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Fs"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        public void Initialize(float Fs, int N)
        {
            string location = "";

            try
            {
                SamplingRateHz = Fs;
                _numPts = N;

                Data = new float[N];

                location = "modulation";
                modulation.Initialize(Fs, N);

                location = "gate";
                gate.Initialize(Fs, N);
                _gateClosed = false;

                _nptsDelay = Mathf.RoundToInt(Fs * gate.Delay_ms / 1000);
                _nptsPerInterval = gate.Active ? Mathf.RoundToInt(Fs * gate.Period_ms / 1000) : -1;

                location = "level";
                var digitimer = waveform as Digitimer;
                if (digitimer != null && digitimer.Source == Digitimer.DemandSource.Internal)
                {
                    level.Units = LevelUnits.Volts;
                    level.Value = 1;
                }

                level.Initialize(MyEndpoint, Vmax);

                _rampState = RampState.Off;

                location = "waveform";
                waveform.Initialize(Fs, N, this);

                if (Laterality == Laterality.Diotic)
                {
                    location = "balance";
                    SetBalance();
                }
                _deactivated = false;

                _intramuralVariables.Clear();
            }
            catch (Exception ex)
            {
                string errorMessage = LongName + " " + location + ":\n" + ex.Message;
                throw new Exception(errorMessage);
            }
        }

        public float GetMaxLevel()
        {
            try
            {
                if (level.Units == LevelUnits.dB_attenuation)
                {
                    return 0;
                }
                else if (level.Units == LevelUnits.mA)
                {
                    level.Initialize(MyEndpoint, Vmax);
                    return level.GetMaxLevel(level.Units);
                }

                float maxValue = float.PositiveInfinity;

                for (int k=0; k<NumCopies; k++)
                {
                    //SetEndpoint(k);
                    level.Initialize(MyEndpoint, Vmax);
                    float mx = waveform.GetMaxLevel(level, SamplingRateHz);
                    mx += modulation.LevelCorrection;
                    maxValue = Mathf.Min(maxValue, mx);
                }
                return maxValue + 20 * Mathf.Log10(Vmax);
            }
            catch (Exception)
            {
                return float.NaN;
            }
        }

        [JsonIgnore]
        public float ConvertSLToSPL
        {
            get { return level.SL_to_SPL; }
        }

        public void ApplyBinauralProperties()
        {
            if (Laterality == Laterality.Binaural)
            {
                SetBinauralCoherence();
                return;
            }

            if (ContraSide == null)
                throw new System.Exception("Cannot apply binaural properties. Channel is not stereo.");

            if (binaural.balance != 0 && binaural.ILD != 0)
                throw new System.Exception("Balance and ILD cannot both be nonzero");

            SetBinauralLevel();
            SetBinauralBalance();
            SetBinauralPhase();
            SetBinauralCoherence();

            ResetSweepables();
            ContraSide.ResetSweepables();
        }

        public void ChangeMBL(float value)
        {
            if (Laterality == Laterality.Binaural && ContraSide != null)
            {
                binaural.MBL = value;
                SetBinauralLevel();
            }
        }

        public void ChangeILD(float value)
        {
            if (Laterality == Laterality.Binaural && ContraSide != null)
            {
                binaural.ILD = value;
                SetBinauralLevel();
            }
        }

        public void ChangeIPD(float value)
        {
            if (Laterality == Laterality.Binaural && ContraSide != null)
            {
                binaural.IPD = value;
                SetBinauralPhase();
            }
        }

        public void ChangeBalance(float value)
        {
            if (Laterality == Laterality.Binaural && ContraSide != null)
            {
                binaural.balance = value;
                SetBinauralBalance();
            }
        }

        private void SetBinauralLevel()
        {
            if (Laterality == Laterality.Binaural && ContraSide != null)
            {
                level.Value = binaural.MBL - binaural.ILD / 2;
                ContraSide.level.Value = binaural.MBL + binaural.ILD / 2;
            }
        }

        private void SetBinauralBalance()
        {
            if (Laterality == Laterality.Binaural && ContraSide != null)
            {
                float Aipsi = 1;
                float Acontra = 1;
                if (binaural.balance > 0)
                {
                    Acontra = 1;
                    Aipsi = 1 - binaural.balance;
                }
                else
                {
                    Aipsi = 1;
                    Acontra = 1 + binaural.balance;
                }

                level.Value = binaural.MBL + 20 * Mathf.Log10(Aipsi);
                ContraSide.level.Value = binaural.MBL + 20 * Mathf.Log10(Acontra);
            }
        }

        private void SetBinauralPhase()
        {
            if (Laterality == Laterality.Binaural && ContraSide != null && waveform.Shape == Waveshape.Sinusoid)
            {
                (waveform as Sinusoid).Phase_cycles = -binaural.IPD / 2;
                (ContraSide.waveform as Sinusoid).Phase_cycles = binaural.IPD / 2;
            }
        }

        private void SetBinauralCoherence()
        {
            if (Laterality == Laterality.Binaural && ContraSide != null && waveform.Shape == Waveshape.Noise)
            {
                int seed = (waveform as Noise).seed;
                if (seed <= 0)
                {
                    var rng = new System.Random();
                    seed = (int)(int.MaxValue * rng.NextDouble());
                    (waveform as Noise).SetSharedSeed(seed);
                }

                (ContraSide.waveform as Noise).SetSharedSeed(seed);
            }
        }

        public void SetBalance()
        {
            if (binaural.balance > 0)
            {
                ARight = 1;
                ALeft = 1 - binaural.balance;
            }
            else
            {
                ALeft = 1;
                ARight = 1 + binaural.balance;
            }
        }

        public void Create()
        {
            References references;
            float modRefCorr = 0;


            for (int k = 0; k < Data.Length; k++)
            {
                Data[k] = 0;
            }

            if (waveform == null)
            {
                return;
            }

            if (_gateClosed && _rampState == RampState.Off)
            {
                return;
            }

            if (float.IsInfinity(level.Value))
            {
                return;
            }

            references = waveform.Create(Data);
            if (!waveform.HandlesModulation)
            {
                modRefCorr = modulation.Apply(Data);
            }                    

            if (gate.Active)
            {
                gate.Apply(Data);

                //if (!gate.Running) // so a one-shot will turn the channel off automatically
                //{
                //    //_gateClosed = true;
                //}
                _rampState = (gate.Running) ? RampState.On : RampState.Off;


                //if (_intramuralVariables.Count > 0 && gate.Looped)
                //{
                //    UpdateIntramurals();
                //}

                //if (!active && _rampState == RampState.Off)
                //{
                // //   gate.Reset();
                //}
            }
            else
            {
                _rampState = active ? RampState.On : RampState.Off;
            }

            _rampState = active ? _rampState : RampState.Off;

            if (active && !_deactivated)
            {
                level.Apply(Data, references.Offset(modRefCorr));
            }
            else if (!active && !_deactivated)
            {
                level.Apply(Data, references.Offset(modRefCorr));
                Gate.RampDown(Data);
                _deactivated = true;
            }
            else if (active && _deactivated)
            {
                level.Apply(Data, references.Offset(modRefCorr));
                Gate.RampUp(Data);
                _deactivated = false;
            }
            else if (!active && _deactivated)
            {
                for (int k = 0; k < Data.Length; k++) Data[k] = 0;
            }
        }

        public void ClearExpertOptions()
        {
            intramural = "";
            if (level.Units == LevelUnits.dB_SPL_noLDL) level.Units = LevelUnits.dB_SPL;
        }

#if FIXME
        public void InitializeIntramural(List<Turandot.Flag> flags)
        {
            _intramuralVariables.Clear();

            if (string.IsNullOrEmpty(intramural)) return;
            if (!gate.Active || float.IsPositiveInfinity(gate.Period_ms)) return;

            string[] subExpr = intramural.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var e in subExpr)
            {
                string[] exprParts = e.Split(new char[] { '=' });
                string expression = exprParts[1];
                foreach (var f in flags)
                {
                    expression = expression.Replace("{" + f.name + "}", f.value.ToString());
                }

                Debug.Log("IM: " + expression);
                _intramuralVariables.Add(new IntramuralVariable(exprParts[0], Expressions.Evaluate(expression)));
            }
            UpdateIntramurals();
        }
#endif
        public float MaxIntramuralTime
        {
            get
            {
                int maxNum = 0;
                foreach (var iv in _intramuralVariables) maxNum = Mathf.Max(maxNum, iv.values.Length);
                return maxNum * gate.Period_ms / 1000;
            }
        }

        private void UpdateIntramurals()
        {
            foreach (var iv in _intramuralVariables)
            {
                SetParameter(iv.parameter, iv.GetNext());
            }
        }
    }
}
