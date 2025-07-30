using UnityEngine;
using System.Collections;

using Turandot.Cues;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotCue : MonoBehaviour
    {
        virtual public string Name { get { return ""; } }

        public void Initialize() { }

        protected void SetPosition(float x, float y)
        {
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(x, y);
            rt.anchorMax = new Vector2(x, y);
        }

        Cue _cue = null;
        CueLog _log = new CueLog();

        public void ClearLog()
        {
            _log.Clear();
        }

        virtual public void Activate(Cue cue)
        {
            _cue = cue;
            ShowCue(cue.BeginVisible);
        }

        public void Deactivate()
        {
            if (_cue != null)
            {
                StopAllCoroutines();
                ShowCue(_cue.EndVisible);
            }
        }

        void ShowCue(bool visible)
        {
            gameObject.SetActive(visible);
            if (_log != null) _log.Add(Time.timeSinceLevelLoad, visible);
        }
/*
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
*/
        public string LogJSONString
        {
            get
            {
                _log.Trim();
                return KLib.FileIO.JSONSerializeToString(_log);
            }
        }

    }
}