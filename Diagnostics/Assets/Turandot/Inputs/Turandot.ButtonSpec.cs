using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Turandot
{
    [Serializable]
    public class ButtonSpec
    {
        public enum ButtonStyle { None, Circle, Square, Rectangle, Right, Left};

        public string name = "";
        public string label = "";
        public int size = 200;
        public int height = 150;
        public int x = 0;
        public int y = 0;
        public ButtonStyle style = ButtonStyle.Square;
        public XboxLink xbox = new XboxLink();
        public bool showPress = true;
        public bool useKeyboard = false;
        public KeyCode keyboardValue = KeyCode.A;

        public ButtonSpec() { }
        public ButtonSpec(string name, string label)
        {
            this.name = name;
            this.label = label;
        }
    }
}
