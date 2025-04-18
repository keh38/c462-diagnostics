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

        public delegate void TimeOutDelegate(string source);
        public TimeOutDelegate TimeOut;
        private void OnTimeOut(string source) { TimeOut?.Invoke(source); }

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

        double[] _stimTimes = new double[1000];
        int _numStimTimes = 0;

        string _name;

        AudioLog _log = new AudioLog();
        bool _startLogged = false;

        public bool IsRunning { get { return _isRunning; } }
        public AudioLog Log { get { return _log; } }
        public int NumEvents { get { return _numStimTimes; } }
        public double[] EventTimes { get { return _stimTimes; } }

        void Start()
        {
        }

        public SignalManager SigMan
        {
            get { return _sigMan; }
        }

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

                audioSource.bypassEffects = true;

                _sigMan.Name = name;
                // TURANDOT FIX 
                //_sigMan.MaxLevelMargin = maxLevelMargin;
                _sigMan.AdapterMap = HardwareInterface.AdapterMap;;
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
            _numStimTimes = 0;

            if (_sigMan != null)
            {
                if (_sigMan.Synth)
                    throw new System.Exception("Still synthesizing!");

                int npts, nbuf;
                AudioSettings.GetDSPBufferSize(out npts, out nbuf);

                _sigMan.Name = name;
                _sigMan.Initialize(AudioSettings.outputSampleRate, npts);
                _sigMan.StartPaused();
                _isi = _sigMan.channels[0].gate.Active ? _sigMan.channels[0].gate.Period_ms / 1000f : float.PositiveInfinity;
            }
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

                HardwareInterface.Digitimer?.EnableDevices(_sigMan.GetDigitimerChannels());

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

            if (_sigMan != null)
            {
                HardwareInterface.Digitimer?.DisableDevices(_sigMan.GetDigitimerChannels());
            }
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
                    _stimTimes[_numStimTimes] = AudioSettings.dspTime;
                    _numStimTimes++;
                }
                else if (_sigMan.LoopOffset >= 0)
                {
                    _log.Add(AudioSettings.dspTime + _sigMan.LoopOffset, "activated");
                    _stimTimes[_numStimTimes] = AudioSettings.dspTime + _sigMan.LoopOffset;
                    _numStimTimes++;
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