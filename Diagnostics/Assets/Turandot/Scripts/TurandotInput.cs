using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Input = Turandot.Inputs.Input;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotInput : MonoBehaviour
    {
        public Text label;
        public TurandotButton button;

        Input _input = null;

        public void Initialize()
        {
        }

        virtual public void Activate(Input input)
        {
            _input = input;

            if (label != null) label.text = _input.label;
            ShowInput(input.BeginVisible);
        }

        virtual public void Deactivate()
        {
            if (_input != null)
            {
                //StopAllCoroutines();
                ShowInput(_input.EndVisible);
            }
        }

        void ShowInput(bool visible)
        {
            transform.localPosition = new Vector2(_input.X, visible ? _input.Y : -5000);
            //button.Move();
        }


    }
}