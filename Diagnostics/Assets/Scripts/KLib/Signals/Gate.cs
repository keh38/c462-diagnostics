using UnityEngine;
using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Enumerations;

namespace KLib.Signals
{
    /// <summary>
    /// Class for creation of signal gates.
    /// </summary>
	[ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
	[JsonObject(MemberSerialization.OptOut)]
    [Serializable]
	public class Gate
    {
        [ProtoMember(1, IsRequired = true)]
        /// <summary>
        /// Turn gate on and off.
        /// </summary>
        /// <remarks>
        /// If the gate is not active, <see cref="Create">Create</see> returns all ones
        /// </remarks>
        public bool Active { set; get; }

        /// <summary>
        /// Gate onset (ms)
        /// </summary>
        /// <remarks>
        /// <list><item>Default = 0</item></list>
        /// </remarks>
        [ProtoMember(2, IsRequired = true)]
        public float Delay_ms { set; get; }

        /// <summary>
        /// Gate duration (ms)
        /// </summary>
        /// <remarks>
        /// <list><item>Default = 50 ms</item></list>
        /// </remarks>
        [ProtoMember(3, IsRequired = true)]
        public float Duration_ms { set; get; }

        [ProtoMember(4, IsRequired = true)]
        /// <summary>
        /// Gate ramp rise-fall time (ms)
        /// </summary>
        /// <remarks>
        /// <list><item>Default = 5 ms</item></list>
        /// </remarks>
        public float Ramp_ms { set; get; }

        [ProtoMember(5, IsRequired = true)]
        public bool Bursted { set; get; }

        [ProtoMember(6, IsRequired = true)]
        public float BurstDuration_ms { set; get; }

        [ProtoMember(7, IsRequired = true)]
        public int NumPulses { set; get; }

        /// <summary>
        /// Gate period.
        /// </summary>
        /// <value>The period_ms.</value>
        [ProtoMember(8, IsRequired = true)]
        public float Period_ms { set; get; }

        private float[] myRamp;
        private int _rampIndex;

        private int _gateIndex;
        private int _startPointOfRampUp;
        private int _startPointOfRampDown;
        private int _totalPoints;
        private int _nmax;

        private int _lastPulseTotalPoints;
        private int _currentPulse;
        private float _fs = 0;

        private bool _isOneShot;

        private GateState _state;
        private bool _wasLooped = false;


        [JsonIgnore]
        [ProtoIgnore]
        public GateState State
        {
            get { return _state; }
        }

        [JsonIgnore]
        [ProtoIgnore]
        public bool Looped
        {
            get { return _wasLooped; }
        }

        [JsonIgnore]
        [ProtoIgnore]
        public bool Running
        {
            get { return Active && _state != GateState.Finished; }
        }

        [JsonIgnore]
        [ProtoIgnore]
        public int NMax { get { return _nmax; } }

        [JsonIgnore]
        [ProtoIgnore]
        public int DelaySamples { get { return _startPointOfRampUp; } }

        [JsonIgnore]
        [ProtoIgnore]
        public int TotalSamples { get { return _totalPoints; } }

        /// <summary>
        /// Construct default Gate object.
        /// </summary>
        public Gate()
        {
            Active = false;
            Delay_ms = 0;
            Duration_ms = 50;
            Ramp_ms = 5;
            Period_ms = 100;
            Bursted = false;
        }

        public Gate(float ramp_ms)
        {
            Active = true;
            Delay_ms = 0;
            Ramp_ms = ramp_ms;
            Duration_ms = Period_ms = float.PositiveInfinity;
            Bursted = false;
        }

        /// <summary>
        /// Constructor that initializes gate properties.
        /// </summary>
        /// <param name="delay_ms">Gate onset (ms)</param>
        /// <param name="duration_ms">Gate duration (ms)</param>
        /// <param name="ramp_ms">Gate ramp rise-fall time (ms)</param>
        /// <remarks>Automatically sets <see cref="Active"/> to true</remarks>
        public Gate(float delay_ms, float duration_ms, float ramp_ms)
        {
            Active = true;
            Delay_ms = delay_ms;
            Duration_ms = duration_ms;
            Ramp_ms = ramp_ms;
            Bursted = false;
        }

        public List<string> GetValidParameters()
        {
            List<string> p = new List<string>();

            if (Active)
            {
                p.Add("Gate.Delay_ms");
                p.Add("Gate.Duration_ms");
                p.Add("Gate.Period_ms");
            }
            return p;
        }

        public string SetParameter(string paramName, float value)
        {
            switch (paramName)
            {
                case "Delay_ms":
                    Delay_ms = value;
                    UpdateProperties();
                    break;

                case "Duration_ms":
                    Duration_ms = value;
                    UpdateProperties();
                    break;

                case "Period_ms":
                    Period_ms = value;
                    break;
            }

            return "";
        }

        public float GetParameter(string paramName)
        {
            switch (paramName)
            {
                case "Delay_ms":
                    return Delay_ms;
                case "Duration_ms":
                    return Duration_ms;
                case "Period_ms":
                    return Period_ms;
            }

            return float.NaN;
        }


        /// <summary>
        /// Static function to create just the ramp portion of a gate.
        /// </summary>
        /// <param name="Fs">Sampling rate (Hz)</param>
        /// <param name="N">Number of points in ramp</param>
        /// <param name="up"><paramref name="true"/> for ramp up, <paramref name="false"/> for ramp down</param>
        /// <returns>sine-squared ramp</returns>
        public static float[] Sine2Ramp(float Fs, int N, bool up)
        {
            float[] array = new float[N];
            for (int k = 0; k < N; k++)
            {
                array[k] = Mathf.Sin(0.25f * 2 * Mathf.PI * k / N);
                array[k] = array[k] * array[k];
                if (!up) array[k] = 1 - array[k];
            }

            return (array);
        }

        public static void RampUp(float[] data)
        {
            for (int k = 0; k < data.Length; k++)
            {
                data[k] *= (Mathf.Sin(0.25f * 2 * Mathf.PI * k / data.Length));
            }

        }

        public static void RampDown(float[] data)
        {
            for (int k = 0; k < data.Length; k++)
            {
                data[k] *= (1 - Mathf.Sin(0.25f * 2 * Mathf.PI * k / data.Length));
            }

        }

        public void Set(float delay_ms, float ramp_ms)
        {
            Active = true;
            Delay_ms = delay_ms;
            Duration_ms = float.PositiveInfinity;
            Ramp_ms = ramp_ms;
            Period_ms = float.PositiveInfinity;
        }

        /// <summary>
        /// Sets gate properties in a single call.
        /// </summary>
        /// <param name="delay_ms">Gate onset (ms)</param>
        /// <param name="duration_ms">Gate duration (ms)</param>
        /// <param name="ramp_ms">Gate ramp rise-fall time (ms)</param>
        /// <remarks>Automatically sets <see cref="Active"/> to true</remarks>
        public void Set(float delay_ms, float ramp_ms, float duration_ms)
        {
            Active = true;
            Delay_ms = delay_ms;
            Duration_ms = duration_ms;
            Ramp_ms = ramp_ms;

            // don't want this to function as a one-shot automatically. Too cumbersome.
            //Period_ms = float.PositiveInfinity;
            Period_ms = duration_ms;
        }

        /// <summary>
        /// Sets gate properties in a single call.
        /// </summary>
        /// <param name="delay_ms">Delay_ms.</param>
        /// <param name="ramp_ms">Ramp_ms.</param>
        /// <param name="duration_ms">Duration_ms.</param>
        /// <param name="period_ms">Period_ms.</param>
        /// <remarks>Automatically sets <see cref="Active"/> to true</remarks>
        public void Set(float delay_ms, float ramp_ms, float duration_ms, float period_ms)
        {
            Active = true;
            Delay_ms = delay_ms;
            Duration_ms = duration_ms;
            Ramp_ms = ramp_ms;
            Period_ms = period_ms;
        }


        /// <summary>
        /// Synthesize gate that can be applied to signal of duration T.
        /// </summary>
        /// <param name="Fs">Sampling rate (Hz)</param>
        /// <param name="T">Buffer duration (ms)</param>
        /// <returns>Gate waveform. If <see cref="Active"/> = <paramref name="true"/>, returns array of ones.</returns>
        /// <exception cref="IndexOutOfRangeException">Gate extends beyond length of signal buffer (delay + duration > T)</exception>
        /// <exception cref="IndexOutOfRangeException">Ramps longer than duration (2 * ramp > duration)</exception>
        public float[] Create(float Fs, float T)
        {
            int N = Mathf.RoundToInt(Fs * T/1000);
            return Create(Fs, N);
        }

        /// <summary>
        /// Synthesize gate that can be applied to signal of length N.
        /// </summary>
        /// <param name="Fs">Sampling rate (Hz)</param>
        /// <param name="N">Total length of signal buffer (including zeros on either side of gate)</param>
        /// <returns>Gate waveform. If <see cref="Active"/> = <paramref name="true"/>, returns array of ones.</returns>
        /// <exception cref="IndexOutOfRangeException">Gate extends beyond length of signal buffer (delay + duration > N/Fs)</exception>
        /// <exception cref="IndexOutOfRangeException">Ramps longer than duration (2 * ramp > duration)</exception>
        public float[] Create(float Fs, int N)
        {
            float[] array = new float[N];

            int nDelayPts = (int)(Delay_ms * Fs / 1000);
            int nWidthPts = (int)(Duration_ms * Fs / 1000);
            int nRampPts = (int)(Ramp_ms * Fs / 1000);
            int numOnes = nWidthPts - 2 * nRampPts;

            if (nDelayPts + nWidthPts > N)
            {
                throw new System.IndexOutOfRangeException("Gate extends beyond length of signal buffer.");
            }
            if (numOnes < 0)
            {
                throw new System.IndexOutOfRangeException("Gate rise/fall ramps are longer than gate duration.");
            }

            if (Active)
            {
                int idx = nDelayPts;
                for (int k = 0; k < nRampPts; k++) array[idx++] = Mathf.Pow(Mathf.Sin(0.25f * 2 * Mathf.PI * k / nRampPts), 2);
                for (int k = 0; k < numOnes; k++) array[idx++] = 1;
                for (int k = 0; k < nRampPts; k++) array[idx++] = 1 - array[nDelayPts + k];
            }
            else
            {
                for (int k = 0; k < N; k++) array[k] = 1;
            }

            return (array);
        }

        public void Initialize(float Fs, int N)
        {
            _fs = Fs;

            _nmax = -1;
            _startPointOfRampUp = -1;
            _totalPoints = -1;

            if (!Active) return;

            int nRampPts = Mathf.RoundToInt(Fs * Ramp_ms / 1000);

            myRamp = Sine2Ramp(Fs, nRampPts, true);

            _totalPoints = Mathf.RoundToInt(Fs * Period_ms / 1000);
            if (Bursted)
            {
                float burstDur = BurstDuration_ms > Period_ms ? BurstDuration_ms : Period_ms;

                _lastPulseTotalPoints = (int)Mathf.Round(Fs * (burstDur - (NumPulses - 1) * Period_ms) / 1000);
            }

            _startPointOfRampUp = Mathf.RoundToInt(Fs * Delay_ms / 1000);
            if (float.IsPositiveInfinity(Duration_ms))
            {
                _startPointOfRampDown = int.MaxValue;
            }
            else
            {
                _startPointOfRampDown = _startPointOfRampUp + Mathf.RoundToInt(Fs * Duration_ms / 1000) - nRampPts;
            }

            _isOneShot = float.IsPositiveInfinity(Period_ms);

            _gateIndex = 0;
            _rampIndex = 0;
            _currentPulse = 0;

            _state = GateState.Idle;

            if (!Bursted && !float.IsInfinity(Duration_ms) && Period_ms > 0)
            {
                int n = Mathf.RoundToInt(Fs * Period_ms / 1000);
                if (n < N) _nmax = Mathf.RoundToInt(Fs * Duration_ms / 1000);
            }
        }

        private void UpdateProperties()
        {
            int nRampPts = Mathf.RoundToInt(_fs * Ramp_ms / 1000);
            _startPointOfRampUp = Mathf.RoundToInt(_fs * Delay_ms / 1000);
            _startPointOfRampDown = _startPointOfRampUp + Mathf.RoundToInt(_fs * Duration_ms / 1000) - nRampPts;
        }

        public void Apply(float[] data)
        {
            bool finished = false;
            float value = 0;

            _wasLooped = false;

            for (int k=0; k<data.Length; k++)
            {
                if (_gateIndex < _startPointOfRampUp)
                {
                    _state = GateState.Delay;
                    value = 0;
                }
                else if (_gateIndex < _startPointOfRampDown && _rampIndex < myRamp.Length)
                {
                    _state = GateState.RampUp;
                    value = myRamp[_rampIndex++];
                }
                else if (_gateIndex < _startPointOfRampDown)
                {
                    _state = GateState.On;
                    value = 1;
                }
                else if (_rampIndex > 0)
                {
                    _state = GateState.RampDown;
                    value = myRamp[--_rampIndex];
                }
                else
                {
                    _state = GateState.Off;
                    value = 0;
                }

                data[k] *= value;

                if (_isOneShot && _state == GateState.Off)
                {
                    finished = true;
                }
                else
                {
                    _gateIndex++;
                    if (Bursted)
                    {
                        if (_currentPulse == NumPulses - 1 && _gateIndex == _lastPulseTotalPoints)
                        {
                            _gateIndex = 0;
                            _wasLooped = true;
                            _currentPulse = 0;
                        }
                        else if (_currentPulse < NumPulses - 1 && _gateIndex == _totalPoints)
                        {
                            _gateIndex = 0;
                            _currentPulse++;
                        }
                    }
                    else if (_gateIndex == _totalPoints)
                    {
                        _gateIndex = 0;
                        _wasLooped = true;
                    }
                }
            }

            if (finished)
            {
                _state = GateState.Finished;
            }
        }

        public void Reset()
        {
            _gateIndex = 0;
            _rampIndex = 0;
            _state = GateState.Idle;
        }
    }
}
