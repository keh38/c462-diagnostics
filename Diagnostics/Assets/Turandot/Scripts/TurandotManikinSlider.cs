using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using Turandot.Inputs;
using UnityEngine.EventSystems;

namespace Turandot.Scripts
{
    public class TurandotManikinSlider : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private GameObject _thumb;

        public bool Moved { get; private set; }

        public delegate void ValueChangedDelegate(float value);
        public event ValueChangedDelegate ValueChanged;
        private void OnValueChanged(float value)
        {
            ValueChanged?.Invoke(value);
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
            _slider.value = -1;
            _thumb.SetActive(false);
            Moved = false;
        }

        public int Value { get { return (int) _slider.value; } }
   }
}