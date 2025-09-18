using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using KLib.TypeConverters;

namespace LDL
{
    public class HapticStimulusConverter : SortableExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var stim = value as HapticStimulus;
            if (stim != null)
            {
                var sortedProperties = GetSortedProperties(value, attributes);
                List<PropertyDescriptor> filteredProperties = new List<PropertyDescriptor>();
                foreach (PropertyDescriptor prop in sortedProperties)
                {
                    if (prop.Name == "Source")
                    {
                        filteredProperties.Add(prop);
                    }
                    else if (stim.Source == HapticSource.NONE) { }
                    else if (stim.Source == HapticSource.TENS && prop.Name == "Vibration") { }
                    else if (stim.Source == HapticSource.Vibration && prop.Name == "TENS") { }
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
            if (destinationType == typeof(HapticStimulus))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) && value is HapticStimulus)
            {
                return (value as HapticStimulus).Source.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) { return false; }
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return false; }
    }
}