using UnityEngine;
using System.Collections;

using Turandot.Inputs;
using Input = Turandot.Inputs.Input;
using System.Collections.Generic;
using Turandot.Screen;
using Newtonsoft.Json.Linq;

namespace Turandot.Scripts
{
    public class TurandotManikinPanel : TurandotInput
    {
        [SerializeField] private GameObject _manikinPrefab;
        [SerializeField] private GameObject _button;

        private ManikinLayout _layout;
        private List<TurandotManikinSlider> _sliders;
        private float[] yPositions;

        public override string Name { get { return _layout.Name; } }
        public ButtonData ButtonData { get; private set; }

        void Start()
        {
        }

        public void Initialize(ManikinLayout layout)
        {
            _layout = layout;
            LayoutControl();
            ButtonData = new ButtonData() { name = layout.Name };
        }

        private void LayoutControl()
        {
            _sliders = new List<TurandotManikinSlider>();
            yPositions = new float[_layout.Manikins.Count];

            float myWidth = 0;
            float myHeight = 0;
            float yoffset = 0;
            int index = 0;
            foreach (var item in _layout.Manikins)
            {
                var manikinSpec = item as ManikinSpec;

                var gameObject = GameObject.Instantiate(_manikinPrefab, this.transform);
                gameObject.name = manikinSpec.Name;
                var slider = gameObject.GetComponent<TurandotManikinSlider>();
                var rt = slider.Layout(_layout, manikinSpec, yoffset);
                slider.ValueChanged += OnSliderChanged;

                yPositions[index] = -yoffset;
                index++;

                yoffset += rt.rect.height + _layout.SliderSpacing;
                myHeight += rt.rect.height + _layout.SliderSpacing;
                myWidth = Mathf.Max(myWidth, rt.rect.width);

                _sliders.Add(slider);
            }

            var myRT = GetComponent<RectTransform>();
            myRT.sizeDelta = new Vector2(myWidth, myHeight);
        }

        override public void Activate(Input input, TurandotAudio audio)
        {
            ButtonData.value = false;
            _button.SetActive(false);

            foreach (var slider in _sliders)
            {
                slider.Reset();
            }

            if (_layout.RandomizeOrder)
            {
                var iorder = KLib.KMath.Permute(_sliders.Count);
                for (int k = 0; k<_sliders.Count; k++)
                {
                    float y = yPositions[iorder[k]];
                    var rt = _sliders[k].GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(0, y);
                }
            }

            base.Activate(input, audio);
        }

        override public void Deactivate()
        {
            base.Deactivate();
        }

        public void OnSliderChanged(float value)
        {
            _button.SetActive(_sliders.Find(x => !x.Moved) == null);
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

                foreach (var slider in _sliders)
                {
                    r += $"{slider.name}={slider.Value};";
                }

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