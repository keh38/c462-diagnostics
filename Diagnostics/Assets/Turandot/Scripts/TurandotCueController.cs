using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Turandot;
using Turandot.Cues;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotCueController : MonoBehaviour
    {
        public TurandotCue led;
        public TurandotCueMessage message;
        public TurandotFixationPoint fixationPoint;
        public TurandotProgressBarCue progressBar;
        public TurandotCueHelp helpCue;
        public TurandotCueCounter counter;
        public TurandotCueScoreboard scoreboard;
        public TurandotImage image;

        private List<string> _used = new List<string>();
        private List<Flag> _flags = null;

        public void Initialize(ScreenElements screen, List<string> cuesUsed)
        {
            _used = cuesUsed;

            //led.Initialize();
            //message.Initialize(screen.messageLayout);
            //fixationPoint.Initialize(screen.fixationPoint);
            //image.Initialize();
            //progressBar.Initialize(screen.progressBarLayout);
            //helpCue.Initialize();
            //counter.Initialize(screen.counter);
            //scoreboard.Initialize(screen.scoreboard);
        }

        public void ClearScreen()
        {
            //led.HideCue();
            //message.HideCue();
            //fixationPoint.ShowCue(false);
            //image.HideCue();
            //helpCue.HideCue();
            //counter.ShowCue(false);
            //scoreboard.ShowCue(false);
            //progressBar.ShowCue(false);
        }

        public void ApplySkin(Skin skin)
        {
            message.ApplySkin(skin);
            helpCue.ApplySkin(skin);
            //progressBar.ApplySkin(skin);
        }

        public void ClearLog()
        {
            //led.ClearLog();
            //message.ClearLog();
            //fixationPoint.ClearLog();
            //helpCue.ClearLog();
        }

        public void SetFlags(List<Flag> flags)
        {
            _flags = flags;
        }

        public void Activate(List<Cue> cues)
        {
            //led.Clear();
            //message.Clear();

            foreach (Cue c in cues)
            {
                if (c is Message)
                    message.ActivateMessage(c, _flags);
                //else if (c is FixationPointAction)
                //    fixationPoint.DoAction(c as FixationPointAction);
                //else if (c is ProgressBarAction)
                //    progressBar.DoAction(c as ProgressBarAction);
                else if (c is Image)
                    image.Activate(c as Image);
                else if (c is Help)
                    helpCue.Activate(c);
                else if (c is CounterAction)
                    counter.Activate(c as CounterAction);
                else if (c is ScoreboardAction)
                    scoreboard.Activate(c as ScoreboardAction);
                else
                    led.Activate(c);
            }
        }

        public void Deactivate()
        {
            //led.Deactivate();
            //message.Deactivate();
            //fixationPoint.Deactivate();
            //image.Deactivate();
            //progressBar.Deactivate();
            //helpCue.Deactivate();
            //counter.Deactivate();
            scoreboard.Deactivate();
        }

        public string LogJSONString
        {
            get
            {
                string json = "";
                //if (_used.Contains("led")) json = KLib.FileIO.JSONStringAdd(json, "LED", led.LogJSONString);
                //if (_used.Contains("message")) json = KLib.FileIO.JSONStringAdd(json, "Message", message.LogJSONString);
                //if (_used.Contains("fixation point")) json = KLib.FileIO.JSONStringAdd(json, "Fixation", fixationPoint.LogJSONString);
                //if (_used.Contains("help")) json = KLib.FileIO.JSONStringAdd(json, "Help", helpCue.LogJSONString);

                return json;
            }
        }

    }
}