using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turandot.Inputs
{ 
    public class ParameterSliderProperties
    {
        public enum SliderScale { Linear, Log};

        [Browsable(false)]
        public string Channel { set; get; }
        [Browsable(false)]
        public string Property { set; get; }

        public SliderScale Scale { set; get; }
        public float MinValue { set; get; }
        public float MaxValue { set; get; } = 1;
        public float StartValue { set; get; }
        public bool ShowDigital { set; get; }
        public string DisplayFormat { set; get; }
    }
}
