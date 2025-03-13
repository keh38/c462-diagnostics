using UnityEngine;
using System.Collections;

using Turandot.Inputs;
using Input = Turandot.Inputs.Input;

namespace Turandot.Scripts
{
    public class TurandotInputCategorizer : TurandotInput
    {
        // TURANDOT FIX
        /*
        public UIWidget widget;

        private RadioButtonPanel _radioButtonPanel;
        private InputLog _log = new InputLog("category");
        private int _numItems = 1;


        void Start()
        {
            _radioButtonPanel = gameObject.GetComponent<RadioButtonPanel>();
            _radioButtonPanel.OnChange = OnRadioPanelChange;
        }

        override public void Activate(Input input)
        {
            _radioButtonPanel.LayoutPanel((input as Categorizer).categories, false);
            _numItems = (input as Categorizer).categories.Count;
            label.transform.localPosition = new Vector2(0, _radioButtonPanel.Top);
            button.IsVisible = false;

            _log.Clear();
            _log.Add(Time.timeSinceLevelLoad, float.NaN);

            base.Activate(input);
        }

        override public void Deactivate()
        {
            button.SetState(false);
            base.Deactivate();
        }

        private void OnRadioPanelChange()
        {
            button.IsVisible = true;
            _log.Add(Time.timeSinceLevelLoad, (float)_radioButtonPanel.Selected[0] / _numItems);
        }

        public int Result
        {
            get { return _radioButtonPanel.Selected[0]; }
        }

        public string LogJSONString
        {
            get
            {
                _log.Trim();
                return KLib.FileIO.JSONSerializeToString(_log);
            }
        }*/
    }
}