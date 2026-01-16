using System.ComponentModel;

using OrderedPropertyGrid;
using KLib.TypeConverters;

namespace LDL.Haptics
{
    [TypeConverter(typeof(SortableTypeConverter))]
    public class HapticSeqVar
    {
        [PropertyOrder(6)]
        [TypeConverter(typeof(HapticSeqVarVariableConverter))]
        public string Variable { get; set; }
        private bool ShouldSerializeVariable() { return false; }

        [PropertyOrder(7)]
        public string Expression { get; set; }
        private bool ShouldSerializeExpression() { return false; }

        public HapticSeqVar()
        {
            Variable = "Delay_ms";
            Expression = "-100:20:100";
        }

        public override string ToString()
        {
            return Variable;
        }
    }
}