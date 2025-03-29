using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Turandot.Cues;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotCueScoreboard : MonoBehaviour
    {
        public Text label;
        public TurandotCueCounter counter;

        private Scoreboard _defaultProperties;
        private ScoreboardAction _action = null;

        private int _score = 0;

        public void Initialize(Scoreboard s)
        {
            _score = 0;

            _defaultProperties = s;
            SetDefaultProperties();
        }

        public void ShowCue(bool show)
        {
            //NGUITools.SetActive(label.gameObject, show);
        }

        public void Activate(ScoreboardAction action)
        {
            _action = action;
            switch (_action.action)
            {
                case ScoreboardAction.ScoreAction.None:
                    break;

                case ScoreboardAction.ScoreAction.Change:
                    ChangeScore(_action.deltaExpr, _action.pointsPerSec, _action.maxSec);
                    break;
            }

            ShowCue(_action.BeginVisible);
        }

        public void Deactivate()
        {
            if (_action != null) ShowCue(_action.EndVisible);
            _action = null;
        }

        private void SetDefaultProperties()
        {
            label.text = _score.ToString();
            label.fontSize = _defaultProperties.size;
            //label.color = KLib.Unity.ColorFromARGB(_defaultProperties.color);
            transform.localPosition = new Vector2(_defaultProperties.X, _defaultProperties.Y);
        }

        private void ChangeScore(string expr, int rate, float tmax)
        {
            expr = expr.Replace("Counter", counter.Count.ToString());
            int delta = KLib.Expressions.EvaluateToIntScalar(expr);

            if (rate <= 0 || tmax <= 0)
            {
                _score += delta;
                label.text = _score.ToString();
                //label.color = KLib.Unity.ColorFromARGB(_score < 0 ? _defaultProperties.negativeColor : _defaultProperties.color);
            }
            else
            {
                StartCoroutine(AnimateScoreChange(delta, rate, tmax));
            }
        }

        private IEnumerator AnimateScoreChange(int delta, int rate, float tmax)
        {
            float interval = 0.1f;

            float T = Mathf.Min(tmax, (float) Mathf.Abs(delta) / rate);

            int curScore = _score;
            int nsteps = Mathf.FloorToInt(T / interval);
            int ptsPerStep = Mathf.RoundToInt((float)delta / nsteps);

            for (int k=0; k<nsteps; k++)
            {
                curScore += ptsPerStep;
                label.text = curScore.ToString();
                //label.color = KLib.Unity.ColorFromARGB(curScore < 0 ? _defaultProperties.negativeColor : _defaultProperties.color);

                yield return new WaitForSeconds(interval);
            }

            _score += delta;
            label.text = _score.ToString();
            //label.color = KLib.Unity.ColorFromARGB(_score < 0 ? _defaultProperties.negativeColor : _defaultProperties.color);
        }

    }
}