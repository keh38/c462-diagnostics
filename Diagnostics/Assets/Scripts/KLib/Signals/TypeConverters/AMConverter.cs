using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using KLib.TypeConverters;
using KLib.Signals.Enumerations;

namespace KLib.Signals.Modulations
{
    public class AMConverter : SortableExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var am = value as AM;
            if (am != null)
            {
                var sortedProperties = GetSortedProperties(value, attributes);
                List<PropertyDescriptor> filteredProperties = new List<PropertyDescriptor>();
                foreach (PropertyDescriptor prop in sortedProperties)
                {
                    if (am.Shape == AMShape.None) { }
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
            if (destinationType == typeof(AM))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) && value is AM)
            {
                if ((value as AM).Shape == AMShape.None) return "OFF";

                return (value as AM).Shape.ToString();
            }
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
            if (value is string)
            {
                return AMFactory.Create(value as string);
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { "OFF", "SAM" });
        }
    }
}