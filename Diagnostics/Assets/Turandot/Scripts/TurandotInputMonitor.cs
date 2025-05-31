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
        [SerializeField] private GameObject _buttonPrefab;
        [SerializeField] private GameObject _checklistPrefab;
        [SerializeField] private GameObject _sliderPrefab;
        [SerializeField] private GameObject _manikinPrefab;

        bool _isRunning = false;
        List<ButtonData> _buttonData = new List<ButtonData>();
        List<ScalarData> _scalarData = new List<ScalarData>();
        EventLog _log = new EventLog();

        int[] _controlValues;
        int[] _eventValues;

        List<InputEvent> _inputEvents;
        List<TurandotInput> _inputObjects;
        List<Input> _currentStateInputs;

        public delegate void EventChangedDelegate(string eventName);
        public EventChangedDelegate EventChanged;
        private void OnEventChanged(string eventName) { EventChanged?.Invoke(eventName); }

        List<Flag> _flags;
        int _numButtons = 0;

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
                    OnEventChanged(ie.name);
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
                    OnEventChanged(ie.name);
                }
            }
        }

        public void ClearScreen()
        {
            if (_inputObjects == null) return;
            foreach (var i in _inputObjects) i.gameObject.SetActive(false);
        }

        public void Initialize(List<InputLayout> inputLayouts, List<InputEvent> inputEvents)
        {
            _isRunning = false;

            _scalarData.Clear();

            _inputObjects = new List<TurandotInput>();
            _buttonData = new List<ButtonData>();
            
            var canvasRT = GameObject.Find("Canvas").GetComponent<RectTransform>();
            foreach (var layout in inputLayouts)
            {
                if (layout is ButtonLayout)
                {
                    var gobj = GameObject.Instantiate(_buttonPrefab, canvasRT);
                    var i = gobj.GetComponent<TurandotButton>();
                    i.Initialize(layout as ButtonLayout);
                    _inputObjects.Add(i);
                    gobj.SetActive(false);
                    _buttonData.Add(i.Data);
                }
                else if (layout is ChecklistLayout)
                {
                    var gobj = GameObject.Instantiate(_checklistPrefab, canvasRT);
                    var i = gobj.GetComponent<TurandotChecklist>();
                    i.Initialize(layout as ChecklistLayout);
                    _inputObjects.Add(i);
                    gobj.SetActive(false);
                    _buttonData.Add(i.ButtonData);
                }
                else if (layout is ManikinLayout)
                {
                    var gobj = GameObject.Instantiate(_manikinPrefab, canvasRT);
                    var i = gobj.GetComponent<TurandotManikins>();
                    i.Initialize(layout as ManikinLayout);
                    _inputObjects.Add(i);
                    gobj.SetActive(false);
                    _buttonData.Add(i.ButtonData);
                }
                else if (layout is ParamSliderLayout)
                {
                    var gobj = GameObject.Instantiate(_sliderPrefab, canvasRT);
                    var i = gobj.GetComponent<TurandotParamSlider>();
                    i.Initialize(layout as ParamSliderLayout);
                    _inputObjects.Add(i);
                    gobj.SetActive(false);
                    _buttonData.Add(i.ButtonData);
                }
            }
            _log.Initialize(_buttonData.Select(b => b.name).ToArray(), inputEvents.Select(ie => ie.name).ToArray());
            _controlValues = new int[_buttonData.Count];

            _inputEvents = inputEvents;
            _eventValues = new int[inputEvents.Count];
        }

        public void Activate(List<Input> inputs, TurandotAudio audio, float timeOut)
        {
            _currentStateInputs = inputs;

            foreach (InputEvent ie in _inputEvents)
            {
                ie.Reset();
            }

            foreach (var i in inputs)
            {
                var target = _inputObjects.Find(x => x.Name.Equals(i.Target));
                target?.Activate(i, audio);
            }
        }

        public void Deactivate()
        {
            foreach (var i in _currentStateInputs)
            {
                var target = _inputObjects.Find(x => x.Name.Equals(i.Target));
                target?.Deactivate();
            }

            for (int k = 0; k < _inputObjects.Count; k++) _inputObjects[k].Deactivate();
            foreach (var ie in _inputEvents) ie.ClearRisingFalling();
        }

        public void StartMonitor(List<Flag> flags)
        {
            _flags = flags;
            foreach (InputEvent ie in _inputEvents)
            {
                ie.Reset();
            }

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

                foreach (var i in _inputObjects)
                {
                    if (i.Log != null)
                    {
                        json = KLib.FileIO.JSONStringAdd(json, i.Name, i.Log.JSONString);
                    }
                }

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

        public bool Contains(string item)
        {
            return _inputObjects.Find(x => x.Name.Equals(item)) != null;
        }

        public string ExpandResult(string item)
        {
            var i = _inputObjects.Find(x => x.Name.Equals(item));
            if (i == null) return "";

            return i.Result;
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

        //public string SAMResult
        //{
        //    get { return SAM.Result; }
        //}

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