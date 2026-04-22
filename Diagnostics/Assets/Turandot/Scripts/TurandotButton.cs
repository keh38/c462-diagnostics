using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Turandot;
using Turandot.Inputs;
using Turandot.Screen;
using UnityEngine.Windows;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Turandot.Scripts
{
    public class TurandotButton : TurandotInput, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMPro.TMP_Text _label;

        [SerializeField] private Sprite _circle;

        private UnityEngine.UI.Button _button;
        private Color _normalColor;
        private Color _disabledColor;

        public ButtonData Data { get; private set; }
        
        private ButtonLayout _layout;
        private Turandot.Inputs.Button _buttonAction;
        private InputAction _pressAction;

        private Rect _myRect;

        private bool _value;
        private bool _lastvalue;

        public override string Name { get { return _layout.Name; } }

        private void Awake()
        {
            _button = GetComponent<UnityEngine.UI.Button>();
            _normalColor = _button.colors.normalColor;
            _disabledColor = _button.colors.disabledColor;
        }

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

            //SetBinding("");
        }

        private void OnDestroy()
        {
            ClearBinding();
        }

        private void LayoutControl()
        {
            _label.fontSize = _layout.FontSize;
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
            _buttonAction = (Turandot.Inputs.Button)input;

            _button.interactable = input.Enabled == EnabledState.Enabled;
            if (input.Enabled == EnabledState.Enabled) { }
            else
            {
                var cblock = _button.colors;
                if (input.Enabled == EnabledState.Disabled)
                {
                    cblock.disabledColor = _normalColor;
                }
                else if (input.Enabled == EnabledState.Grayed)
                {
                    cblock.disabledColor = _disabledColor;
                }
                _button.colors = cblock;
            }

            base.Activate(input, audio);

            if (_buttonAction.NumFlash > 0)
            {
                StartCoroutine(FlashCue());
            }
            else if (_buttonAction.Delay_ms > 0)
            {
                StartCoroutine(DelayCue());
            }
            else
            {
                ShowButton(_buttonAction.BeginVisible);
            }
        }

        public override void Deactivate()
        {
            _value = false;
            Data.value = false;
            base.Deactivate();
        }

        public void SetBinding(string bindingPath)
        {
            // Clean up any existing action
            ClearBinding();

            // e.g. bindingPath = "<Keyboard>/space" or "<Gamepad>/buttonSouth"
            _pressAction = new InputAction("HoldButton", InputActionType.Button, bindingPath);

            _pressAction.started += _ => HandlePressStart();
            _pressAction.canceled += _ => HandlePressEnd();
        }

        public void AddBinding(string bindingPath)
        {
            // Call this to support multiple bindings on the same button
            // e.g. keyboard AND gamepad both bound to the same button
            _pressAction?.AddBinding(bindingPath);
        }

        public void ClearBinding()
        {
            if (_pressAction != null)
            {
                _pressAction.Disable();
                _pressAction.Dispose();
                _pressAction = null;
            }
        }

        // ---- Pointer input (via EventSystem) ----

        public void OnPointerDown(PointerEventData eventData)
        {
            HandlePressStart();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            HandlePressEnd();
        }

        private void HandlePressStart()
        {
            _value = true;
            Data.value = _value;
        }

        private void HandlePressEnd()
        {
            _value = false;
            Data.value = _value;
        }

        private IEnumerator DelayCue()
        {
            ShowButton(false);
            yield return new WaitForSeconds(_buttonAction.Delay_ms * 0.001f);
            ShowButton(true);
        }

        private void ShowButton(bool show)
        {
            _image.enabled = show;
            _label.enabled = show;

            if (show)
            {
                _pressAction?.Enable();
            }
            else
            {
                _pressAction?.Disable();
            }
        }

        private IEnumerator FlashCue()
        {
            float onTime = _buttonAction.Duration_ms * 0.001f;
            float offTime = (_buttonAction.Interval_ms - _buttonAction.Duration_ms) * 0.001f;

            ShowButton(false);
            if (_buttonAction.Delay_ms > 0)
            {
                yield return new WaitForSeconds(_buttonAction.Delay_ms * 0.001f);
            }

            for (int k = 0; k < _buttonAction.NumFlash; k++)
            {
                ShowButton(true);
                yield return new WaitForSeconds(onTime);

                ShowButton(false);
                if (offTime != float.PositiveInfinity)
                {
                    yield return new WaitForSeconds(offTime);
                }
            }

            if (_buttonAction.BeginVisible && offTime != float.PositiveInfinity)
            {
                ShowButton(true);
            }
        }


    }

}