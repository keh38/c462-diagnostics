using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Turandot
{
    public class ScalarData
    {
        public string name;
        public float value;
        public bool hasChanged = false;

        public ScalarData(string name)
        {
            this.name = name;
            value = 0;
        }

        public void SetValue(float value)
        {
            this.value = value;
            hasChanged = true;
        }
    }

}
