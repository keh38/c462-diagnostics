using System.Collections.Generic;
using System;
using System.ComponentModel;
using NUnit.Framework.Internal;
using System.Globalization;
using KLib.TypeConverters;

namespace SpeechReception
{
    public class ListPropertiesConverter : SortableExpandableObjectConverter
    {
        public static TestType TestType = TestType.OpenSet;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var list = value as ListProperties;
            if (list != null)
            {
                var sortedProperties = GetSortedProperties(value, attributes);
                List<PropertyDescriptor> filteredProperties = new List<PropertyDescriptor>();
                foreach (PropertyDescriptor prop in sortedProperties)
                {
                    if (prop.Category == "Masker" && TestType == TestType.QuickSIN) { }
                    else if (prop.Name == "SNR" && TestType == TestType.QuickSIN) { }
                    else if (prop.Name == "ClosedSet" && TestType != TestType.ClosedSet) { }
                    else if (prop.Name == "Sequence" && TestType != TestType.ClosedSet) { }
                    else if (prop.Name == "MatrixTest" && TestType != TestType.Matrix) { }
                    else
                    {
                        filteredProperties.Add(prop);
                    }
                }
                return new PropertyDescriptorCollection(filteredProperties.ToArray()).Sort(GetSortedPropertyOrder(value, attributes));
            }
            return GetSortedProperties(value, attributes);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return base.ConvertFrom(context, culture, value);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return false;
        }

    }
}