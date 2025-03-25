using UnityEngine;
using System.Collections.Generic;

using KLib.Signals;
using KLib.Signals.Enumerations;
using KLib.Signals.Waveforms;

namespace Turandot.Scripts
{
    public class TurandotAudio : MonoBehaviour
    {
        //public AudioHighPassFilter hpFilter;
        //public AudioLowPassFilter lpFilter;
        public AudioSource audioSource;
        // TURANDOT FIX 

        public delegate void TimeOutDelegate(string source);
        public TimeOutDelegate TimeOut;
        private void OnTimeOut(string source) { TimeOut?.Invoke(source); }


        //KStringDelegate _onTimeOut = null;
        SignalManager _sigMan;
        bool _isRunning = false;
        bool _timerOnly = false;
        bool _killAudio = false;
        float _startTime = -1;
        float _timeOut = 0;
        float _isi = 0;

        string _transducer = "";

        bool _pauseAudio = false;
        bool _resumeAudio = false;

        string _name;

        AudioLog _log = new AudioLog();
        bool _startLogged = false;

        public bool IsRunning { get { return _isRunning; } }
        public AudioLog Log { get { return _log; } }

        //private System.DateTime tlast = System.DateTime.MinValue;
        //private double dsplast = 0;
        //private double dspElapsed = 0;
        //private double tElapsed = 0;

        void Start()
        {
        }

        public SignalManager SigMan
        {
            get { return _sigMan; }
        }
        // TURANDOT FIX 
        void Update()
        {
            if ((_timerOnly && (_killAudio || Time.timeSinceLevelLoad > _startTime + _timeOut && _timeOut > 0)) ||
                (_isRunning && (_sigMan.TimedOut || _sigMan.HaveException)))
            {
                Deactivate();

                if (_sigMan != null && _sigMan.HaveException)
                    throw _sigMan.LastException;

                if (!_killAudio) OnTimeOut(name);

            }
        }

        public void Initialize(SignalManager sigMan)
        {
            _sigMan = sigMan;
            //_transducer = transducer;
            _isRunning = false;

            if (_sigMan != null)
            {
                int npts, nbuf;
                AudioSettings.GetDSPBufferSize(out npts, out nbuf);
                Debug.Log("Npts = " + npts);
                Debug.Log("Nbuf = " + nbuf);

                audioSource.bypassEffects = true;

                _sigMan.Name = name;
                // TURANDOT FIX 
                //_sigMan.MaxLevelMargin = maxLevelMargin;
                _sigMan.AdapterMap = KLib.AdapterMap.DefaultStereoMap("HD280");
                _sigMan.Initialize(AudioSettings.outputSampleRate, npts);
                _sigMan.StartPaused();
                _isi = _sigMan.channels[0].gate.Period_ms / 1000f;
            }

            _log.Clear();
        }

        public void Reset()
        {
            _startTime = -1;
            _timerOnly = false;
            _isRunning = false;
            _killAudio = false;

            if (_sigMan != null)
            {
                if (_sigMan.Synth)
                    throw new System.Exception("Still synthesizing!");

                int npts, nbuf;
                AudioSettings.GetDSPBufferSize(out npts, out nbuf);

                _sigMan.Name = name;
                // TURANDOT FIX 
                //_sigMan.Initialize(_transducer, AudioSettings.outputSampleRate, npts);
                _isi = _sigMan.channels[0].gate.Active ? _sigMan.channels[0].gate.Period_ms / 1000f : float.PositiveInfinity;
            }
        }

        public void ClearLog()
        {
            _log.Clear();
        }

        public void Activate(float timeOut, List<Turandot.Flag> flags)
        {
            //Reset();

            _timeOut = timeOut;
            _startTime = Time.timeSinceLevelLoad;
            _killAudio = false;

            if (_sigMan != null)
            {
                //_sigMan.Activate(flags);

                if (timeOut == 0)
                {
                    _timeOut = _sigMan.GetMinTime(1f);
                }

                if (timeOut>=0 && _isi > 0 && !float.IsInfinity(_isi))
                {
                    _timeOut = Mathf.Round(timeOut / _isi) * _isi;
                }

                _sigMan.SetTimeout(_timeOut);
                _sigMan.ResetSweepables();
                _sigMan.Unpause();
                _isRunning = true;
                _name = name; // for debugging purposes: can't access 'name' from OnAudioFilterRead() 
            }
            else
            {
                _timerOnly = true;
            }
        }

        public void Deactivate()
        {
            _isRunning = false;
            _timerOnly = false;
        }

        public void KillAudio()
        {
            _killAudio = true;
        }

        public void PauseAudio(bool pause)
        {
            _pauseAudio = pause;
            _resumeAudio = !pause;
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if ((_isRunning || _resumeAudio) && !_sigMan.TimedOut)
            {
                var wasStarted = _sigMan.Synthesize(data);
                if (wasStarted)
                {
                    _log.Add(AudioSettings.dspTime, "activated");
                }

                if (_resumeAudio)
                {
                    Gate.RampUp(data);
                    _resumeAudio = false;
                    _isRunning = true;
                }

                if (_killAudio || _sigMan.TimedOut || _pauseAudio)
                {
                    Gate.RampDown(data);
                    if (_pauseAudio)
                    {
                        _pauseAudio = false;
                        _isRunning = false;
                    }
                }
                if (_killAudio)
                {
                    Deactivate();
                }
            }
        }

    }
}