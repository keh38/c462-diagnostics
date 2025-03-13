using UnityEngine;
using System.Collections.Generic;

using Turandot.Inputs;
using Turandot.Screen;
using Input = Turandot.Inputs.Input;

namespace Turandot.Scripts
{
    public class TurandotInputSlider : TurandotInput
    {
/*        // TURANDOT FIX

        public KUISlider slider;
        public UISprite thumb;
        public TurandotTickMark tickMark;
        public TurandotTickMark refTick;

        private InputLog _log = new InputLog("scaler");
        private bool _isEnabled = false;
        private bool _useButton = true;

        private float _xmin;
        private float _xmax;

        private List<GameObject> _ticks = new List<GameObject>();

        void Start()
        {
            slider.ThumbOnly = true;
            slider.OnSelectNotifier = OnSliderSelected;
            NGUITools.SetActive(tickMark.gameObject, false);
        }

        override public void Activate(Input input)
        {
            Scaler scaler = input as Scaler;

            thumb.alpha = scaler.thumb ? 1 : 0;

            slider.value = scaler.value;
            slider.foregroundWidget.width = scaler.length;
            slider.backgroundWidget.width = scaler.length;
            if (scaler.bgColor > 0)
                slider.GetComponent<UISprite>().color = KLib.Unity.ColorFromARGB(scaler.bgColor);

            SetupTickMarks(scaler.tickSpex);
            SetupReference(scaler.scaleReference);

            _useButton = input.enabled;

            _xmin = scaler.X - (float)(slider.foregroundWidget.width - button.Width) / 2;
            _xmax = scaler.X + (float)(slider.foregroundWidget.width - button.Width) / 2;

            button.IsVisible = false;

            _log.Clear();
            _log.Add(Time.timeSinceLevelLoad, float.NaN);

//            _isEnabled = true;
            _isEnabled = scaler.enabled;
            slider.enabled = scaler.enabled;

            base.Activate(input);
        }

        override public void Deactivate()
        {
            _isEnabled = false;
            base.Deactivate();
        }

        public override void ApplySkin(Skin skin)
        {
            base.ApplySkin(skin);

            slider.foregroundWidget.GetComponent<UISprite>().color = skin.sliderForegroundColor;
            slider.GetComponent<UISprite>().color = skin.sliderBackgroundColor;
        }

        private void SetupTickMarks(TickSpex spex)
        {
            if (_ticks != null && _ticks.Count > 0)
            {
                foreach (var t in _ticks) GameObject.Destroy(t);
                _ticks.Clear();
            }

            if (spex == null || spex.tickType == TickSpex.TickType.None)
            {
                NGUITools.SetActive(tickMark.gameObject, false);
                return;
            }


            List<string> labels;
            if (spex.tickType == TickSpex.TickType.Linear)
            {
                labels = new List<string>();
                for (float v = spex.min; v <= spex.max; v += spex.step) labels.Add(v.ToString());
            }
            else
            {
                labels = spex.labels;
            }

            int ntick = labels.Count;
            float dx = (float)slider.backgroundWidget.GetComponent<UISprite>().width / (ntick - 1);
            float x = slider.backgroundWidget.localCorners[0].x;

            NGUITools.SetActive(tickMark.gameObject, true);
            for (int k = 0; k < ntick; k++)
            {
                if (k == 0)
                {
                    tickMark.label.text = labels[k];
                    tickMark.transform.localPosition = new Vector2(x, tickMark.transform.localPosition.y);
                }
                else
                {
                    x += dx;

                    GameObject obj = GameObject.Instantiate(tickMark.gameObject);

                    obj.transform.parent = tickMark.gameObject.transform.parent;
                    obj.transform.localScale = Vector3.one;
                    obj.transform.localPosition = new Vector2(x, tickMark.transform.localPosition.y);
                    obj.GetComponent<TurandotTickMark>().label.text = labels[k];
                    _ticks.Add(obj);
                }

            }
        }

        private void SetupReference(ScaleReference scaleReference)
        {
            if (scaleReference == null || scaleReference.refType == ScaleReference.RefType.None)
            {
                NGUITools.SetActive(refTick.gameObject, false);
                return;
            }

            float val = KLib.Expressions.EvaluateToFloatScalar(scaleReference.location);
            if (float.IsNaN(val))
            {
                NGUITools.SetActive(refTick.gameObject, false);
                return;
            }

            NGUITools.SetActive(refTick.gameObject, true);
            float dx = val * (float)slider.backgroundWidget.GetComponent<UISprite>().width;
            float x = slider.backgroundWidget.localCorners[0].x;

            refTick.transform.localPosition = new Vector2(x + dx, refTick.transform.localPosition.y);
            refTick.label.text = scaleReference.label;
            refTick.line.color = KLib.Unity.ColorFromARGB(scaleReference.color);

            if (scaleReference.startAt)
            {
                slider.value = val;
            }
        }

        private void OnSliderSelected(bool selected)
        {
            if (!_useButton) return;

            button.IsVisible = !selected;
            if (!selected)
            {
                float x = Mathf.Max(_xmin, slider.thumb.localPosition.x);
                button.SetX(Mathf.Min(_xmax, x));
            }
        }

        public void SliderMoved()
        {
            if (_isEnabled) _log.Add(Time.timeSinceLevelLoad, slider.value);
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