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
        [SerializeField] private GameObject _fixedSliderPrefab;
        [SerializeField] private GameObject _mobileSliderPrefab;
        [SerializeField] private GameObject _manikinPrefab;
        [SerializeField] private GameObject _scalerPrefab;

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
                    var i = gobj.GetComponent<TurandotManikinPanel>();
                    _inputObjects.Add(i);
                    i.Initialize(layout as ManikinLayout);
                    gobj.SetActive(false);
                    _buttonData.Add(i.ButtonData);
                }
                else if (layout is ParamSliderLayout)
                {
                    var paramSliderLayout = (ParamSliderLayout)layout;
                    GameObject prefab = (paramSliderLayout.ButtonStyle == ParamSliderButtonStyle.Fixed) ? _fixedSliderPrefab : _mobileSliderPrefab;
                    var gobj = GameObject.Instantiate(prefab, canvasRT);
                    var i = gobj.GetComponent<TurandotParamSlider>();
                    i.Initialize(layout as ParamSliderLayout);
                    _inputObjects.Add(i);
                    gobj.SetActive(false);
                    _buttonData.Add(i.ButtonData);
                }
                else if (layout is ScalerLayout)
                {
                    var gobj = GameObject.Instantiate(_scalerPrefab, canvasRT);
                    var i = gobj.GetComponent<TurandotScaler>();
                    i.Initialize(layout as ScalerLayout);
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

            foreach (var b in _buttonData)
            {
                b.value = false;
            }

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

        public float GetValue(string item)
        {
            var i = _inputObjects.Find(x => x.Name.Equals(item));
            if (i == null) return float.NaN;

            return i.Value;
        }

    }
} 