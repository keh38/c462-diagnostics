using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turandot.Inputs
{ 
    public class ParameterSliderProperties
    {
        public enum SliderScale { Linear, Log};

        public string Channel { set; get; }

        public string Property { set; get; }

        public SliderScale Scale { set; get; }
        private bool ShouldSerializeScale() { return false; }

        public float MinValue { set; get; }
        private bool ShouldSerializeMinValue() { return false; }

        public float MaxValue { set; get; } = 1;
        private bool ShouldSerializeMaxValue() { return false; }

        public float StartValue { set; get; }
        private bool ShouldSerializeStartValue() { return false; }

        public bool ShowDigital { set; get; }
        private bool ShouldSerializeShowDigital() { return false; }

        public string DisplayFormat { set; get; } = "F";
        private bool ShouldSerializeDisplayFormat() { return false; }

        public string Label { set; get; }
        private bool ShouldSerializeLabel() { return false; }

        public string FullParameterName { get { return $"{Channel}.{Property}"; } }

        public ParameterSliderProperties() { }
    }
}
