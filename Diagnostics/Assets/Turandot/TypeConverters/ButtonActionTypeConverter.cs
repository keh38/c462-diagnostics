using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using KLib.TypeConverters;

using Turandot.Inputs;

namespace Turandot.TypeConverters
{
    public class ButtonActionTypeConverter : SortableExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var button = value as Button;
            if (button != null)
            {
                var sortedProperties = GetSortedProperties(value, attributes);
                List<PropertyDescriptor> filteredProperties = new List<PropertyDescriptor>();
                foreach (PropertyDescriptor prop in sortedProperties)
                {
                    if (button.NumFlash == 0 && (prop.Name == "Duration_ms" || prop.Name == "Interval_ms")) { }
                    else
                    {
                        filteredProperties.Add(prop);
                    }
                }
                return new PropertyDescriptorCollection(filteredProperties.ToArray()).Sort(GetSortedPropertyOrder(value, attributes));
            }
            return GetSortedProperties(value, attributes);
        }

    }
}