

namespace LDL.Haptics
{
    public class HapticSeqVar
    {
        public string Variable { get; set; }
        private bool ShouldSerializeVariable() { return false; }

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