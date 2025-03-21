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

//        [Browsable(false)]
        [Category("Signal parameter")]
        [ReadOnly(true)]
        public string Channel { set; get; }
        [Browsable(false)]
        public string Property { set; get; }

        //public string Group { set; get; }

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

        private bool ShouldSerializeParameterSliderProperties()
        {
            //RETURN:
            //      = true if the property value should be displayed in bold, or "treated as different from a default one"
            return false;
        }
    }
}
