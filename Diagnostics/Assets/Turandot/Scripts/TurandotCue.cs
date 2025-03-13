using UnityEngine;
using System.Collections;

using Turandot.Cues;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotCue : MonoBehaviour
    {
        virtual public void Activate(Cue cue)
        {

        }
        public void Initialize()
        {
        }
        // TURANDOT FIX 
        /*
        public UIWidget widget;

        Cue _cue = null;
        CueLog _log;

        private float _maxAlpha = 1;

        virtual public void Initialize()
        {
            _log = new CueLog(name);
            _log.Clear();
            widget.alpha = 0;
        }

        public void HideCue()
        {
            widget.alpha = 0;
        }

        public void Clear()
        {
            _cue = null;
        }

        public void ClearLog()
        {
            _log.Clear();
        }

        virtual public void Activate(Cue cue)
        {
            _cue = cue;

            if (cue.changeAppearance)
            {
                widget.color = new Color(cue.R, cue.G, cue.B);
                widget.alpha = 0;
                _maxAlpha = cue.A;
                widget.transform.localPosition = new Vector2(cue.X, cue.Y);
            }

            if (_cue.delay_ms > 0 && _cue.startVisible && _cue.numFlash == 0)
            {
                StartCoroutine(ShowCueDelayed(_cue.delay_ms / 1000));
            }
            else
            {
                ShowCue(cue.startVisible);
            }

            if (_cue.numFlash > 0)
            {
                StartCoroutine(FlashCue());
            }
        }

        public void Deactivate()
        {
            if (_cue != null)
            {
                StopAllCoroutines();
                ShowCue(_cue.endVisible);
            }
        }

        void ShowCue(bool visible)
        {
            widget.alpha = visible ? _maxAlpha : 0;
            if (_log != null) _log.Add(Time.timeSinceLevelLoad, visible);
        }

        private IEnumerator ShowCueDelayed(float delay_s)
        {
            yield return new WaitForSeconds(delay_s);
            ShowCue(true);
        }

        private IEnumerator FlashCue()
        {
            float onTime = _cue.duration_ms * 0.001f;
            float offTime = (_cue.interval_ms - _cue.duration_ms) * 0.001f;

            widget.alpha = 0;
            _log.Add(Time.timeSinceLevelLoad, false);
            yield return new WaitForSeconds(_cue.delay_ms * 0.001f);


            for (int k = 0; k < _cue.numFlash; k++)
            {
                widget.alpha = _maxAlpha;
                _log.Add(Time.timeSinceLevelLoad, true);
                yield return new WaitForSeconds(onTime);
                widget.alpha = 0;
                _log.Add(Time.timeSinceLevelLoad, false);
                yield return new WaitForSeconds(offTime);
            }

            if (_cue.startVisible)
            {
                widget.alpha = _maxAlpha;
                _log.Add(Time.timeSinceLevelLoad, true);
            }

        }

        public string LogJSONString
        {
            get
            {
                _log.Trim();
                return KLib.FileIO.JSONSerializeToString(_log);
            }
        }*/

    }
}