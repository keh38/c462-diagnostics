using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using Turandot.Inputs;
using UnityEngine.EventSystems;
using System.IO;
using Turandot.Cues;
using Turandot.Screen;
using JimmysUnityUtilities;
//using UnityEngine.UIElements;

namespace Turandot.Scripts
{
    public class TurandotManikinSlider : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text _label;
        [SerializeField] private Image _image;
        [SerializeField] private Slider _slider;
        [SerializeField] private GameObject _thumb;

        private float _startPosition = 0;

        public bool Moved { get; private set; }

        public delegate void ValueChangedDelegate(float value);
        public event ValueChangedDelegate ValueChanged;
        private void OnValueChanged(float value)
        {
            ValueChanged?.Invoke(value);
        }

        public RectTransform Layout(ManikinLayout layout, ManikinSpec manikinSpec, float yoffset)
        {
            _label.text = manikinSpec.Label;
            _label.fontSize = layout.SliderFontSize;

            _startPosition = layout.StartPosition;

            var imageRT = _image.gameObject.GetComponent<RectTransform>();
            var imageBottom = imageRT.anchoredPosition.y;
            float width = 0;

            if (!string.IsNullOrEmpty(manikinSpec.Image))
            {
                string imagePath = Path.Combine(FileLocations.LocalResourceFolder("Images"), manikinSpec.Image);
                Debug.Log(imagePath);

                var texture = new Texture2D(10, 10);
                texture.LoadImage(File.ReadAllBytes(imagePath));
                _image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                _image.SetNativeSize();

                imageBottom -= imageRT.rect.height;
                width = imageRT.rect.width;
            }
            else
            {
                _image.gameObject.SetActive(false);
            }

            var rt = _slider.gameObject.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, layout.SliderHeight);

            rt.anchoredPosition = new Vector2(0, imageBottom + layout.SliderVerticalOffset);
            var anchorOffset = (1 - layout.SliderWidth) / 2;
            rt.anchorMin = new Vector2(anchorOffset, 1);
            rt.anchorMax = new Vector2(1 - anchorOffset, 1);

            var sliderBottom = rt.anchoredPosition.y - rt.rect.height;

            var myHeight = -Mathf.Min(imageBottom, sliderBottom);
            var myRT = GetComponent<RectTransform>();
            if (width == 0)
            {
                width = myRT.rect.width;
            }

            myRT.sizeDelta = new Vector2(width, myHeight);
            myRT.anchoredPosition = new Vector2(0, -yoffset);

            return myRT;
        }

        public void OnSliderValueChanged(float sliderValue)
        {
            Moved = true;
            _thumb.SetActive(true);
            OnValueChanged(sliderValue);
        }
        public void OnPointerDown(BaseEventData data)
        {
            //_button.SetActive(false);
        }
        public void OnPointerUp(BaseEventData data)
        {
            Moved = true;
            _thumb.SetActive(true);
            OnValueChanged(0);
        }

        public void Reset()
        {
            _slider.value = _startPosition;
            //_thumb.SetActive(false);
            Moved = false;
        }

        public float Value { get { return _slider.value; } }
   }
}