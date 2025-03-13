using UnityEngine;
using System.Collections;

using Turandot.Inputs;
using Turandot.Screen;
using Input = Turandot.Inputs.Input;

namespace Turandot.Scripts
{
    public class TurandotThumbSlider : TurandotInput
    {
        // FIX TURANDOT
        /*
        public KUISlider slider;
        public KUISlider leftThumb;
        public KUISlider rightThumb;

        private InputLog _log = new InputLog("thumbslider");
        private bool _useButton = false;
        private bool _isEnabled = false;

        private bool leftSelected = false;
        private bool rightSelected = false;

        public void Initialize(ThumbSliderLayout layout)
        {
            leftThumb.ThumbOnly = true;
            rightThumb.ThumbOnly = true;

            leftThumb.OnSelectNotifier = OnLeftSliderSelected;
            rightThumb.OnSelectNotifier = OnRightSliderSelected;

            leftThumb.transform.localPosition = new Vector2(-layout.thumbX, layout.thumbY);
            rightThumb.transform.localPosition = new Vector2(layout.thumbX, layout.thumbY);
        }

        public void ClearLog()
        {
            _log.Clear();
        }

        override public void Activate(Input input)
        {
            ThumbSliderAction action = input as ThumbSliderAction;
            slider.value = action.value;
            leftThumb.value = action.value;
            rightThumb.value = action.value;

            button.IsVisible = false;
            _isEnabled = true;

            _log.Add(Time.timeSinceLevelLoad, float.NaN);

            base.Activate(input);
        }

        override public void Deactivate()
        {
            base.Deactivate();
        }

        public override void ApplySkin(Skin skin)
        {
            base.ApplySkin(skin);

            slider.foregroundWidget.GetComponent<UISprite>().color = skin.sliderForegroundColor;
            slider.GetComponent<UISprite>().color = skin.sliderBackgroundColor;
        }

        private void OnLeftSliderSelected(bool selected)
        {
            leftSelected = selected && !rightSelected;
            rightThumb.enabled = !leftSelected;
        }

        private void OnRightSliderSelected(bool selected)
        {
            rightSelected = selected && !leftSelected;
            leftThumb.enabled = !rightSelected;
        }

        public void LeftSliderMoved()
        {
            if (_isEnabled && leftSelected)
            {
                slider.value = leftThumb.value;
                rightThumb.value = leftThumb.value;
                _log.Add(Time.timeSinceLevelLoad, leftThumb.value);
            }
        }

        public void RightSliderMoved()
        {
            if (_isEnabled && rightSelected)
            {
                slider.value = rightThumb.value;
                leftThumb.value = rightThumb.value;
                _log.Add(Time.timeSinceLevelLoad, rightThumb.value);
            }
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