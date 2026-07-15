using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using KLib.Expressions;
using KLib.Signals;
using KLib.Logging;

public class TappingPatternGenerator
{
    private List<float[]> _signals;

    private int[] _intervals;
    private int _numRepeats;

    // --- running position (persists across Process calls) ---
    private int _intervalIndex;   // which interval in the pattern
    private int _repeatIndex;     // which repeat of the whole pattern
    private int _posInInterval;   // samples already emitted into current interval
    private int _stimulusIndex;   // which stimulus is playing (free-running)

    private int _channelOffsetL;
    private int _channelOffsetR;

    private int _markerChannelOffset = 6;
    private float _markerPulseFreq = 1000f;
    private float[] _markerPulse;

    private AudioDspEventLog _dspEventLog;
    public AudioDspEventLog DspEventLog => _dspEventLog.Trim();

    private float _dt;

    public volatile bool IsComplete;

    public void Initialize(
        Channel channel, 
        float minISI,
        string intervalExpression,
        int numIntervals, 
        int numRepeats)
    {
        _numRepeats = numRepeats;

        var audioConfig = AudioSettings.GetConfiguration();
        _dt = 1f / audioConfig.sampleRate;

        CreateSignals(audioConfig.sampleRate, channel.Clone());
        CreatePattern(audioConfig.sampleRate, minISI, intervalExpression, numIntervals);
        CreateMarkerPulse();

        _dspEventLog = new AudioDspEventLog("pattern");

        IsComplete = false;
    }

    private void CreateSignals(int Fs, Channel channel)
    {
        float Tmax = channel.Gate.Width_ms;
        int npts = Mathf.RoundToInt(Fs * Tmax / 1000f);

        var signalManager = new SignalManager();
        signalManager.AddChannel(channel);

        signalManager.Initialize(Fs, npts, SessionContext.Signal);

        _channelOffsetL = channel.OutputNum;
        _channelOffsetR = channel.IsStereo ? channel.ContraOutputNum : -1;

        channel.SetActive(true);
        channel.Create();

        _signals = new List<float[]>();
        _signals.Add(channel.Data);
    }

    void CreateMarkerPulse()
    {
        var config = AudioSettings.GetConfiguration();

        int numPulsePts = Mathf.RoundToInt(config.sampleRate / _markerPulseFreq);
        _markerPulse = new float[numPulsePts];
        for (int k = 0; k < numPulsePts; k++) _markerPulse[k] = Mathf.Sin(2 * Mathf.PI * _markerPulseFreq * k / config.sampleRate) / Mathf.Sqrt(2);
    }


    private void CreatePattern(float Fs, float minISI, string ratioExpression, int numIntervals)
    {
        float[] ratios = Expressions.Evaluate(ratioExpression);

        int[] inum = new int[0];

        while (true)
        {
            inum = KLib.KMath.Permute(ratios.Length, numIntervals);
            bool allSame = inum.All(x => x == inum[0]);
            if (!allSame)
                break;
        }

        _intervals = new int[numIntervals];
        for (int k=0; k < numIntervals; k++)
        {
            _intervals[k] = Mathf.RoundToInt(Fs * ratios[inum[k]] * minISI / 1000f);
        }
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
            float[] stim = _signals[_stimulusIndex];

            // Fill until the buffer is full OR this interval ends.
            // First stim.Length samples of the interval = the stimulus, rest = silence.
            while (outPos < data.Length && _posInInterval < intervalLength)
            {
                data[outPos + _channelOffsetL] = (_posInInterval < stim.Length) ? stim[_posInInterval] : 0f;
                if (_channelOffsetR >= 0)
                    data[outPos + _channelOffsetR] = data[outPos + _channelOffsetL];

                if (_posInInterval < _markerPulse.Length)
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
            data[outPos + _channelOffsetL] = 0f;
            if (_channelOffsetR >= 0)
                data[outPos + _channelOffsetR] = 0f;
            outPos += channels;
        }
    }

    private void AdvanceInterval()
    {
        _posInInterval = 0;
        _stimulusIndex = (_stimulusIndex + 1) % _signals.Count;

        if (++_intervalIndex >= _intervals.Length)
        {
            _intervalIndex = 0;
            if (++_repeatIndex >= _numRepeats)
                IsComplete = true;
            Debug.Log($"repeat index = {_repeatIndex}");
        }
    }

    public void Reset()
    {
        _intervalIndex = _repeatIndex = _posInInterval = _stimulusIndex = 0;
        IsComplete = false;
    }
}
