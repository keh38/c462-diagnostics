using System.Collections.Generic;
using System;
using System.ComponentModel;
using NUnit.Framework.Internal;
using System.Globalization;
using KLib.TypeConverters;

using LDL;
using LDL.Haptics;

namespace SpeechReception
{
    public class MatrixTestTypeConverter : SortableExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return GetSortedProperties(value, attributes);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MatrixTest))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) && value is MatrixTest test)
            {
                return test.Mode.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(System.String))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                var test = (context.Instance as ListProperties).MatrixTest;
                test.Mode = (MatrixTestMode)Enum.Parse(typeof(MatrixTestMode), value as string);
                return test;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(Enum.GetNames(typeof(Order)));
        }


    }
}