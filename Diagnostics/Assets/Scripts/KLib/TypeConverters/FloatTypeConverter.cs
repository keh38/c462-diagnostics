// Example custom TypeConverter
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Xml.Linq;

namespace KLib.Signals
{
    class FloatTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            return value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value != null)
            {
                string expr = (string)value;
                if (expr.ToLower().Equals("inf"))
                {
                    return float.PositiveInfinity;
                }
                else
                {
                    float fval;
                    if (float.TryParse(expr, out fval))
                    {
                        return fval;
                    }
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

    }
}