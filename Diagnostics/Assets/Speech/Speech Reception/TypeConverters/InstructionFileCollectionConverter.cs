using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using KLib.TypeConverters;
using OrderedPropertyGrid;

namespace SpeechReception
{
    public class InstructionFileCollectionConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return false; // destinationType == typeof(string);// || base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false; // sourceType == typeof(string); //base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value is List<InstructionFile> myCollection)
            {
                if (myCollection.Count == 0)
                {
                    return "(no files)";
                }

                return $"({myCollection.Count} file" + (myCollection.Count > 1 ? "s" : "") + ")";
            }

            return null; // base.ConvertTo(context, culture, value, destinationType);
        }
    }
}