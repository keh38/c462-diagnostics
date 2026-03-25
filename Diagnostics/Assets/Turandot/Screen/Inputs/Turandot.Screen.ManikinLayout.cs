using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Turandot.Screen
{
    [Serializable]
    public class ManikinLayout : InputLayout
    {
        public bool RandomizeOrder { get; set; }
        private bool ShouldSerializeRandomizeOrder() { return false; }

        public int SliderSpacing { get; set; }
        private bool ShouldSerializeSliderSpacing() { return false; }

        public int ButtonSpacing { get; set; }
        private bool ShouldSerializeButtonSpacing() { return false; }

        public int ButtonWidth { get; set; }
        private bool ShouldSerializeButtonWidth() { return false; }

        public int ButtonHeight { get; set; }
        private bool ShouldSerializeButtonHeight() { return false; }

        public float SliderHeight { get; set; }
        private bool ShouldSerializeSliderHeight() { return false; }

        public float SliderWidth { get; set; }
        private bool ShouldSerializeSliderWidth() { return false; }

        public int SliderVerticalOffset { get; set; }
        private bool ShouldSerializeSliderVerticalOffset() { return false; }
        
        public float SliderFontSize { get; set; }
        private bool ShouldSerializeSliderFontSize() { return false; }

        //[Editor(typeof(ManikinCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public ManikinCollection Manikins { get; set; }

        public ManikinLayout()
        {
            Name = "Manikins";
            Manikins = new ManikinCollection();
            RandomizeOrder = false;
            SliderFontSize = 42;
            SliderHeight = 75;
            SliderWidth = 1;
            SliderVerticalOffset = 0;
            SliderSpacing = 50;
            ButtonSpacing = 50;
            ButtonWidth = 200;
            ButtonHeight = 100;
        }

    }
}
