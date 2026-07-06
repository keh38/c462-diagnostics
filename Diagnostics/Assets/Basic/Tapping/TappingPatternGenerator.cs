using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using KLib.Expressions;
using KLib.Signals;

public class TappingPatternGenerator
{
    private List<float[]> _signals;

    private int[] _intervalLength;
    private int _numRepeats;
    private int _repeatNumber;
    private int _intervalNumber;
    private int _pointPerCycle;
    private int _currentPointIndex;
    private int _signalIndex;

    private int _channelOffsetL;
    private int _channelOffsetR;

    private int _bufferSize;

    public bool IsFinished { get; private set; }

    public void Initialize(
        Channel channel, 
        float minISI,
        string intervalExpression,
        int numIntervals)
    {
        var audioConfig = AudioSettings.GetConfiguration();
        _bufferSize = audioConfig.dspBufferSize;

        CreateSignals(audioConfig.sampleRate, channel.Clone());
        CreatePattern(audioConfig.sampleRate, minISI, intervalExpression, numIntervals);

        IsFinished = false;

        _signalIndex = 0;
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

        _intervalLength = new int[numIntervals];
        for (int k=0; k < numIntervals; k++)
        {
            _intervalLength[k] = Mathf.RoundToInt(Fs * ratios[inum[k]] * minISI * 1000f);
        }
    }

    public void Process(float[] data, int channels)
    {
        for (int k=0; k<_bufferSize; k++)
        {

        }
    }

}
