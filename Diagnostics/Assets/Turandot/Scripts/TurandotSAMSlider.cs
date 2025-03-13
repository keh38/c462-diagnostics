using UnityEngine;
using System.Collections;

using Turandot.Inputs;

namespace Turandot.Scripts
{
    public class TurandotSAMSlider : MonoBehaviour
    {
        // TURANDOT FIX
/*
        public KUISlider slider;
        public UILabel minLabel;
        public UILabel maxLabel;
        public UISprite sprite;

        private InputLog _log = new InputLog();
        private bool _isEnabled = false;

        public void Initialize(string name, SAM.Appearance appearance, int color)
        {
            minLabel.text = appearance.minlabel;
            maxLabel.text = appearance.maxLabel;

            slider.value = 0;
            slider.HasValue = false;

            sprite.color = KLib.Unity.ColorFromARGB((uint)color);
            slider.thumb.gameObject.GetComponent<UISprite>().color = KLib.Unity.ColorFromARGB((uint)color);

            NGUITools.SetActive(slider.thumb.gameObject, false);

            _log.Initialize(name);
            _log.Add(Time.timeSinceLevelLoad, float.NaN);
            _isEnabled = true;
        }

        public void Deactivate()
        {
            _isEnabled = false;
        }

        public KEventDelegate NotifyOnMove
        {
            get; set;
        }

        public void OnSliderMove()
        {
            if (_isEnabled)
            {
                NGUITools.SetActive(slider.thumb.gameObject, true);
                _log.Add(Time.timeSinceLevelLoad, slider.value);
                NotifyOnMove();
            }
        }

        public int Value
        {
            get { return (int)(slider.value * 8); }
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