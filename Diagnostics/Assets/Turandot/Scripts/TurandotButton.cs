using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Turandot;
using Turandot.Inputs;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotButton : TurandotInput
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMPro.TMP_Text _label;

        [SerializeField] private Sprite _circle;

        public ButtonData Data { get; private set; }
        
        private ButtonLayout _layout;
        private Rect _myRect;

        private bool _value;
        private bool _lastvalue;
        private bool _isVisible = true;
        private bool _enabled = true;

        public override string Name { get { return _layout.Name; } }

        public void Initialize(ButtonLayout layout)
        {
            _layout = layout;
            LayoutControl();

            Data = new ButtonData() { name = _layout.Name };
            var rt = this.gameObject.GetComponent<RectTransform>();

            Vector2 position = new Vector2(
                rt.anchorMin.x * UnityEngine.Screen.width - rt.rect.width/2, 
                rt.anchorMin.y * UnityEngine.Screen.height - rt.rect.height/2);

            _myRect = new Rect(position.x, position.y, rt.rect.width, rt.rect.height);
        }

        private void LayoutControl()
        {
            _label.text = _layout.Label;

            if (_layout.Style == ButtonLayout.ButtonStyle.Circle)
            {
                _image.sprite = _circle;
            }

            var rt = GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(_layout.X, _layout.Y);
            rt.anchorMax = new Vector2(_layout.X, _layout.Y);
            rt.sizeDelta = new Vector2(_layout.Width, _layout.Height);
        }

        override public void Activate(Turandot.Inputs.Input input, TurandotAudio audio)
        {
            //Message m = cue as Message;

            //_label.text = m.Text;
            ////ChangeAppearance(m);

            base.Activate(input, audio);
        }

        public override void Deactivate()
        {
            _value = false;
            Data.value = false;
            base.Deactivate();
        }

#if UNITY_EDITOR
        public void FixedUpdate()
#else
        public void OnGUI()
