using UnityEngine;
using System.Collections.Generic;

using KLib.Signals;
using KLib.Signals.Enumerations;
using KLib.Signals.Waveforms;

namespace Turandot.Scripts
{
    public class TurandotSecondaryAudio : MonoBehaviour
    {
        public AudioHighPassFilter hpFilter;
        public AudioLowPassFilter lpFilter;
        public AudioSource audioSource;

        SignalManager _sigMan;
        bool _isRunning = false;
        bool _killAudio = false;

        string _transducer = "";

        bool _setFilters = false;
        Noise _noise;

        public void Initialize(SignalManager sigMan, string transducer)
        {
            _sigMan = sigMan;
            _transducer = transducer;
            _isRunning = false;

            if (_sigMan != null)
            {
                int npts, nbuf;
                AudioSettings.GetDSPBufferSize(out npts, out nbuf);

                audioSource.bypassEffects = true;
                _setFilters = false;
                _noise = null;


                var ch = _sigMan.channels.Find(o => o.waveform is Noise);
                if (ch != null)
                {
                    _noise = ch.waveform as Noise;
                    if (_noise != null && _noise.filter.shape != FilterShape.None && _noise.filter.unityFilter)
                    {
                        audioSource.bypassEffects = false;
                        _setFilters = true;
                        hpFilter.cutoffFrequency = _noise.filter.CF * Mathf.Pow(2, -_noise.filter.BW / 2);
                        lpFilter.cutoffFrequency = _noise.filter.CF * Mathf.Pow(2, _noise.filter.BW / 2);
                    }
                }
                
                //_sigMan.Initialize(transducer, AudioSettings.outputSampleRate, npts);
            }

        }

        public void Reset()
        {
            _isRunning = false;
            _killAudio = false;

            if (_sigMan != null)
            {
                int npts, nbuf;
                AudioSettings.GetDSPBufferSize(out npts, out nbuf);

                //_sigMan.Initialize(_transducer, AudioSettings.outputSampleRate, npts);

                if (_setFilters)
                {
                    hpFilter.cutoffFrequency = _noise.filter.CF * Mathf.Pow(2, -_noise.filter.BW / 2);
                    lpFilter.cutoffFrequency = _noise.filter.CF * Mathf.Pow(2, _noise.filter.BW / 2);
                }
            }
        }

        public void Activate()
        {
            _killAudio = false;
            _isRunning = true;

        }

        public void Deactivate()
        {
            _isRunning = false;
        }

        public void KillAudio()
        {
            _killAudio = true;
        }


        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (_isRunning && !_sigMan.TimedOut)
            {
                _sigMan.Synthesize(data);

                if (_killAudio)
                {
                    Gate.RampDown(data);
                    Deactivate();
                }
            }
        }

    }
}