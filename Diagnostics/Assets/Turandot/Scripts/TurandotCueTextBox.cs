using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Turandot.Cues;
using Turandot.Screen;
using LogicUI.FancyTextRendering;

namespace Turandot.Scripts
{
    public class TurandotCueTextBox : TurandotCue
    {
        [SerializeField] private MarkdownRenderer _markdownRenderer;

        private TextBoxLayout _layout = new TextBoxLayout();

        public override string Name { get { return _layout.Name; } }

        public void Initialize(TextBoxLayout layout)
        {
            _layout = layout;
            LayoutControl();

            base.Initialize();
        }

        override public void Activate(Cue cue)
        {
            TextBoxAction m = cue as TextBoxAction;

            _markdownRenderer.Source = m.Markdown;

            base.Activate(cue);
        }

        private void LayoutControl()
        {
            _markdownRenderer.TextMesh.fontSize = _layout.FontSize;
            _markdownRenderer.TextMesh.color = KLib.ColorTranslator.ColorFromARGB(_layout.Color);
            _markdownRenderer.TextMesh.alignment = ConvertAlignmentEnum(_layout.HorizontalAlignment);

            var rt = GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(_layout.X, _layout.Y);
            rt.anchorMax = new Vector2(_layout.X, _layout.Y);

            float ypivot = 0.5f;
            switch (_layout.BoxVerticalAlignment)
            {
                case Instructions.VerticalTextAlignment.Top:
                    ypivot = 1;
                    break;
                case Instructions.VerticalTextAlignment.Middle:
                    ypivot = 0.5f;
                    break;
                case Instructions.VerticalTextAlignment.Bottom:
                    ypivot = 0;
                    break;
            }
            rt.pivot = new Vector2(0.5f, ypivot);
        }

        private TMPro.TextAlignmentOptions ConvertAlignmentEnum(Turandot.Instructions.HorizontalTextAlignment horizontalAlignment)
        {
            switch (horizontalAlignment)
            {
                case Turandot.Instructions.HorizontalTextAlignment.Left:
                    return TMPro.TextAlignmentOptions.TopLeft;
                case Turandot.Instructions.HorizontalTextAlignment.Center:
                    return TMPro.TextAlignmentOptions.Top;
                case Turandot.Instructions.HorizontalTextAlignment.Right:
                    return TMPro.TextAlignmentOptions.TopRight;
            }

            return TMPro.TextAlignmentOptions.Left;
        }

    }
}