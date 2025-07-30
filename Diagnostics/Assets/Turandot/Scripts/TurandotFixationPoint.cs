using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotFixationPoint : TurandotCue
    {
        [SerializeField] private Image _image;
        [SerializeField] private Image _horizontalBar;
        [SerializeField] private Image _verticalBar;

        private FixationPointLayout _layout;

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
                var image_rt = _image.GetComponent<RectTransform>();
                image_rt.sizeDelta = new Vector2(_layout.Size, _layout.Size);
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
        }
   }
}