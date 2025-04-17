using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Turandot.Inputs;
using Input = Turandot.Inputs.Input;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotInput : MonoBehaviour
    {

        public TurandotButton button;
        Input _input = null;

        virtual public string Name { get { return ""; } }
        virtual public string Result { get { return ""; } }

        protected InputLog _log = null;
        public InputLog Log { get { return _log; } }
            
        public void Initialize()
        {
        }

        virtual public void Activate(Input input, TurandotAudio audio)
        {
            _input = input;

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
            gameObject.SetActive(visible);
        }


    }
}