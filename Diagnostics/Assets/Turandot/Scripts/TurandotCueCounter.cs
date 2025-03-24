using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Turandot.Cues;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotCueCounter : MonoBehaviour
    {
        public Text label;

        private Counter _defaultProperties;
        private CounterAction _action = null;

        private int _count = 0;
        private int _minVal = 0;
        private int _maxVal = 0;
        private bool _isRunning = false;
        private bool _stopRunning = false;

        public void Initialize(Counter c)
        {
            _defaultProperties = c;

            label.text = "";
            transform.localPosition = new Vector2(c.X, c.Y);
        }

        [System.Xml.Serialization.XmlIgnore]
        public int Count
        {
            get { return _count; }
        }

        public void ShowCue(bool show)
        {
            // TURANDOT FIX
            //NGUITools.SetActive(label.gameObject, show);
        }

        public void Activate(CounterAction action)
        {
            _action = action;
            switch (_action.action)
            {
                case CounterAction.Action.None:
                    break;

                case CounterAction.Action.Reset:
                    SetDefaultProperties();
                    _count = _action.startAt;
                    _minVal = _action.minValue;
                    _maxVal = _action.maxValue;
                    label.text = _count.ToString();
                    break;

                case CounterAction.Action.Start:
                    if (!_isRunning) StartCoroutine(RunCounter(_action.delay_s, _action.incrementBy, _action.interval_s));
                    break;

                case CounterAction.Action.Stop:
                    _stopRunning = true;
                    break;
            }


            if (_action.changeColor)
            {
                // TURANDOT FIX
                //label.color = KLib.Unity.ColorFromARGB(_action.newColor);
            }

            ShowCue(_action.StartVisible);
        }

        public void Deactivate()
        {
            if (_action != null) ShowCue(_action.EndVisible);
            _action = null;
        }

        private void SetDefaultProperties()
        {
            label.text = "";
            label.fontSize = _defaultProperties.size;
            // TURANDOT FIX
            //label.color = KLib.Unity.ColorFromARGB(_defaultProperties.color);
            transform.localPosition = new Vector2(_defaultProperties.X, _defaultProperties.Y);
        }

        private IEnumerator RunCounter(float delay, int increment, float interval)
        {
            _isRunning = true;
            _stopRunning = false;

            if (delay > 0)
                yield return new WaitForSeconds(delay);

            while (!_stopRunning)
            {
                    yield return new WaitForSeconds(interval);

                    if (!_stopRunning)
                {
                    _count += increment;
                    _count = Mathf.Max(_count, _minVal);
                    _count = Mathf.Min(_count, _maxVal);

                    label.text = _count.ToString();

                    if (_count == _minVal || _count == _maxVal) _stopRunning = true;
                }
            }

            _isRunning = false;
        }

    }
}