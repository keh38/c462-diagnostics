using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OrderedPropertyGrid;

namespace KLib.TypeConverters
{
    public class SortableExpandableObjectConverter : ExpandableObjectConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext? context)
        {
            return true;
        }

        protected PropertyDescriptorCollection GetSortedProperties(object value, Attribute[]? attributes)
        {
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(value, attributes);
            return pdc.Sort(GetSortedPropertyOrder(value, attributes));
        }

        protected string[] GetSortedPropertyOrder(object value, Attribute[]? attributes)
        {
            //
            // This override returns a list of properties in order
            //
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(value, attributes);
            ArrayList orderedProperties = new ArrayList();
            foreach (PropertyDescriptor pd in pdc)
            {
                Attribute attribute = pd.Attributes[typeof(PropertyOrderAttribute)];
                if (attribute != null)
                {
                    //
                    // If the attribute is found, then create an pair object to hold it
                    //
                    PropertyOrderAttribute poa = (PropertyOrderAttribute)attribute;
                    orderedProperties.Add(new PropertyOrderPair(pd.Name, poa.Order));
                }
                else
                {
                    //
                    // If no order attribute is specifed then given it an order of 0
                    //
                    orderedProperties.Add(new PropertyOrderPair(pd.Name, 0));
                }
            }
            //
            // Perform the actual order using the value PropertyOrderPair classes
            // implementation of IComparable to sort
            //
            orderedProperties.Sort();
            //
            // Build a string list of the ordered names
            //
            ArrayList propertyNames = new ArrayList();
            foreach (PropertyOrderPair pop in orderedProperties)
            {
                propertyNames.Add(pop.Name);
            }
            //
            // Pass in the ordered list for the PropertyDescriptorCollection to sort by
            //
            return (string[])propertyNames.ToArray(typeof(string));
        }
    }
}
