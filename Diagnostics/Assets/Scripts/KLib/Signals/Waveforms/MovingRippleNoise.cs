using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;

namespace KLib.Signals.Waveforms
{
    [System.Serializable]
	[ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
	[JsonObject(MemberSerialization.OptOut)]
    public class MovingRippleNoise : Waveform
    {
		public enum CreateMode {Trig, SweepVelocity, SweepDensity, SweepDepth, SweepBoth};

        [ProtoMember(1, IsRequired = true)]
        public float Fmin;
        [ProtoMember(2, IsRequired = true)]
        public float Fmax;
        [ProtoMember(3, IsRequired = true)]
        public int CompPerOctave;
        [ProtoMember(4, IsRequired = true)]
        public float Depth;
        [ProtoMember(5, IsRequired = true)]
        public float RippleDensity;
        [ProtoMember(6, IsRequired = true)]
        public float RippleVelocity;
        [ProtoMember(7, IsRequired = true)]
        public float InitialPhase;
        [ProtoMember(8, IsRequired = true)]
        public bool Weighted;

        [ProtoIgnore]
		//[JsonIgnore]
		public CreateMode createMode = CreateMode.SweepVelocity;

		[ProtoIgnore]
		//[JsonIgnore]
		public float FrequencyRes;

        private int numComponents;

        private float[] theta;
        private float[] phi;

        private float lastVelocity;
        private float lastDensity;
		private float lastDepth;

        // LUT params
        private int tableLength;

        private int[] carrierIndex;
        private int[] carrierSkipSize;
        private int[] envelopeIndex;
        private float[] amplitudes;
        private float densityResolution;

        private float maxVal;

        private float scaleFactor;

        private float[] SinLUT;


        public MovingRippleNoise()
        {
			shape = Waveshape.MovingRippleNoise;
            _shortName = "Ripple";

			Fmin = 250;
            Fmax = 8000;
            CompPerOctave = 20;
            Depth = 0.9f;
            RippleDensity = 0.4f;
            RippleVelocity = 2;
            InitialPhase = 0.75f;

            Weighted = false;

            FrequencyRes = 1;
        }

		new public static List<SweepableParam> GetSweepableParams()
		{
			List<SweepableParam> par = new List<SweepableParam>();
			par.Add(new SweepableParam("Ripple Velocity", "cycles/s", 2));
			par.Add(new SweepableParam("Ripple Density", "cycles/octave", 0.4f));
			par.Add(new SweepableParam("Ripple Depth", "(0-1)", 0.9f));

			return par;
		}

		override public Action<float> ParamSetter(string paramName)
		{
			Action<float> setter = null;
			switch (paramName)
			{
			case "Ripple Velocity":
				setter = x => this.RippleVelocity = x;
				break;
			case "Ripple Density":
				setter = x => this.RippleDensity = x;
				break;
			case "Ripple Depth":
				setter = x => this.Depth = x;
				break;
			}
			
			return setter;
		}

        override public void ResetSweepables()
        {
            lastVelocity = Mathf.Round(this.RippleVelocity / FrequencyRes) * FrequencyRes;
            lastDensity = Mathf.Round(RippleDensity / densityResolution) * densityResolution;
            lastDepth = Depth;
        }

        override public void Initialize(float Fs, int N, int Nmax, Level level)
        {
            base.Initialize(Fs, N, level);

            numComponents = (int)(Mathf.Round(Mathf.Log(Fmax / Fmin) / Mathf.Log(2) * CompPerOctave)) + 1;

            theta = new float[numComponents];
            for (int k = 0; k < numComponents; k++) theta[k] = UnityEngine.Random.Range(0f, 1f);

            phi = new float[numComponents];
            for (int kc = 0; kc < numComponents; kc++) phi[kc] = InitialPhase;

            amplitudes = new float[numComponents];

            maxVal = float.NegativeInfinity;

            float f = Fmin;
            float dx = 1.0f / CompPerOctave;
            float df = Mathf.Pow(2, dx);
            for (int kc=0; kc <numComponents; kc++)
            {
                amplitudes[kc] = _calib.GetAmplitude(f); // amplitude giving reference (= 100 dB SPL)

                maxVal = Mathf.Max(maxVal, _calib.GetMax(f));

                f *= df;
            }

            lastVelocity = RippleVelocity;
            lastDensity = RippleDensity;
			lastDepth = Depth;

            InitLUT(Fs, N);
        }

        public void InitLUT(float Fs, int N)
        {
//            base.Initialize(Fs, N);

            numComponents = (int)(Mathf.Round(Mathf.Log(Fmax / Fmin) / Mathf.Log(2) * CompPerOctave)) + 1;

            tableLength = (int)(Fs / FrequencyRes);
            SinLUT = new float [tableLength];
            for (int k = 0; k < tableLength; k++) SinLUT[k] = Mathf.Sin(2 * Mathf.PI * (float)k / (float)tableLength);

            carrierSkipSize = new int[numComponents];
            carrierIndex = new int[numComponents];
            envelopeIndex = new int[numComponents];

            float x = 0;
            float f = Fmin;
            float dx = 1.0f / CompPerOctave;
            float df = Mathf.Pow(2, dx);

            densityResolution = 1 / (dx * (float)tableLength);

            float cycles;

            for (int kc = 0; kc < numComponents; kc++)
            {
                cycles = RippleDensity * x;
                cycles -= Mathf.Round(cycles);
                if (cycles < 0) cycles += 1;

                envelopeIndex[kc] = (int)Mathf.Round(tableLength * (cycles + InitialPhase));

                if (envelopeIndex[kc] >= tableLength) envelopeIndex[kc] -= tableLength;

                carrierSkipSize[kc] = (int)Mathf.Round(f / FrequencyRes);

                carrierIndex[kc] = (int)(UnityEngine.Random.Range(0f, 1f) * (float)tableLength);
                if (carrierIndex[kc] >= tableLength) carrierIndex[kc] -= tableLength;

                x += dx;
                f *= df;
            }

            scaleFactor = 1.0f / (float)numComponents;
        }

        override public References Create(float[] data)
		{
			switch (createMode)
			{
			case CreateMode.Trig:
				CreateTrig(data);
				break;
			case CreateMode.SweepVelocity:
				CreateSweepVelocity(data);
				break;
			case CreateMode.SweepDensity:
				CreateSweepDensity(data);
				break;
			case CreateMode.SweepDepth:
				CreateSweepDepth(data);
				break;
			case CreateMode.SweepBoth:
				CreateSweepBoth(data);
				break;
			}

//            return calib.Reference + 20 * Mathf.Log10(scaleFactor);
            return new References(
                _calib.Reference - 10 * Mathf.Log10(numComponents),
                maxVal);
        }
		
		private void CreateSweepVelocity(float[] data)
		{
            RippleVelocity = Mathf.Round(RippleVelocity / FrequencyRes) * FrequencyRes;

			// How much to increment (decrement) the table step size every time step
            float deltaVel_dt = (RippleVelocity - lastVelocity) / (float)Npts;

            float arg1;

            float A;

            arg1 = lastVelocity;
            
            for (int kt = 0; kt < Npts; kt++)
            {
				int velocityShift = (int)Mathf.Round(arg1/FrequencyRes);
            	
	            for (int kc = 0; kc < numComponents; kc++)
                {
                    // create the current component sample
                    A = 1 + Depth * SinLUT[envelopeIndex[kc]];
                    data[kt] += amplitudes[kc] * A * SinLUT[carrierIndex[kc]];

                    // Update the carrier phase (index)
                    carrierIndex[kc] += carrierSkipSize[kc];
                    if (carrierIndex[kc] >= tableLength) carrierIndex[kc] -= tableLength;

                    // update the phase (index) for the next time sample
                    envelopeIndex[kc] += velocityShift;

                    // Phase wrapping
                    if (envelopeIndex[kc] >= tableLength) envelopeIndex[kc] -= tableLength;
                    if (envelopeIndex[kc] < 0) envelopeIndex[kc] += tableLength;
                }

				// increment the velocity argument
                arg1 += deltaVel_dt;

            }

            lastDensity = RippleDensity;
            lastVelocity = RippleVelocity;

            // Scale to ~ +/-1
            for (int kt = 0; kt < Npts; kt++)
            {
                data[kt] *= scaleFactor;
            }
        }

        private void CreateSweepDensity(float[] data)
		{
            RippleDensity = Mathf.Round(RippleDensity / densityResolution) * densityResolution;

            // How much to shift the phase (index) each time step (per octave). Recall that a phase shift of 0.5 cycles
            // means shifting half the table length.
            float deltaDen = (RippleDensity - lastDensity) * (float)tableLength;
			
            float arg1;
            float x = 0;
            float dx = 1.0f / CompPerOctave;

            float A;

            arg1 = lastVelocity;
            int velocityShift = (int)Mathf.Round(arg1 / FrequencyRes);

            for (int kc = 0; kc < numComponents; kc++)
            {
                int densityShift=0;
                int densityShiftSign = (deltaDen < 0) ? -1 : 1;
                int totalDensityShift = (int)(Mathf.Abs(Mathf.Round(deltaDen * x)));
                bool slopeLessThanOne = (totalDensityShift <= Npts);

                int dt;
                int dy;

                if (slopeLessThanOne)
                {
                    dy = totalDensityShift;
                    dt = Npts;
                }
                else
                {
                    dy = Npts;
                    dt = totalDensityShift;
                    densityShift = densityShiftSign;
                }

                int D = 2 * dy - dt;

                for (int kt = 0; kt < Npts; kt++)
                {
                    // create the current component sample
                    A = 1 + Depth * SinLUT[envelopeIndex[kc]];
                    data[kt] += amplitudes[kc] * A * SinLUT[carrierIndex[kc]];

                    // Update the carrier phase (index)
                    carrierIndex[kc] += carrierSkipSize[kc];
                    if (carrierIndex[kc] >= tableLength) carrierIndex[kc] -= tableLength;

                    // increment the density shift
                    if (slopeLessThanOne)
                    {
                        if (D > 0)
                        {
                            densityShift = densityShiftSign;
                            D += (2 * dy - 2 * dt);
                        }
                        else
                        {
                            densityShift = 0;
                            D += 2 * dy;
                        }
                    }
                    else
                    {
                        while (D <= 0)
                        {
                            densityShift += densityShiftSign;
                            D += 2 * dy;
                        }
                        densityShift += densityShiftSign;
                        D += (2 * dy - 2 * dt);
                    }

                    // update the phase (index) for the next time sample
                    envelopeIndex[kc] += velocityShift + densityShift;

                    densityShift = 0;

                    // Phase wrapping
                    if (envelopeIndex[kc] >= tableLength) envelopeIndex[kc] -= tableLength;
                    if (envelopeIndex[kc] < 0) envelopeIndex[kc] += tableLength;
                }

                x += dx;
            }

            lastDensity = RippleDensity;

            // Scale to ~ +/-1
            for (int kt = 0; kt < Npts; kt++)
            {
                data[kt] *= scaleFactor;
            }

        }

        private void CreateSweepDepth(float[] data)
		{
			// How much to increment (decrement) the modulation depth every time step
			Depth = Mathf.Clamp(Depth, 0f, 1f);
			float deltaDepth = (Depth - lastDepth) / (float)Npts;

            float arg1;

            float A;

            arg1 = lastVelocity;
			int velocityShift = (int)Mathf.Round(arg1/FrequencyRes);

			float curDepth = lastDepth;
			
            for (int kt = 0; kt < Npts; kt++)
            {
            	
	            for (int kc = 0; kc < numComponents; kc++)
                {
                    // create the current component sample
                    A = 1 + curDepth * SinLUT[envelopeIndex[kc]];
                    data[kt] += amplitudes[kc] * A * SinLUT[carrierIndex[kc]];

                    // Update the carrier phase (index)
                    carrierIndex[kc] += carrierSkipSize[kc];
                    if (carrierIndex[kc] >= tableLength) carrierIndex[kc] -= tableLength;

                    // update the phase (index) for the next time sample
                    envelopeIndex[kc] += velocityShift;

                    // Phase wrapping
                    if (envelopeIndex[kc] >= tableLength) envelopeIndex[kc] -= tableLength;
                    if (envelopeIndex[kc] < 0) envelopeIndex[kc] += tableLength;
                }

				// increment the depth
				curDepth += deltaDepth;
            }

            lastDepth = Depth;

			// Scale to ~ +/-1
            for (int kt = 0; kt < Npts; kt++)
            {
                data[kt] *= scaleFactor;
            }
        }

        private void CreateSweepBoth(float[] data)
		{
            RippleVelocity = Mathf.Round(RippleVelocity / FrequencyRes) * FrequencyRes;
            RippleDensity = Mathf.Round(RippleDensity / densityResolution) * densityResolution;


			// How much to increment (decrement) the table step size every time step
            float deltaVel_dt = (RippleVelocity - lastVelocity) / (float)Npts;
			
            // How much to shift the phase (index) each time step (per octave). Recall that a phase shift of 0.5 cycles
            // means shifting half the table length.
            float deltaDen_dt = (RippleDensity - lastDensity) * (float)tableLength;

            float arg1;
            float x = 0;
            float dx = 1.0f / CompPerOctave;

            float A;

            for (int kc = 0; kc < numComponents; kc++)
            {
				float totalDensityShift = Mathf.Round(deltaDen_dt * x);
				int shiftDensityEvery = (int) Mathf.Abs (Mathf.Floor (totalDensityShift / (float) Npts));
				int numSinceLastDenShift = 0;				
				int densityShiftIncrement = (totalDensityShift < 0) ? -1 : 1;
				
	            arg1 = lastVelocity;
                
                for (int kt = 0; kt < Npts; kt++)
                {
                    // create the current component sample
                    A = 1 + Depth * SinLUT[envelopeIndex[kc]];
                    data[kt] += amplitudes[kc] * A * SinLUT[carrierIndex[kc]];

                    // Update the carrier phase (index)
                    carrierIndex[kc] += carrierSkipSize[kc];
                    if (carrierIndex[kc] >= tableLength) carrierIndex[kc] -= tableLength;
					
					// increment the velocity argument
	                arg1 += deltaVel_dt;
					int velocityShift = (int)Mathf.Round(arg1/FrequencyRes);                

					// increment the density shift
					int densityShift = 0;
                    ++numSinceLastDenShift;
					if (numSinceLastDenShift == shiftDensityEvery)
					{
						densityShift = densityShiftIncrement;
						numSinceLastDenShift = 0;
					}

                    // update the phase (index) for the next time sample
                    envelopeIndex[kc] += velocityShift + densityShift;

                    // Phase wrapping
                    if (envelopeIndex[kc] >= tableLength) envelopeIndex[kc] -= tableLength;
                    if (envelopeIndex[kc] < 0) envelopeIndex[kc] += tableLength;
                }
				
                x += dx;
            }
			
			lastVelocity = RippleVelocity;
            lastDensity = RippleDensity;

            // Scale to ~ +/-1
            for (int kt = 0; kt < Npts; kt++)
            {
                data[kt] *= scaleFactor;
            }
        }

        private void CreateTrig(float[] data)
        {
            float dVel_dt = (RippleVelocity - lastVelocity) / (2.0f * T);
            float deltaDen = (RippleDensity - lastDensity) / Npts;

            float arg1, arg2;
            float x = 0;
            float f = Fmin;
            float dx = 1.0f / CompPerOctave;
            float df = Mathf.Pow(2, dx);
            float dt = 1.0f / samplingRate_Hz;

            float A;
            float t;

            for (int kc = 0; kc < numComponents; kc++)
            {
                t = 0;
                arg1 = 0;
                arg2 = lastDensity;
				
				int idx = 0;
                for (int kt = 0; kt < Npts; kt++)
                {
                   A = 1 + Depth * Mathf.Sin(2.0f * Mathf.PI * (arg1 + arg2 * x + phi[kc])); // phi[kc] incorporates the initial value of arg1
                    data[idx++] += A * Mathf.Sin(2.0f * Mathf.PI * (f * t + theta[kc]));

                    t += dt;
                    arg1 = ((lastVelocity + dVel_dt * t) * t);
                    arg2 += deltaDen;
                }
                phi[kc] += arg1;
                theta[kc] += Mathf.Round(f) * t;

                x += dx;
                f *= df;
            }

            lastDensity = RippleDensity;
            lastVelocity = RippleVelocity;

            for (int kt = 0; kt < Npts; kt++)
            {
                data[kt] *= scaleFactor;
            }
        }

  }
}
