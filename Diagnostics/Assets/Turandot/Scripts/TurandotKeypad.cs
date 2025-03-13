using UnityEngine;
using System.Collections.Generic;
using Turandot.Inputs;

namespace Turandot.Scripts
{
    public class TurandotKeypad : TurandotInput
    {
        // TURANDOT FIX
        /*
        public UISprite keypadBG;
        public UISprite displayBG;
        public UILabel display;
        public TurandotButton enterKey;

        private List<KeypadButton> _buttons;

        private InputLog _log = new InputLog("keypad");
        private string _value;

        void Start()
        {
            foreach (TurandotKeypadButton b in GetComponentsInChildren<TurandotKeypadButton>())
            {
                b.ClickCallback = OnButtonClick;
            }
        }

        public string Value
        {
            get { return _value; }
        }

        private void OnButtonClick(string buttonText)
        {
            if (string.IsNullOrEmpty(_value)) enterKey.Enable();

            if (buttonText == "Delete")
                _log.Add(Time.timeSinceLevelLoad, 127);
            else
                _log.Add(Time.timeSinceLevelLoad, buttonText[0]);

            if (buttonText == ".")
            {
                if (!_value.Contains(".")) _value += ".";
            }
            else if (buttonText == "Delete")
            {
                if (_value.Length > 0) _value = _value.Substring(0, _value.Length - 1);
            }
            else if (buttonText == "-")
            {
                if (_value.Length > 0 && _value[0] == '-')
                    _value = _value.Substring(1);
                else
                    _value = _value.Insert(0, "-");
            }
            else
            {
                _value += buttonText;
            }
            display.text = _value;
        }

        public override void Activate(Turandot.Inputs.Input input)
        {
            _value = "";
            display.text = "0";

            _log.Clear();

            enterKey.Disable();

            Color bgColor = KLib.Unity.ColorFromARGB((uint)(input as Turandot.Inputs.Keypad).backgroundColor);
            displayBG.color = bgColor;

            base.Activate(input);
        }

        public string LogJSONString
        {
            get
            {
                _log.Trim();
                return KLib.FileIO.JSONSerializeToString(_log);
            }
        }
        */
    }
}