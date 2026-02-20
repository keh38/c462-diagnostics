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
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _thumbImage;
        [SerializeField] private GameObject _tickMarkPrefab;

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

            if (_layout.ShowTicks && _layout.WholeNumbers)
            {
                if (_layout.LabelTicks)
                {
                    _leftLabel.rectTransform.pivot = new Vector2(1f, 0.5f);
                    _leftLabel.alignment = TMPro.TextAlignmentOptions.Right;
                    _leftLabel.rectTransform.anchorMin = new Vector2(0f, 0.5f);
                    _leftLabel.rectTransform.anchorMax = new Vector2(0f, 0.5f);
                    _leftLabel.rectTransform.anchoredPosition = new Vector2(-25, 0);

                    _rightLabel.rectTransform.pivot = new Vector2(0f, 0.5f);
                    _rightLabel.alignment = TMPro.TextAlignmentOptions.Left;
                    _rightLabel.rectTransform.anchorMin = new Vector2(1f, 0.5f);
                    _rightLabel.rectTransform.anchorMax = new Vector2(1f, 0.5f);
                    _rightLabel.rectTransform.anchoredPosition = new Vector2(25, 0);
                }

                CreateTickMarks();
            }

            _fillImage.raycastTarget = _layout.BarClickable;
            _backgroundImage.raycastTarget = _layout.BarClickable;

            _fillImage.enabled = _layout.ShowFill;
            if (!_layout.ShowThumb)
            {
                _thumbImage.color = new Color(1, 1, 1, 0);
            }

            _slider.minValue = _layout.MinValue;
            _slider.maxValue = _layout.MaxValue;
            _slider.wholeNumbers = _layout.WholeNumbers;
        }

        private void CreateTickMarks()
        {
            int numTicks = (int)(_layout.MaxValue - _layout.MinValue) + 1;
            for (int i = 0; i < numTicks; i++)
            {
                var tickMark = Instantiate(_tickMarkPrefab, _slider.transform);
                var tickRectTransform = tickMark.GetComponent<RectTransform>();
                float normalizedValue = (i * (_slider.maxValue - _slider.minValue) / (numTicks - 1) + _slider.minValue - _slider.minValue) / (_slider.maxValue - _slider.minValue);
                tickRectTransform.anchorMin = new Vector2(normalizedValue, 0);
                tickRectTransform.anchorMax = new Vector2(normalizedValue, 0);
                tickRectTransform.anchoredPosition = Vector2.zero;

                var tickLabel = tickMark.GetComponentInChildren<TMPro.TMP_Text>();
                tickLabel.fontSize = _layout.FontSize;
                tickLabel.text = (_layout.MinValue + i).ToString();
            }

        }

        public void ClearLog()
        {
            _log.Clear();
        }

        override public void Activate(Input input, TurandotAudio audio)
        {
            var action = input as ScalerAction;

            _slider.value = action.StartValue;
            ButtonData.value = false;
            _button.SetActive(false);

            if (action.Enabled != EnabledState.Enabled)
            {
                _fillImage.raycastTarget = false;
                _backgroundImage.raycastTarget = false;
                _thumbImage.raycastTarget= false;
            }

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