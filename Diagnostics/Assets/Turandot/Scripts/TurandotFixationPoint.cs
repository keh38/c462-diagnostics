using System.Collections;

using UnityEngine;
using UnityEngine.UI;

//using Turandot.Cues;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotFixationPoint : TurandotCue
    {
        [SerializeField] private Image _image;
        [SerializeField] private Image _horizontalBar;
        [SerializeField] private Image _verticalBar;

        private FixationPointLayout _layout;
        // TURANDOT FIX
        /*(        public UISprite background;
                public UISprite vertical;
                public UISprite horizontal;
                public UISprite ear;
                public UILabel label;

                CueLog _log;
                FixationPoint _fp = null;
                FixationPointAction _cue = null;

                public void ShowCue(bool show)
                {
                    transform.localPosition = new Vector2(show ? 0 : -1600, 0);

                    NGUITools.SetActive(background.gameObject, show);
                    NGUITools.SetActive(vertical.gameObject, show);
                    NGUITools.SetActive(horizontal.gameObject, show);
                    NGUITools.SetActive(ear.gameObject, show);
                }
                */

        public override string Name { get { return _layout.Name; } }

        public void Initialize(FixationPointLayout layout)
        {
            _layout = layout;

            SetPosition(_layout.X, _layout.Y);

            var rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(_layout.Size, _layout.Size);

            if (_layout.Shape == FixationPointLayout.Style.Circle)
            {
                _horizontalBar.enabled = false;
                _verticalBar.enabled = false;
                _image.color = KLib.ColorTranslator.ColorFromARGB(_layout.Color);
            }
            else
            {
                _image.enabled = false;

                rt = _horizontalBar.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(_layout.Size, _layout.BarWidth);
                _horizontalBar.color = KLib.ColorTranslator.ColorFromARGB(_layout.Color);

                rt = _verticalBar.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(_layout.BarWidth, _layout.Size);
                _verticalBar.color = KLib.ColorTranslator.ColorFromARGB(_layout.Color);
            }
            //_fp = fp;

            //_log = new CueLog(name);
            //_log.Clear();

            //if (fp.style == FixationPoint.Style.Default || fp.style == FixationPoint.Style.Bar)
            //{
            //    background.alpha = 1;
            //    vertical.alpha = 1;
            //    horizontal.alpha = 1;
            //    ear.alpha = 0;

            //    background.height = fp.size;
            //    background.width = fp.size;
            //    background.color = KLib.Unity.ColorFromARGB(fp.color);

            //    vertical.width = fp.barWidth;
            //    vertical.height = fp.size;
            //    vertical.color = KLib.Unity.ColorFromARGB(fp.barColor);

            //    horizontal.height = fp.barWidth;
            //    horizontal.width = fp.size;
            //    horizontal.color = KLib.Unity.ColorFromARGB(fp.barColor);

            //    vertical.transform.localEulerAngles = new Vector3(0, 0, fp.barAngle);
            //    horizontal.transform.localEulerAngles = new Vector3(0, 0, fp.barAngle);

            //    horizontal.alpha = (fp.style == FixationPoint.Style.Default) ? 1 : 0;
            //}
            //else if (fp.style == FixationPoint.Style.Ear)
            //{
            //    background.alpha = 0;
            //    vertical.alpha = 0;
            //    horizontal.alpha = 0;
            //    ear.alpha = 1;

            //    ear.height = fp.size;
            //    ear.width = fp.size;
            //    ear.color = KLib.Unity.ColorFromARGB(fp.color);
            //}

            //label.text = fp.label;

            //ShowCue(false);
        }
        /*
        public void ClearLog()
        {
            _log.Clear();
        }

        public void DoAction(FixationPointAction cue)
        {
            _cue = cue;

            if (_cue.changeAppearance)
            {
                vertical.transform.localEulerAngles = new Vector3(0, 0, cue.angle);
                horizontal.transform.localEulerAngles = new Vector3(0, 0, cue.angle);
                label.text = cue.label;
            }

            ShowCue(cue.startVisible);
            _log.Add(Time.timeSinceLevelLoad, cue.angle != _fp.barAngle || cue.label != _fp.label);
        }

        public void Deactivate()
        {
            if (_cue != null) ShowCue(_cue.endVisible);
        }

        public string LogJSONString
        {
            get
            {
                _log.Trim();
                return KLib.FileIO.JSONSerializeToString(_log);
            }
        }
        */
    }
}