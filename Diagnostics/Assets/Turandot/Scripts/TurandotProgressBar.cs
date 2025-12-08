using Turandot.Cues;
using Turandot.Screen;

using UnityEngine;
using UnityEngine.UI;

namespace Turandot.Scripts
{
    public class TurandotProgressBar : TurandotCue
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _fillImage;

        private ProgressBarLayout _layout;

        public override string Name { get { return _layout.Name; } }

        public void Initialize(ProgressBarLayout layout)
        {
            _layout = layout;
            SetPosition(layout.X, layout.Y);

            var rt = GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(layout.Width, layout.Height);

            _fillImage.color = KLib.ColorTranslator.ColorFromARGB(layout.Color);
            _slider.value = 0;
        }

        public void SetValue(float value)
        {
            _slider.value = value;
        }
    }
}