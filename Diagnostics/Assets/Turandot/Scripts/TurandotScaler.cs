using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Turandot.Inputs;
using Turandot.Screen;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Input = Turandot.Inputs.Input;

namespace Turandot.Scripts
{
    public class TurandotScaler : TurandotInput
    {
        [SerializeField] private TMPro.TMP_Text _label;
        [SerializeField] private TMPro.TMP_Text _leftLabel;
        [SerializeField] private TMPro.TMP_Text _rightLabel;
        [SerializeField] private Slider _slider;
        [SerializeField] private RectTransform _thumbRectTransform;
        [SerializeField] private GameObject _button;

        private RectTransform _buttonRectTransform;

        private ScalerLayout _layout = new ScalerLayout();
        private ScalerAction _action = null;

        public override string Name { get { return _layout.Name; } }
        public ButtonData ButtonData { get; private set; }

        private string _result;
        public override string Result { get { return _result; } }
        public override float Value { get { return _slider.value; } }

        private void Awake()
        {
            _buttonRectTransform = _button.GetComponent<RectTransform>();
        }

        public void Initialize(ScalerLayout layout)
        {
            _layout = layout;

            LayoutControl();
            ButtonData = new ButtonData() { name = layout.Name };

            _log = new InputLog(layout.Name);
        }

        private void LayoutControl()
        {
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(_layout.X, _layout.Y);
            rt.anchorMax = new Vector2(_layout.X, _layout.Y);
            rt.sizeDelta = new Vector2(_layout.Width, _layout.Height);

            _label.text = "";

            _leftLabel.fontSize = _layout.FontSize;
            _leftLabel.text = _layout.MinLabel;

            _rightLabel.fontSize = _layout.FontSize;
            _rightLabel.text = _layout.MaxLabel;
        }

        public void ClearLog()
        {
            _log.Clear();
        }

        override public void Activate(Input input, TurandotAudio audio)
        {
            var action = input as ScalerAction;

            _slider.value = 0.5f;
            ButtonData.value = false;
            _button.SetActive(false);
            _result = "";

            base.Activate(input, audio);
        }

        override public void Deactivate()
        {
            ButtonData.value = false;
            base.Deactivate();
        }

        public void OnSliderValueChanged(float sliderValue)
        {
            if (_action != null)
            {
                _log.Add(Time.timeSinceLevelLoad, sliderValue);
            }
        }

        public void OnPointerDown(BaseEventData data)
        {
            _button.SetActive(false);
        }

        public void OnPointerUp(BaseEventData data)
        {
            _buttonRectTransform.anchorMin = new Vector2(_thumbRectTransform.anchorMin.x, -1);
            _buttonRectTransform.anchorMax = new Vector2(_thumbRectTransform.anchorMin.x, -1);

            _button.SetActive(true);
        }

        public void OnButtonClick()
        {
            ButtonData.value = true;
            _result = $"{_layout.Name}={_slider.value};";
        }
    }
}