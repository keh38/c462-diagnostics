using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Turandot;
using Input = Turandot.Inputs.Input;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotInputMonitor : MonoBehaviour
    {
        // TURANDOT FIX
        public TurandotSAM SAM;
        public GameObject Circle;
        public GameObject Square;
        public GameObject Left;
        public GameObject Right;
        public TurandotGrapher grapher;
        public TurandotParamSlider paramSlider;

        bool _isRunning = false;
        List<TurandotButton> _buttons = new List<TurandotButton>();
        List<ButtonData> _buttonData = new List<ButtonData>();
        List<ScalarData> _scalarData = new List<ScalarData>();
        EventLog _log = new EventLog();

        int[] _controlValues;
        int[] _eventValues;

        List<InputEvent> _inputEvents;
        TurandotInputSlider _scaleSlider;
        TurandotInputCategorizer _categorizer;
        TurandotKeypad _keypad;
        TurandotThumbSlider _thumbSlider;
        TurandotPupillometer _pupillometer;
        TurandotRandomProcess _randomProcess;

        //KStringDelegate _onEventChanged;

        List<Flag> _flags;
        List<string> _used;
        int _numButtons = 0;

        void Start()
        {
            _categorizer = GameObject.Find("Inputs/Categorizer").GetComponent<TurandotInputCategorizer>();
            _scaleSlider = GameObject.Find("Inputs/Scale").GetComponent<TurandotInputSlider>();
            _keypad = GameObject.Find("Inputs/Keypad").GetComponent<TurandotKeypad>();
            _thumbSlider = GameObject.Find("Inputs/Thumb Slider").GetComponent<TurandotThumbSlider>();
            _pupillometer = GameObject.Find("Pupillometer").GetComponent<TurandotPupillometer>();
            _randomProcess = GameObject.Find("Inputs/Random Process").GetComponent<TurandotRandomProcess>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isRunning) return;

            for (int k = 0; k < _buttonData.Count; k++)
            {
                _controlValues[k] = _buttonData[k].value ? 1 : 0;
            }

            foreach (InputEvent ie in _inputEvents)
            {
                if (ie.Update(_buttonData, _flags, _scalarData, Time.deltaTime))
                {
                    //_onEventChanged(ie.name);
                }
            }

            for (int k = 0; k < _inputEvents.Count; k++)
            {
                _eventValues[k] = _inputEvents[k].Value ? 1 : 0;
            }

            _log.Add(Time.timeSinceLevelLoad, _controlValues, _eventValues);
        }

        public void PollEvents()
        {
            foreach (InputEvent ie in _inputEvents)
            {
                if (ie.Value)
                {
                    //_onEventChanged(ie.name);
                }
            }
        }

        public void ShowDefaultInputs(bool show)
        {
            //for (int k = 0; k < _numButtons; k++) _buttons[k].Deactivate();
//                NGUITools.SetActive(_buttons[k].gameObject, show);
        }

        public void ClearScreen()
        {
            ShowDefaultInputs(false);
            _thumbSlider.transform.localPosition = new Vector2(-1800, -1500);
            SAM.Deactivate();
        }

        //public KStringDelegate OnEventChanged
        //{
        //    set { _onEventChanged = value; }
        //}

        public void Initialize(ScreenElements screen, List<ButtonLayout> buttonSpex, List<InputEvent> inputEvents, List<string> inputsUsed)
        {
            _isRunning = false;
            _used = inputsUsed;

            _scalarData.Clear();

            //if (screen.inputs.elements.Contains("pupillometer"))
            //{
                //_pupillometer.Activate();
                //_scalarData.Add(_pupillometer.Data);
            //}

            _buttons = new List<TurandotButton>();
            foreach (ButtonLayout bs in buttonSpex)
            {
                _buttons.Add(CreateTurandotButton(bs));
            }
            _numButtons = _buttons.Count;

            if (_used.Contains("categorizer")) _buttons.Add(_categorizer.button);
            if (_used.Contains("keypad")) _buttons.Add(_keypad.button);
            if (_used.Contains("grapher"))
            {
                //grapher.Initialize(screen.grapherLayout);
            }
            if (_used.Contains("sam")) _buttons.Add(SAM.button);
            if (_used.Contains("scaler")) _buttons.Add(_scaleSlider.button);
            if (_used.Contains("param slider"))
            {
                //paramSlider.Initialize(screen.paramSliderLayout);
                //_buttons.Add(paramSlider.button);
            }
            if (_used.Contains("thumb slider"))
            {
                //_thumbSlider.Initialize(screen.thumbSliderLayout);
            }

            if (_used.Contains("random process"))
            {
                _scalarData.Add(_randomProcess.Data);
            }

            _buttonData = new List<ButtonData>();
            //foreach (var b in _buttons) _buttonData.Add(b.data);
            if (_used.Contains("grapher"))
            {
                _buttonData.Add(grapher.ButtonData);
            }

            _log.Initialize(_buttonData.Select(b => b.name).ToArray(), inputEvents.Select(ie => ie.name).ToArray());
            _controlValues = new int[_buttonData.Count];

            _inputEvents = inputEvents;
            _eventValues = new int[inputEvents.Count];

            ShowDefaultInputs(true);
        }

        private TurandotButton CreateTurandotButton(ButtonLayout buttonSpec)
        {
            //int height = buttonSpec.size;
            //GameObject src = null;
            //switch (buttonSpec.style)
            //{
            //    case ButtonLayout.ButtonStyle.Circle:
            //        src = Circle;
            //        break;
            //    case ButtonLayout.ButtonStyle.None:
            //    case ButtonLayout.ButtonStyle.Square:
            //        src = Square;
            //        break;
            //    case ButtonLayout.ButtonStyle.Rectangle:
            //        src = Square;
            //        height = buttonSpec.height;
            //        break;
            //    case ButtonLayout.ButtonStyle.Left:
            //        src = Left;
            //        height = buttonSpec.height;
            //        break;
            //    case ButtonLayout.ButtonStyle.Right:
            //        src = Right;
            //        height = buttonSpec.height;
            //        break;
            //}
            //GameObject obj = GameObject.Instantiate(src);

            //obj.transform.parent = src.transform.parent;
            //obj.transform.localScale = Vector3.one;
            //obj.transform.localPosition = new Vector2(buttonSpec.x, buttonSpec.y);
            //obj.name = buttonSpec.name;

            //TurandotButton b = obj.GetComponent<TurandotButton>();
            ////b.SetSize(buttonSpec.size, height);

            ////b.ApplySkin(_skin);
            ////b.Initialize(buttonSpec);
            ////b.IsVisible = false;

            ////if (buttonSpec.style == ButtonSpec.ButtonStyle.None)
            ////{
            ////    b.MakeHidden();
            ////}

            return null;
        }

        public List<TurandotButton> Buttons
        {
            get { return _buttons; }
        }

        public void Activate(List<Input> inputs, TurandotAudio audio, float timeOut)
        {
            foreach (Input input in inputs)
            {
                if (input is Turandot.Inputs.Categorizer)
                    _categorizer.Activate(input);
                else if (input is Turandot.Inputs.GrapherAction)
                    grapher.Activate(input, audio.SigMan, timeOut);
                else if (input is Turandot.Inputs.Keypad)
                    _keypad.Activate(input);
                //else if (input is Turandot.Inputs.ParamSliderAction)
                //    paramSlider.Activate(input, audio);
                else if (input is Turandot.Inputs.SAM)
                    SAM.Activate(input);
                else if (input is Turandot.Inputs.Scaler)
                    _scaleSlider.Activate(input);
                else if (input is Turandot.Inputs.ThumbSliderAction)
                    _thumbSlider.Activate(input);
                else if (input is Turandot.Inputs.RandomProcess)
                    _randomProcess.Activate(input);
                else if (input is Turandot.Inputs.Button)
                {
                    //_buttons.Find(b => b.name == input.name).Activate(input);
                }
            }
        }

        public void Deactivate()
        {
            _categorizer.Deactivate();
            grapher.Deactivate();
            _keypad.Deactivate();
            paramSlider.Deactivate();
            SAM.Deactivate();
            _scaleSlider.Deactivate();
            _thumbSlider.Deactivate();
            _randomProcess.Deactivate();

            //for (int k = 0; k < _numButtons; k++) _buttons[k].Deactivate();
            foreach (var ie in _inputEvents) ie.ClearRisingFalling();
        }

        public void StartMonitor(List<Flag> flags)
        {
            _flags = flags;
            foreach (InputEvent ie in _inputEvents)
            {
                ie.Reset();
            }
            //paramSlider.ClearLog();
            //_thumbSlider.ClearLog();
            //for (int k = 0; k < _numButtons; k++) _buttons[k].ClearLog();

            _log.Clear();
            _isRunning = true;
        }

        public void StopMonitor()
        {
            _isRunning = false;
            _flags = null;
        }

        public string EventLogJSONString
        {
            get
            {
                _log.Trim();
                return KLib.FileIO.JSONSerializeToString(_log);
            }
        }

        public string InputLogJSONString
        {
            get
            {
                string json = "";

                //if (_used.Contains("categorizer")) json = KLib.FileIO.JSONStringAdd(json, "Category", _categorizer.LogJSONString);
                //if (_used.Contains("grapher")) json = KLib.FileIO.JSONStringAdd(json, "Grapher", grapher.LogJSONString);
                //if (_used.Contains("keypad")) json = KLib.FileIO.JSONStringAdd(json, "Keypad", _keypad.LogJSONString);
                //if (_used.Contains("param slider")) json = KLib.FileIO.JSONStringAdd(json, "ParamSlider", paramSlider.LogJSONString);
                //if (_used.Contains("sam")) json = KLib.FileIO.JSONStringAdd(json, "SAM", SAM.LogJSONString);
                //if (_used.Contains("scaler")) json = KLib.FileIO.JSONStringAdd(json, "Scaler", _scaleSlider.LogJSONString);
                //if (_used.Contains("thumb slider")) json = KLib.FileIO.JSONStringAdd(json, "ThumbSlider", _thumbSlider.LogJSONString);

                // SAVED WITH EVENTS???

                //for (int k = 0; k < _numButtons; k++)
                //    if (_buttons[k].HaveLog) json = KLib.FileIO.JSONStringAdd(json, _buttons[k].name, _buttons[k].LogJSONString);

                return json;
            }
        }

        public string Category
        {
            get { return ":"; }
            //get { return "category=" + _categorizer.Result.ToString() + ";"; }
        }

        public string SliderResult
        {
            get { return ":"; }

            //get { return "slider=" + _scaleSlider.slider.value.ToString("F4") + ";"; }
        }

        public string SliderSubstitution
        {
            get { return ":"; }
            //get { return _scaleSlider.slider.value.ToString("F4"); }
        }

        public string SAMResult
        {
            get { return SAM.Result; }
        }

        public string KeypadResult
        {
            get { return ":"; }
            //get { return "keypad=" + _keypad.Value + ";"; }
        }

        public string ParamResult
        {
            get { return ":"; }
            //get { return "param=" + paramSlider.Value + ";"; }
        }

        public string ParamSubstitution
        {
            get { return ":"; }
            //get { return paramSlider.Value.ToString(); }
        }

        public string TraceResult
        {
            get { return ":"; }
            //get { return "trace=" + grapher.Result; }
        }
        
    }
} 