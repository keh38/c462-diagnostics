using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using KLib.TypeConverters;

namespace KLib.Signals.Waveforms
{
    public class DigitimerConverter : SortableExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var digitimer = value as Digitimer;
            if (digitimer != null)
            {
                var sortedProperties = GetSortedProperties(value, attributes);
                List<PropertyDescriptor> filteredProperties = new List<PropertyDescriptor>();
                foreach (PropertyDescriptor prop in sortedProperties)
                {
                    if (digitimer.Source == Digitimer.DemandSource.External && prop.Name == "Demand") { }
                    else
                    {
                        filteredProperties.Add(prop);
                    }
                }
                return new PropertyDescriptorCollection(filteredProperties.ToArray()).Sort(GetSortedPropertyOrder(value, attributes));
            }
            return GetSortedProperties(value, attributes);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(System.String) && value is Digitimer)
            {
                return (value as Digitimer).PulseRate_Hz.ToString() + " pps";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

    }
}