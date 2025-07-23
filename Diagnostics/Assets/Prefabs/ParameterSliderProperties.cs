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

        [Category("Signal parameter")]
        [ReadOnly(true)]
        public string Channel { set; get; }

        [Category("Signal parameter")]
        [ReadOnly(true)]
        public string Property { set; get; }

        [Category("Scale")]
        public SliderScale Scale { set; get; }
        private bool ShouldSerializeScale() { return false; }

        [Category("Scale")]
        public float MinValue { set; get; }
        private bool ShouldSerializeMinValue() { return false; }

        [Category("Scale")]
        public float MaxValue { set; get; } = 1;
        private bool ShouldSerializeMaxValue() { return false; }

        [Category("Scale")]
        public float StartValue { set; get; }
        private bool ShouldSerializeStartValue() { return false; }

        [Category("Appearance")]
        [Browsable(false)]
        public bool ShowDigital { set; get; }
        private bool ShouldSerializeShowDigital() { return false; }

        [Category("Appearance")]
        public string DisplayFormat { set; get; } = "F";
        private bool ShouldSerializeDisplayFormat() { return false; }

        [Category("Appearance")]
        public string Label { set; get; }
        private bool ShouldSerializeLabel() { return false; }

        [Browsable(false)]
        public string FullParameterName { get { return $"{Channel}.{Property}"; } }

        public ParameterSliderProperties() { }
    }
}
