using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using KLib.Expressions;
using KLib.Signals;
using KLib.Logging;

using Tapping;

public class TappingPatternGenerator
{
    internal class Signal
    {
        public List<float[]> channelData = new List<float[]>();

        public Signal() { }
        public void AddChannelData(float[] data)
        {
            channelData.Add(data);
        }
    }

    private List<int> _channelOffsets = new List<int>();
    private List<Signal> _signals = new List<Signal>();

    private int[] _intervals;

    // --- running position (persists across Process calls) ---
    private int _intervalIndex;   // which interval in the pattern
    private int _posInInterval;   // samples already emitted into current interval
    private int _signalIndex;   // which stimulus is playing (free-running)

    private int _markerChannelOffset = 6;
    private float _markerPulseFreq = 1000f;
    private float[] _markerPulse;

    private AudioDspEventLog _dspEventLog;
    public AudioDspEventLog DspEventLog => _dspEventLog?.Trim();

    private bool _isPacer;

    private float _Fs;
    private float _dt;

    public volatile bool IsComplete;

    public TappingPatternGenerator(float Fs)
    {
        _Fs = Fs;
        _dt = 1f / _Fs;

        CreateMarkerPulse();
    }

    public void Initialize(Channel channel, float[] intervals, List<ParameterProfile> parameterProfiles, bool isPacer)
    {
        _isPacer = isPacer;

        // Convert intervals from ms to samples
        _intervals = new int[intervals.Length];
        for (int k=0; k<intervals.Length; k++) {
            _intervals[k] = Mathf.RoundToInt(_Fs * intervals[k] / 1000f);
        }

        CreateSignals(channel.Clone(), parameterProfiles);

        _dspEventLog = new AudioDspEventLog(channel.Name);

        Reset();
    }

    private void CreateSignals(Channel channel, List<ParameterProfile> parameterProfiles)
    {
        var myProfiles = parameterProfiles?.Where(p => p.Item.StartsWith($"{channel.Name}.")).ToList();

        if (myProfiles != null && myProfiles.Count > 1)
            throw new Exception("TappingPatternGenerator cannot handle more than one parameter profile");

        float Tmax = channel.Gate.Width_ms;
        int npts = Mathf.RoundToInt(_Fs * Tmax / 1000f);

        var signalManager = new SignalManager();
        signalManager.AddChannel(channel);

        signalManager.Initialize(_Fs, npts, SessionContext.Signal);

        _channelOffsets.Clear();
        _signals.Clear();

        channel.SetActive(true);
        channel.Create();

        var signal = new Signal();
        signal.AddChannelData(channel.Data);
        _channelOffsets.Add(channel.OutputNum);

        if (channel.IsStereo)
        {
            channel.ContraSide.Create();
            signal.AddChannelData(channel.ContraSide.Data);
            _channelOffsets.Add(channel.ContraSide.OutputNum);
        }
        _signals.Add(signal);
    }

    void CreateMarkerPulse()
    {
        int numPulsePts = Mathf.RoundToInt(_Fs / _markerPulseFreq);
        _markerPulse = new float[numPulsePts];
        for (int k = 0; k < numPulsePts; k++) _markerPulse[k] = Mathf.Sin(2 * Mathf.PI * _markerPulseFreq * k * _dt) / Mathf.Sqrt(2);
    }

    public void Process(float[] data, int channels, int blockNumber)
    {
        int outPos = 0;

        double offset = 0;
        while (outPos < data.Length && !IsComplete)
        {
            if (_posInInterval == 0)
            {
                _dspEventLog.AddEvent(blockNumber, offset);
            }

            int intervalLength = _intervals[_intervalIndex];
            var stim = _signals[_signalIndex];

            // Fill until the buffer is full OR this interval ends.
            // First stim.Length samples of the interval = the stimulus, rest = silence.
            while (outPos < data.Length && _posInInterval < intervalLength)
            {
                for (int k = 0; k < _channelOffsets.Count; k++)
                {
                    data[outPos + _channelOffsets[k]] = (_posInInterval < stim.channelData[k].Length) ? stim.channelData[k][_posInInterval] : 0f;
                }

                if (_isPacer && _posInInterval < _markerPulse.Length)
                    data[outPos + _markerChannelOffset] = _markerPulse[_posInInterval];

                outPos += channels;

                _posInInterval++;
                offset += _dt;
            }

            // Advance ONLY if we truly reached the interval end,
            // not if we just ran out of buffer.
            if (_posInInterval >= intervalLength)
                AdvanceInterval();
        }

        // After completion (or if we broke early), pad remaining buffer with silence.
        while (outPos < data.Length)
        {
            for (int k = 0; k < _channelOffsets.Count; k++)
                data[outPos + _channelOffsets[k]] = 0f;
            outPos += channels;
        }
    }

    private void AdvanceInterval()
    {
        _posInInterval = 0;
        _signalIndex = (_signalIndex + 1) % _signals.Count;

        if (++_intervalIndex >= _intervals.Length)
        {
            IsComplete = true;
        }
    }

    public void Reset()
    {
        _intervalIndex = _posInInterval = _signalIndex = 0;
        IsComplete = false;
    }
}
