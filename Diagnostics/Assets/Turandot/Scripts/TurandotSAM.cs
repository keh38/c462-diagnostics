using UnityEngine;
using System.Collections;

using Turandot.Inputs;
using Input = Turandot.Inputs.Input;

namespace Turandot.Scripts
{
    public class TurandotSAM : TurandotInput
    {
        public TurandotSAMSlider valenceSlider;
        public TurandotSAMSlider arousalSlider;
        public TurandotSAMSlider dominanceSlider;
        public TurandotSAMSlider loudnessSlider;

        private bool _validValence = false;
        private bool _validArousal = false;
        private bool _validDominance = false;
        private bool _validLoudness = false;

        private SAM _sam = null;

        void Start()
        {
            //valenceSlider.NotifyOnMove = OnValenceChanged;
            //arousalSlider.NotifyOnMove = OnArousalChanged;
            //dominanceSlider.NotifyOnMove = OnDominanceChanged;
            //loudnessSlider.NotifyOnMove = OnLoudnessChanged;
        }

        override public void Activate(Input input)
        {
            _sam = input as SAM;

            //button.IsVisible = false;

            int nactive = _sam.valence.visible ? 1 : 0;
            nactive += _sam.arousal.visible ? 1 : 0;
            nactive += _sam.dominance.visible ? 1 : 0;
            nactive += _sam.loudness.visible ? 1 : 0;

            float dy = 337;
            float y = 225;
            if (nactive == 1) y = 0;
            else if (nactive == 2) y = dy / 2;

            //loudnessSlider.Initialize("Loudness", _sam.loudness, _sam.color);
            _validLoudness = !_sam.loudness.visible;
            if (_sam.loudness.visible)
            {
                loudnessSlider.transform.localPosition = new Vector2(0, y);
                y -= dy;
            }
            //NGUITools.SetActive(loudnessSlider.gameObject, _sam.loudness.visible);

            //valenceSlider.Initialize("Valence", _sam.valence, _sam.color);
            _validValence = !_sam.valence.visible;
            if (_sam.valence.visible)
            {
                valenceSlider.transform.localPosition = new Vector2(0, y);
                y -= dy;
            }

            //NGUITools.SetActive(valenceSlider.gameObject, _sam.valence.visible);

            //arousalSlider.Initialize("Arousal", _sam.arousal, _sam.color);
            _validArousal = !_sam.arousal.visible;
            if (_sam.arousal.visible)
            {
                arousalSlider.transform.localPosition = new Vector2(0, y);
                y -= dy;
            }
            //NGUITools.SetActive(arousalSlider.gameObject, _sam.arousal.visible);

            //dominanceSlider.Initialize("Dominance", _sam.dominance, _sam.color);
            _validDominance = !_sam.dominance.visible;
            if (_sam.dominance.visible)
            {
                dominanceSlider.transform.localPosition = new Vector2(0, y);
                y -= dy;
            }
            //NGUITools.SetActive(dominanceSlider.gameObject, _sam.dominance.visible);

            button.transform.localPosition = new Vector3(button.transform.localPosition.x, y + dy);

            base.Activate(input);
        }

        override public void Deactivate()
        {
            //button.IsVisible = false;

            //valenceSlider.Deactivate();
            //arousalSlider.Deactivate();
            //dominanceSlider.Deactivate();
            //loudnessSlider.Deactivate();
            base.Deactivate();
        }

        public void OnValenceChanged()
        {
            _validValence = true;
            //button.IsVisible = _validValence && _validArousal && _validDominance && _validLoudness;
        }

        public void OnArousalChanged()
        {
            _validArousal = true;
            //button.IsVisible = _validValence && _validArousal && _validDominance && _validLoudness;
        }

        public void OnDominanceChanged()
        {
            _validDominance = true;
            //button.IsVisible = _validValence && _validArousal && _validDominance && _validLoudness;
        }

        public void OnLoudnessChanged()
        {
            _validLoudness = true;
            //button.IsVisible = _validValence && _validArousal && _validDominance && _validLoudness;
        }

        public string Result
        {
            get
            {
                string r = "";

                //if (_sam.valence.visible)
                //    r += "valence=" + valenceSlider.Value + ";";

                //if (_sam.arousal.visible)
                //    r += "arousal=" + arousalSlider.Value + ";";

                //if (_sam.dominance.visible)
                //    r += "dominance=" + dominanceSlider.Value + ";";

                //if (_sam.loudness.visible)
                //    r += "loudness=" + loudnessSlider.Value + ";";

                return r;
            }
        }

        public string LogJSONString
        {
            get
            {
                string json = "";
#if !KDEBUG
                //if (_sam.valence.visible) json = KLib.FileIO.JSONStringAdd(json, "valence", valenceSlider.LogJSONString);
                //if (_sam.arousal.visible) json = KLib.FileIO.JSONStringAdd(json, "arousal", arousalSlider.LogJSONString);
                //if (_sam.dominance.visible) json = KLib.FileIO.JSONStringAdd(json, "dominance", dominanceSlider.LogJSONString);
#endif
                return json;
            }
        }

    }
}