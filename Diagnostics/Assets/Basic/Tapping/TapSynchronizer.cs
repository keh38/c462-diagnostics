using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using C462.Shared;
using Newtonsoft.Json;

[RequireComponent(typeof(AudioSource))]
public class TapSynchronizer : MonoBehaviour
{
    [SerializeField] public float pollInterval_s = 5;
    
    private float _pulseInterval_s = 1f;
    private float _pulseCarrierFreq = 1000f;
    private int _numPulsePts;
    private double[] _pulseDspTimes;

    private float[] _signal;
    private double _dt;
    private int _bufferSize;

    private int _signalIndex = 0;
    private int _pulseCount = 0;

    private bool _isRunning = false;
    private bool _stopPending = false;

    private long _firstPulseClockTime = -1;

    public double LastPulseDspTime => _pulseDspTimes != null && _pulseCount > 0 ? _pulseDspTimes[_pulseCount - 1] : 0;
    public long LastPulseClockTime { get; private set; } = 0;
    public double[] SyncDspTimes => _pulseDspTimes != null ? _pulseDspTimes[.._pulseCount] : new double[0];

    private int _channelOffset;
    private int _blocksProcessed = 0;

    public void StartSyncPulses(int channelOffset)
    {
        _channelOffset = channelOffset;
        CreateSignal();

        _pulseDspTimes = new double[10000];
        _pulseCount = 0;
        _signalIndex = _signal.Length / 2; // start with a delay so that the first pulse is not at the very beginning of the audio buffer

        _isRunning = true;
        _stopPending = false;

        Debug.Log("[TapSynchronizer]: started");
    }

    public void StopSyncPulses()
    {
        _stopPending = true;
        Debug.Log($"[TapSynchronizer]: stopped ({_pulseCount} pulses generated)");
    }

    void CreateSignal()
    {
        var config = AudioSettings.GetConfiguration();
        _dt = 1.0 / config.sampleRate;
        _bufferSize = config.dspBufferSize;

        _numPulsePts = Mathf.RoundToInt(config.sampleRate / _pulseCarrierFreq);
        var pulse = new float[_numPulsePts];
        for (int k = 0; k < _numPulsePts; k++) pulse[k] = Mathf.Sin(2 * Mathf.PI * _pulseCarrierFreq * k / config.sampleRate) / Mathf.Sqrt(2);

        int npts = Mathf.RoundToInt(config.sampleRate * _pulseInterval_s);
        _signal = new float[npts];

        for (int k = 0; k < _numPulsePts; k++) _signal[k] = pulse[k];
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        _blocksProcessed++;
        //Debug.Log($"[{GetType().Name}] blocksProcessed={_blocksProcessed} dspBase={AudioSettings.dspTime:F6} wall={HighPrecisionClock.UtcNowIn100nsTicks}");

        if (!_isRunning) return;

        //int index = channels - 1;
        int index = _channelOffset;

        for (int k = 0; k < _bufferSize; k++)
        {
            if (_stopPending && _signalIndex > _numPulsePts) // avoid cutting off the pulse in the middle
            {
                _isRunning = false;
                _stopPending = false;
                break;
            }

            if (_signalIndex == 0)
            {
                if (_pulseCount >= _pulseDspTimes.Length) // avoid buffer overflow, just in case
                {
                    _isRunning = false;
                    _stopPending = false;
                    break;
                }

                LastPulseClockTime = HighPrecisionClock.UtcNowIn100nsTicks;

                if (_firstPulseClockTime < 0) 
                    _firstPulseClockTime = LastPulseClockTime;

                _pulseDspTimes[_pulseCount] = AudioSettings.dspTime + _dt * k;
                _pulseCount++;
            }

            data[index] += _signal[_signalIndex];

            _signalIndex++;
            index += channels;

            if (_signalIndex >= _signal.Length) _signalIndex = 0;
        }
    }
}
