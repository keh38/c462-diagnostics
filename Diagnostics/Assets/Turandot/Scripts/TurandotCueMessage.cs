using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Turandot.Cues;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotCueMessage : TurandotCue
    {
        [SerializeField] private TMPro.TMP_Text _label;

        private MessageLayout _layout = new MessageLayout();

        public override string Name { get { return _layout.Name; } }

        public void Initialize(MessageLayout layout)
        {
            _layout = layout;
            LayoutControl();

            base.Initialize();
        }

        public void ActivateMessage(Cue cue, List<Flag> flags)
        {
            Message m = cue as Message;
            _label.text = SubstituteFlags(m.Text, flags);
            ChangeAppearance(m);

            base.Activate(cue);
        }

        override public void Activate(Cue cue)
        {
            Message m = cue as Message;

            _label.text = m.Text;
            //ChangeAppearance(m);

            base.Activate(cue);
        }

        private void LayoutControl()
        {
            _label.fontSize = _layout.FontSize;
            _label.color = KLib.ColorTranslator.ColorFromARGB(_layout.Color);

            SetPosition(_layout.X, _layout.Y);
        }

        private void ChangeAppearance(Message m)
        { 
            // TURANDOT FIX
            //label.fontSize = m.changeAppearance ? m.fontSize : _layout.fontSize;
            //label.color = KLib.Unity.ColorFromARGB((uint)_layout.color);

            //widget.transform.localPosition = new Vector2(_layout.X, _layout.Y);

            //NGUITools.SetActive(sprite.gameObject, m.icon != Message.Icon.None);
            //if (m.icon != Message.Icon.None)
            //{
            //    sprite.spriteName = "Icon_" + m.icon.ToString();
            //    sprite.width = m.iconSize;
            //    sprite.height = m.iconSize;

            //    if (!string.IsNullOrEmpty(label.text))
            //    {
            //        sprite.transform.localPosition = new Vector2(label.transform.localPosition.x - (label.width + m.iconSize) / 2 - 25, 0);
            //    }
            //    else
            //    {
            //        sprite.transform.localPosition = new Vector2(0, 0);
            //    }
            //}
        }

        private string SubstituteFlags(string text, List<Flag> flags)
        {
            string pattern = @"{([a-zA-Z]+)}";
            Match m = Regex.Match(text, pattern);

            while (m.Success)
            {
                var f = flags.Find(x => x.name.Equals(m.Groups[1].Value));
                if (f != null)
                {
                    text = text.Replace(m.Groups[0].Value, f.value.ToString());
                }

                m = m.NextMatch();
            }

            return text;
        }

        public void Append(string text)
        {
            _label.text += text;
        }

    }
}