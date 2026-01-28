using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace SpeechReception
{
    public class ListPropertiesCollectionConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return false;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is List<ListProperties> myCollection)
            {
                if (myCollection.Count == 0)
                {
                    return "(no lists)";
                }

                return $"({myCollection.Count} list" + (myCollection.Count > 1 ? "s" : "") + ")";
            }

            return null; // base.ConvertTo(context, culture, value, destinationType);
        }
    }
}