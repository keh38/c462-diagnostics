using UnityEngine;
using System.Collections;

using ExtensionMethods;
using Turandot.Inputs;

namespace Turandot.Scripts
{
    public class TurandotRandomProcess : TurandotInput
    {
        private RandomProcess _randomProcess = null;
        private bool _isEnabled = false;
        private float _nextTime = -1;
        private float _nextValue = float.NaN;

        private Turandot.ScalarData _data = new ScalarData("Random process");

        public ScalarData Data
        {
            get { return _data; }
        }

        void Update()
        {
            if (_isEnabled)
            { 
                if (Time.timeSinceLevelLoad >= _nextTime)
                {
                    _data.SetValue(_nextValue);
                    GenerateNextTimeAndValue();
                }
                else
                {
                    _data.hasChanged = false;
                }
            }
        }

        override public void Activate(Turandot.Inputs.Input input)
        {
            _randomProcess = input as RandomProcess;
            GenerateNextTimeAndValue();
            _isEnabled = true;
        }

        override public void Deactivate()
        {
            _isEnabled = false;
        }

        private void GenerateNextTimeAndValue()
        {
            _nextTime = Time.timeSinceLevelLoad + KLib.Expressions.EvaluateToFloatScalar(_randomProcess.intervalExpr);
            _nextValue = KLib.Expressions.EvaluateToFloatScalar(_randomProcess.valueExpr);
            Debug.Log(Time.timeSinceLevelLoad + ": next value = " + _nextValue + " @ next time = " + (_nextTime - Time.timeSinceLevelLoad).ToString());
        }

    }
}