using UnityEngine;
using System.Collections;

using Turandot.Cues;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotProgressBarCue : MonoBehaviour
    {
        // TURANDOT FIX
/*
        public UISprite foreground;
        public UISprite background;

        ProgressBarAction _cue = null;

        void Start()
        {
            ShowCue(false);
        }

        public void ShowCue(bool show)
        {
            foreground.alpha = show ? 1 : 0;
            background.alpha = show ? 1 : 0;
        }

        public void Initialize(ProgressBarLayout layout)
        {
            foreground.width = layout.W;
            foreground.height = layout.H;
            background.width = layout.W;
            background.height = layout.H;

            transform.localPosition = new Vector2(layout.X, layout.Y);

            ShowCue(false);
        }

        public void ApplySkin(Skin skin)
        {
            background.color = skin.sliderBackgroundColor;
            foreground.color = skin.progressForegroundColor;
        }

        public void DoAction(ProgressBarAction cue)
        {
            _cue = cue;
            ShowCue(cue.startVisible);
        }

        public void Deactivate()
        {
            if (_cue != null) ShowCue(_cue.endVisible);
        }
        */
    }
}