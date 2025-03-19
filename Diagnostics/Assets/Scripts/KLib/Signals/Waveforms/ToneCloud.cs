using UnityEngine;
using System;
using System.Collections.Generic;

using KLib;
using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;

namespace KLib.Signals.Waveforms
{
    /// <summary>
    /// Synthesizes overlapping tone pips of varying frequency.
    /// </summary>
    [System.Serializable]
	[ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
	[JsonObject(MemberSerialization.OptOut)]
	public class ToneCloud : Waveform
    {
        /// <summary>
        /// Duration of individual tone pips (ms). 
        /// </summary>
        /// <remarks>
        /// Default = 30 ms.
        /// <note>Cannot be changed dynamically.</note>
        /// </remarks>
        [ProtoMember(1, IsRequired = true)]
        public float PipDuration_ms;

        /// <summary>
        /// Length of cos-squared ramp applied to each tone pip.
        /// </summary>
        /// <remarks>Default = 5 ms.
        /// <note>Cannot be changed dynamically.</note>
        /// </remarks>
        [ProtoMember(2, IsRequired = true)]
        public float PipRamp_ms;

        /// <summary>
        /// Rate at which new tone pips are generated.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Offset (in ms) between tone pip onsets is 1000/PipRate_Hz.</item>
        /// <item>Default = 100 Hz</item>
        /// </list>
        /// <note>Cannot be changed dynamically.</note>
        /// </remarks>
        [ProtoMember(3, IsRequired = true)]
        public float PipRate_Hz;

        /// <summary>
        /// Mean tone cloud frequency (Hz).
        /// </summary>
        /// <remarks>
        /// <list>
        /// <item>Default = 2000 Hz</item>
        /// </list>
        /// <note>Can be set dynamically</note>
        /// </remarks>
        [ProtoMember(4, IsRequired = true)]
        public float Fmean_Hz;

        /// <summary>
        /// Bandwidth of tone cloud (octaves re Fmean_Hz)
        /// </summary>
        /// <list>
        /// <item>Default = 1 octave</item>
        /// </list>
        /// <note>Can be set dynamically</note>
        /// </remarks>
        [ProtoMember(5, IsRequired = true)]
        public float Bandwidth;
        [ProtoMember(6, IsRequired = true)]
        public BWUnits bwUnits = BWUnits.Octaves;

        /// <summary>
        /// Sharpness of tone cloud frequency distribution (octaves)
        /// </summary>
        /// <remarks>
        /// <list>
        /// <item>Default = 0.5</item>
        /// </list>
        /// <note>Can be set dynamically</note>
        /// </remarks>
        [ProtoMember(7, IsRequired = true)]
        public float Fsigma;

        /// <summary>
        /// Frequency resolution of lookup table (Hz)
        /// </summary>
        /// <remarks>
        /// <list>
        /// <item>Default = 1 Hz</item>
        /// </list>
        /// </remarks>
        [ProtoMember(8, IsRequired = true)]
        public float FrequencyRes_Hz;

        [ProtoMember(9, IsRequired = true)]
        public bool roveLevel;
        [ProtoMember(10, IsRequired = true)]
        public float roveRange;
        [ProtoMember(11, IsRequired = true)]
        public float roveSigma;

        // Cloud components
        private int numComponents;

        private float actualMeanFreqHz;
        private bool isConstrained;
        private float[] constrainedFreqs;

        private struct CloudComponent {
            public int sinIndex;
            public int sinSkip;
            public int envIndex;
            public float amplitude;
        };

        private CloudComponent[] components;
        private float[] Amax;
        private static readonly KLib.Math.TruncatedNormalRandom randt = new KLib.Math.TruncatedNormalRandom();
        private float scaleFactor;

        // LUT params
        private int SinTableLength;
        private float[] SinLUT;

        private float[] envelopeLUT;

        /// <summary>
        /// Constructs default ToneCloud object.
        /// </summary>
        public ToneCloud()
        {
			shape = Waveshape.Tone_Cloud;
            _shortName = "cloud";

            PipDuration_ms = 30;
            PipRamp_ms = 1;
            PipRate_Hz = 100;

            Fmean_Hz = 2000;
            Bandwidth = 1.0f;
            Fsigma = 0.5f;

            FrequencyRes_Hz = 1;

            isConstrained = false;
            roveLevel = false;
            //randt = new TruncatedNormalRandom();
        }
		
		new public static List<SweepableParam> GetSweepableParams()
		{
			List<SweepableParam> par = new List<SweepableParam>();
			par.Add(new SweepableParam("Cloud Mean", "Hz", 500));
            par.Add(new SweepableParam("Cloud BW", "ERB", 0.5f));
            par.Add(new SweepableParam("Cloud Sigma", "ERB", 0.5f));

			return par;
		}

		override public Action<float> ParamSetter(string paramName)
		{
			Action<float> setter = null;
			switch (paramName)
			{
        		case "Cloud Mean":
        			setter = x => this.Fmean_Hz = x;
        			break;
                case "Cloud BW":
                    setter = x => this.Bandwidth = x;
                    break;
                case "Cloud Sigma":
                    setter = x => this.Fsigma = x;
                    break;
            }
			
			return setter;
		}

        override public void ResetSweepables()
        {
            float Toffset = 1000.0f / PipRate_Hz;
            int numOffset = Mathf.RoundToInt(samplingRate_Hz * Toffset / 1000);
            for (int k = 0; k < numComponents; k++)
            {
                components[k].envIndex = -k * numOffset;
                
            }
        }

        [ProtoIgnore]
        [JsonIgnore]
        public float ActualMeanFreqHz
        {
            get { return actualMeanFreqHz;}
        }
        
        [ProtoIgnore]
        [JsonIgnore]
        public float[] ActualFreqsHz
        {
            get { return constrainedFreqs;}
        }
        
        [ProtoIgnore]
        [JsonIgnore]
        public bool ConstrainFreqs
        {
            set { isConstrained = value;}
        }

        /// <summary>
        /// Initialize ToneCloud.
        /// </summary>
        /// <param name="Fs">sampling rate (Hz)</param>
        /// <param name="N">number of points per buffer</param>
        /// <returns>Returns true if successfully initialized.</returns>
        /// <remarks>
        /// 
        /// </remarks>
        override public void Initialize(float Fs, int N, Channel channel)
        {
            base.Initialize(Fs, N, channel);


            float Toffset = 1000.0f / PipRate_Hz;
            int numOffset = (int)Mathf.Round(Fs * Toffset / 1000);

            numComponents = (int) Mathf.Ceil(PipDuration_ms / Toffset);
            float Tgate = (float)numComponents * Toffset;

            // Create sin lookup table (LUT)
            SinTableLength = (int)(Fs / FrequencyRes_Hz);
            SinLUT = new float[SinTableLength];
            for (int k = 0; k < SinTableLength; k++) SinLUT[k] = (float)Mathf.Sin(2 * Mathf.PI * (float)k / (float)SinTableLength);

            // Create envelope LUT
            Gate gate = new Gate(0, PipDuration_ms, PipRamp_ms);
            envelopeLUT = gate.Create(Fs, Tgate);

            components = new CloudComponent[numComponents];
            Amax = new float[numComponents];
            for (int k = 0; k < numComponents; k++)
            {
                components[k].envIndex = -k * numOffset;
                Amax[k] = float.PositiveInfinity;
            }

            scaleFactor = 1.0f / (float)numComponents;
        }

        public void ApplyFreqConstraint()
        {
            float erb = 0;
            if (bwUnits == BWUnits.ERB)
            {
                erb = 24.7f * (4.37f * Fmean_Hz/1000f + 1);
            }

            int numPips = Mathf.CeilToInt(PipRate_Hz * (float)Npts / samplingRate_Hz);
            int numBandWidthBins = numPips;
            float bandWidthBinSize = Bandwidth / (float)numBandWidthBins;
            int numPerBin = Mathf.CeilToInt((float)numPips / (float)numBandWidthBins);
            numPips = numPerBin * numBandWidthBins;
            float[] freqs = new float[numPips];

            int idx = 0;
            float bw_min = -Bandwidth / 2;
            for (int k=0; k<numBandWidthBins; k++)
            {
                for (int j=0; j<numPerBin; j++)
                {
                    if (Bandwidth == 0)
                    {
                        freqs[idx++] = Fmean_Hz;
                    }
                    else if (bwUnits == BWUnits.ERB)
                    {
                        freqs[idx++] = Fmean_Hz + randt.Next(bw_min*erb, (bw_min+bandWidthBinSize)*erb, 0, Fsigma*erb);
                    }
                    else
                    {
                        freqs[idx++] = Fmean_Hz * Mathf.Pow(2, randt.Next(bw_min, (bw_min+bandWidthBinSize), 0, Fsigma));
                    }
                }
                bw_min += bandWidthBinSize;
            }

            constrainedFreqs = KMath.Permute(freqs);
        }

       /// <summary>
       /// Create tone cloud buffer.
       /// </summary>
       /// <returns>New array containing tone cloud buffer.</returns>
//        override public float[] Create()
        override public References Create(float[] data)
        {
            int constrainedIndex = 0;
            actualMeanFreqHz = 0;
            int numNewComponents = 0;

            float erb = 0;
            if (bwUnits == BWUnits.ERB)
            {
                erb = 24.7f * (4.37f * Fmean_Hz / 1000f + 1);
            }

            for (int kc = 0; kc < numComponents; kc++)
            {
                for (int kt = 0; kt < Npts; kt++)
                {
                    // New tone pip: select frequency
                    if (components[kc].envIndex == 0)
                    {
                        float newFreq = 0;
                        components[kc].sinIndex = 0;
                        if (isConstrained)
                        {
                            newFreq = constrainedFreqs [constrainedIndex++];
                        }
                        else
                        {
                            if (Bandwidth == 0)
                            {
                                newFreq = Fmean_Hz;
                            }
                            else if (bwUnits == BWUnits.ERB)
                            {
                                newFreq = Fmean_Hz + randt.Next(-0.5f * Bandwidth * erb, 0.5f * Bandwidth * erb, 0, Fsigma * erb);
                            }
                            else
                            {
                                newFreq = Fmean_Hz * Mathf.Pow(2, randt.Next(-0.5f * Bandwidth, 0.5f * Bandwidth, 0, Fsigma));
                            }
                        }
                        actualMeanFreqHz += newFreq;
                        ++numNewComponents;

                        components[kc].sinSkip = Mathf.RoundToInt(newFreq / FrequencyRes_Hz);

                        float roveFactor = 1;
                        if (roveLevel)
                        {
                            roveFactor = Mathf.Pow(10, -randt.Next(0, roveRange, roveRange/2f, roveSigma)/20f);
                        }

                        components[kc].amplitude = roveFactor * _calib.GetAmplitude(newFreq);
                        Amax[kc] = _calib.GetMax(newFreq);
                    }

                    // Create component
                    if (components [kc].envIndex >= 0)
                    {
                        data[kt] += components [kc].amplitude * SinLUT [components[kc].sinIndex] * envelopeLUT[components[kc].envIndex];
                        components[kc].sinIndex += components [kc].sinSkip;
                        if (components[kc].sinIndex >= SinLUT.Length)
                            components[kc].sinIndex -= SinLUT.Length;
                    }

                    // Update envelope index
                    ++components [kc].envIndex;
                    if (components [kc].envIndex == envelopeLUT.Length)
                    {
                        components [kc].envIndex = 0;
                    }

                }
            }

            if (numNewComponents > 0)
            {
                actualMeanFreqHz /= (float)numNewComponents;
            }

            for (int kt = 0; kt < Npts; kt++)
            {
                data[kt] *= scaleFactor;
            }

            //
            // The reference is *per component*, e.g. dB SPL/component. Because we scale by the number of components,
            // the reference (*per component*) gets reduced accordingly.
            //
            // No such scaling is applied to the max value: we've done the bookkeeping such that we *will* get out
            // the level/component requested. We want to limit the max value in any one component.

            float scale_dB = 20 * Mathf.Log10(scaleFactor);
            return new References(_calib.Reference + scale_dB,
                                  KMath.Min(Amax));
        }
    }
}
