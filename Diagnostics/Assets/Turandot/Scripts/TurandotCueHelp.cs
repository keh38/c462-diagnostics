using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using Turandot.Cues;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotCueHelp : TurandotCue
    {
        public Text label;

        //UISprite _sprite;
        CueLog _log;
        Cue _cue;

        Color _backgroundColor = new Color(0.65f, 0.65f, 0.65f, 0.8f);

        float _animationSpeed = 1.5f;
        // TURANDOT FIX

        override public void Activate(Cue cue)
        {
            _cue = cue;

            //cue.color = KLib.Unity.ColorInt(_backgroundColor);

            //if (cue.changeAppearance)
            //{
            //    _cue.X = (int)(cue as Help).x;
            //    _cue.Y = (int)(cue as Help).y;
            //    widget.width = (int)(cue as Help).w;
            //    widget.height = (int)(cue as Help).h;
            //}
            //else
            //{
            //    _cue.X = 0;
            //    _cue.Y = 100;
            //    widget.width = 1750;
            //    widget.height = 800;
            //    widget.transform.localPosition = new Vector2(_cue.X, _cue.Y);
            //}

            //base.Activate(cue);

            //StartCoroutine(TextAnimator((cue as Help).text));
        }

        // TURANDOT FIX
         public void Initialize()
        {
            //_sprite = widget.GetComponent<UISprite>();
            //_sprite.color = _backgroundColor;
            //_sprite.alpha = 0.85f;
            //label.color = Color.black;
            base.Initialize();
        }


        IEnumerator TextAnimator(string msg)
        {
            string[] lines = msg.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            for (int k = 0; k < lines.Length; k++)
            {
                lines[k] = "[FFFFFF]" + lines[k] + "[-]";
            }

            for (int k = 0; k < lines.Length; k++)
            {
                lines[k] = lines[k].Replace("[FFFFFF]", "[000000]");
                lines[k] = lines[k].Replace("][-]", "]");

                label.text = string.Join("\n\n", lines);

                yield return new WaitForSeconds(k == lines.Length - 1 ? 0.5f : _animationSpeed);
            }

        }

    }
}