#endif
        {
            //if (!_isVisible || !_enabled) return;

#if UNITY_EDITOR
            _lastvalue = _value;

            //_value = !_isHidden && UnityEngine.Input.GetMouseButton(0) && rect.Contains(UnityEngine.Input.mousePosition);
            _value = UnityEngine.Input.GetMouseButton(0) && _myRect.Contains(UnityEngine.Input.mousePosition);

            //if (_useXbox)
            //{
            //    _value |= UnityEngine.Input.GetButton(_xboxControl);
            //}

#else

            _lastvalue = _value;
            _value = UnityEngine.Input.GetMouseButton(0) && _myRect.Contains(UnityEngine.Input.mousePosition);
            //if (_useXbox)
            //{
            //    _value |= UnityEngine.Input.GetButtonDown(_xboxControl);
            //}

#endif
            if (_layout.KeyCode != KeyCode.None && UnityEngine.Input.GetKeyDown(_layout.KeyCode))
            {
                _value = true;
            }

            Data.value = _value;

            //if (_value && !_lastvalue && _showPress) StartCoroutine(ShowButtonPress());
        }


        // TURANDOT FIX
        /*
        [System.NonSerialized]
        public Rect rect;

        [System.NonSerialized]
        public Turandot.ButtonData data = new Turandot.ButtonData();

        private Text _label;
        private bool _value;
        private bool _lastvalue;
        private bool _isVisible = true;
        private bool _enabled = true;

        private float _scaleFactor;
        private float _width;
        private float _height;

        private UISprite _sprite;

        private Button _input = null;
        private InputLog _log = null;

        private bool _tweenActive = false;
        private float _tweenRiseTime;
        private float _tweenOnTime;
        private float _tweenEndTime;
        private float _tweenTime;
        private float _tweenRate;
        private float _tweenScale;

        private Color _baseSpriteColor;
        private Color _buttonDownColor;

        private Vector2 _baseLocation;

        private bool _isHidden = false;

        private bool _useXbox = false;
        private string _xboxControl = "";

        private bool _showPress;

        private bool _useKeyboard;
        public KeyCode _keyboardValue;

        public Texture tex;

        void Awake()
        {
            _label = gameObject.GetComponentInChildren<UILabel>();

            _sprite = GetComponent<UISprite>();

            if (_sprite != null)
            {
                _baseSpriteColor = _sprite.color;

                UIRoot uiRoot = GameObject.FindObjectOfType<UIRoot>();
                _scaleFactor = (float)UnityEngine.Screen.height / (float)uiRoot.manualHeight;
                _scaleFactor /= uiRoot.GetComponentInChildren<Camera>().orthographicSize;

                _width = _sprite.width;
                _height = _sprite.height;

                data.name = name;
                _log = new InputLog(name);
                Move();
            }
            else
            {
                _isVisible = false;
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                data.value = false;
                _isVisible = value;
                if (_sprite != null) _sprite.alpha = _isVisible ? 1 : 0;
            }
        }
        public bool IsPressed { get { return data.value; } }
        public int Width { get { return _sprite == null ? -1 : _sprite.width; } }
        public string Label { get { return _label.text; } }

        public void Initialize(ButtonSpec buttonSpec)
        {
            data.name = name;
            _log = new InputLog(name);

            _baseLocation = transform.localPosition;
            _label.text = buttonSpec.label.Replace(@"\n", System.Environment.NewLine);
            _isVisible = !string.IsNullOrEmpty(_label.text);
            _sprite.alpha = 0;
            _tweenActive = false;
            _showPress = buttonSpec.showPress;

            _useKeyboard = buttonSpec.useKeyboard;
            _keyboardValue = buttonSpec.keyboardValue; 

            if (buttonSpec.xbox.control == XboxLink.ControlSource.None)
            {
                _useXbox = false;
                _xboxControl = "";
            }
            else
            {
                _useXbox = true;
                _xboxControl = "Xbox" + buttonSpec.xbox.control.ToString();
            }
        }

        public void SetLabelFontSize(int size)
        {
            _label.fontSize = size;
            _label.transform.localPosition = Vector3.zero;
        }

        public void ApplySkin(Skin skin)
        {
            _sprite.color = skin.baseColor;
            _baseSpriteColor = skin.baseColor;
            _buttonDownColor = skin.buttonDownColor;

            _label.color = skin.buttonTextColor;
        }

        public void SetSize(int width, int height)
        {
            _sprite.width = width;
            _sprite.height = height;

            _width = width;
            _height = height;

            float w = _width * _scaleFactor;
            float h = _height * _scaleFactor;

            _label.width = width - 5;
            _label.height = height - 5;

            Vector3 pos = gameObject.transform.parent.transform.localPosition + transform.localPosition;

            rect = new Rect(
                UnityEngine.Screen.width / 2 + pos.x * _scaleFactor - w / 2,
                UnityEngine.Screen.height / 2 + pos.y * _scaleFactor - h / 2,
                w, h
                );
        }

        public void Move()
        {
            if (_sprite == null || gameObject.transform.parent == null) return;

            float w = _width * _scaleFactor;
            float h = _height * _scaleFactor;

            Vector3 pos = gameObject.transform.parent.transform.localPosition + transform.localPosition;

            rect = new Rect(
                UnityEngine.Screen.width / 2 + pos.x * _scaleFactor - w / 2,
                UnityEngine.Screen.height / 2 + pos.y * _scaleFactor - h / 2,
                w, h
                );
        }

        public void SetX(float x)
        {
            Vector2 pos = transform.localPosition;
            transform.localPosition = new Vector2(x, pos.y);
            _baseLocation = transform.localPosition;
            Move();
        }

        public void SetState(bool state)
        {
            data.value = state;
        }

#if UNITY_EDITOR
        public void FixedUpdate()
#else
        public void OnGUI()
#endif
        {
            if (!_isVisible || !_enabled) return;

#if UNITY_EDITOR
            _lastvalue = _value;

            _value = !_isHidden && UnityEngine.Input.GetMouseButton(0) && rect.Contains(UnityEngine.Input.mousePosition);
            //_value = UnityEngine.Input.GetMouseButton(0) && rect.Contains(UnityEngine.Input.mousePosition);

            //if (_useXbox)
            //{
            //    _value |= UnityEngine.Input.GetButton(_xboxControl);
            //}

#else

            _lastvalue = _value;
            _value = UnityEngine.Input.GetMouseButton(0) && rect.Contains(UnityEngine.Input.mousePosition);
            if (_useXbox)
            {
                _value |= UnityEngine.Input.GetButtonDown(_xboxControl);
            }

#endif
            if (_useKeyboard && _keyboardValue != KeyCode.None && UnityEngine.Input.GetKeyDown(_keyboardValue))
            {
                _value = true;
            }

            data.value = _value;

            if (_value && !_lastvalue && _showPress) StartCoroutine(ShowButtonPress());
        }

        private IEnumerator ShowButtonPress()
        {
            _sprite.color = _buttonDownColor;
            //_sprite.transform.localPosition = new Vector2(_baseLocation.x + 10, _baseLocation.y - 10);

            yield return new WaitForSeconds(0.4f);

            _sprite.color = _baseSpriteColor;
            //_sprite.transform.localPosition = _baseLocation;
        }

        public void Activate()
        {
            Activate(new Turandot.Inputs.Button());
        }

        public void Activate(Turandot.Inputs.Input input)
        {
            _input = input as Button;

            if (!string.IsNullOrEmpty(_input.label))
                _label.text = _input.label.Replace(@"\n", System.Environment.NewLine);

            _enabled = input.enabled;
            _tweenActive = false;
            if (!_enabled) data.value = false;

            if (_input.numFlash > 0)
            {
                if (_input.tweenScale) StartCoroutine(TweenCue());
                else StartCoroutine(FlashCue());
            }
            else if (_input.delay_ms > 0)
            {
                StartCoroutine(DelayCue());
            }
            else
            {
                if (_input.changeAppearance) ChangeAppearance();
                ShowCue(_input.startVisible);
            }
        }

        public void Deactivate()
        {
            if (_input != null)
            {
                _tweenActive = false;
                StopAllCoroutines();
                ShowCue(_input.endVisible);
            }
            _value = false;
            data.value = false;
            _input = null;
        }

        public void ClearLog()
        {
            _log.Clear();
        }

        public void Enable()
        {
            _sprite.color = _baseSpriteColor;
            _enabled = true;
        }

        public void Disable()
        {
            _sprite.color = Color.gray;
            _enabled = false;
        }

        void ShowCue(bool visible)
        {
            //        transform.localPosition = _baseLocation;
            _isVisible = visible;
            _sprite.color = _baseSpriteColor;
            _sprite.alpha = (visible && !_isHidden) ? 1 : 0;
            _log.Add(Time.timeSinceLevelLoad, _sprite.alpha * 0.85f);
        }

        public void MakeHidden()
        {
            _isHidden = true;
            _sprite.alpha = 0;
            _label.text = "";
        }

        private IEnumerator DelayCue()
        {
            ShowCue(false);
            yield return new WaitForSeconds(_input.delay_ms * 0.001f);
            if (_input.changeAppearance) ChangeAppearance();
            ShowCue(_input.startVisible);
        }

        private IEnumerator FlashCue()
        {
            float onTime = _input.duration_ms * 0.001f;
            float offTime = (_input.interval_ms - _input.duration_ms) * 0.001f;

            _sprite.alpha = 0;
            _log.Add(Time.timeSinceLevelLoad, _sprite.alpha * 0.85f);

            if (_input.delay_ms > 0) yield return new WaitForSeconds(_input.delay_ms * 0.001f);

            for (int k = 0; k < _input.numFlash; k++)
            {
                _sprite.alpha = 1;
                _log.Add(Time.timeSinceLevelLoad, _sprite.alpha * 0.85f);
                yield return new WaitForSeconds(onTime);

                _sprite.alpha = 0;
                _log.Add(Time.timeSinceLevelLoad, _sprite.alpha * 0.85f);
                if (offTime != float.PositiveInfinity) yield return new WaitForSeconds(offTime);
            }

            if (_input.startVisible && offTime != float.PositiveInfinity)
            {
                _sprite.alpha = 1;
                _log.Add(Time.timeSinceLevelLoad, _sprite.alpha * 0.85f);
            }

            if (_input.changeAppearance) ChangeAppearance();
        }

        void ChangeAppearance()
        {
            _sprite.color = KLib.Unity.ColorFromARGB(_input.color);
            _baseSpriteColor = _sprite.color;
        }

        void Update()
        {
            if (_tweenActive)
            {
                if (_tweenTime < 0)
                {
                    _tweenScale = 1;
                    _tweenTime = 0;
                }
                else if (_tweenTime < _tweenRiseTime)
                {
                    _tweenScale += Time.deltaTime * _tweenRate;
                    _tweenTime += Time.deltaTime;
                }
                else if (_tweenTime < _tweenEndTime - _tweenRiseTime)
                {
                    _tweenScale = _input.scaleTo;
                    _tweenTime += Time.deltaTime;
                }
                else if (_tweenTime < _tweenEndTime)
                {
                    _tweenScale -= Time.deltaTime * _tweenRate;
                    _tweenTime += Time.deltaTime;
                }
                else
                {
                    _tweenScale = 1;
                    _tweenActive = false;
                }

                _sprite.transform.localScale = new Vector2(_tweenScale, _tweenScale);
            }
        }

        private IEnumerator TweenCue()
        {
            _tweenRiseTime = 0.25f;
            _tweenEndTime = _input.duration_ms * 0.001f;
            _tweenOnTime = _tweenEndTime - 2 * _tweenRiseTime;
            float totalTime = _input.interval_ms * 0.001f - _tweenOnTime;
            _tweenRate = (_input.scaleTo - 1) / _tweenRiseTime;

            _sprite.alpha = 1;

            if (_input.delay_ms > 0) yield return new WaitForSeconds(_input.delay_ms * 0.001f);

            for (int k = 0; k < _input.numFlash; k++)
            {
                yield return null;

                _tweenTime = -1;
                _tweenActive = true;
                if (totalTime != float.PositiveInfinity) yield return new WaitForSeconds(totalTime);
            }
            if (_input.changeAppearance) ChangeAppearance();
        }

        public bool HaveLog
        {
            get { return _log.Length > 0; }
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