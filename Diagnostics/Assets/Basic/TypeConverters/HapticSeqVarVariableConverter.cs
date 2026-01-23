using static System.ComponentModel.TypeConverter;
using System.Collections.Generic;
using System.ComponentModel;

namespace LDL.Haptics
{
    public class HapticSeqVarVariableConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            // Return the predefined string values.
            return new StandardValuesCollection(new List<string>
            {
                "Delay_ms",
                "Duration_ms",
                "Frequency_Hz",
                "Level"
            });
        }
    }
}
