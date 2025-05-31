using UnityEngine;
using System.Collections;

using Turandot.Inputs;
using Input = Turandot.Inputs.Input;
using System.Collections.Generic;
using Turandot.Screen;
using Newtonsoft.Json.Linq;

namespace Turandot.Scripts
{
    public class TurandotManikins : TurandotInput
    {
        [SerializeField] private TurandotManikinSlider _valenceSlider;
        [SerializeField] private TurandotManikinSlider _arousalSlider;
        [SerializeField] private TurandotManikinSlider _dominanceSlider;
        [SerializeField] private TurandotManikinSlider _loudnessSlider;
        [SerializeField] private GameObject _button;

        private ManikinLayout _layout;

        public override string Name { get { return _layout.Name; } }
        public ButtonData ButtonData { get; private set; }

        void Start()
        {
            _valenceSlider.ValueChanged += OnSliderChanged;
            _arousalSlider.ValueChanged += OnSliderChanged;
            _dominanceSlider.ValueChanged += OnSliderChanged;
            _loudnessSlider.ValueChanged += OnSliderChanged;
        }

        public void Initialize(ManikinLayout layout)
        {
            _layout = layout;
            LayoutControl();
            ButtonData = new ButtonData() { name = layout.Name };
        }

        private void LayoutControl()
        {
            _arousalSlider.gameObject.SetActive(_layout.ShowArousal);
            _loudnessSlider.gameObject.SetActive(_layout.ShowLoudness);
            _dominanceSlider.gameObject.SetActive(_layout.ShowDominance);  
            _valenceSlider.gameObject.SetActive(_layout.ShowValence);
        }

        override public void Activate(Input input, TurandotAudio audio)
        {
            ButtonData.value = false;
            _button.SetActive(false);

            _arousalSlider.Reset();
            _loudnessSlider.Reset();
            _dominanceSlider.Reset();
            _valenceSlider.Reset();

            base.Activate(input, audio);
        }

        override public void Deactivate()
        {
            base.Deactivate();
        }

        public void OnSliderChanged(float value)
        {
            _button.SetActive(
                (!_layout.ShowArousal || _arousalSlider.Moved) &&
                (!_layout.ShowLoudness || _loudnessSlider.Moved) &&
                (!_layout.ShowDominance || _dominanceSlider.Moved) &&
                (!_layout.ShowValence || _valenceSlider.Moved)
                );
        }

        public void OnButtonClick()
        {
            ButtonData.value = true;
            //_result = _value.ToString();
        }

        public override string Result
        {
            get
            {
                string r = "";

                if (_layout.ShowValence)
                    r += "valence=" + _valenceSlider.Value + ";";

                if (_layout.ShowArousal)
                    r += "arousal=" + _arousalSlider.Value + ";";

                if (_layout.ShowDominance)
                    r += "dominance=" + _dominanceSlider.Value + ";";

                if (_layout.ShowLoudness)
                    r += "loudness=" + _loudnessSlider.Value + ";";

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