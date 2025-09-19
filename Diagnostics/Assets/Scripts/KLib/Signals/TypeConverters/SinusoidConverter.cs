using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using KLib.TypeConverters;
using KLib.Signals.Enumerations;
using LDL;

namespace KLib.Signals.Waveforms
{
    public class SinusoidConverter : ExpandableObjectConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) && value is Sinusoid)
            {
                return (value as Sinusoid).Frequency_Hz.ToString() + " Hz";
            }
            return "";
        }

    }
}