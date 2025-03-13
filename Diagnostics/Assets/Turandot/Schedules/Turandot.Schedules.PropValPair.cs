namespace Turandot.Schedules
{
    public class PropValPair
    {
        public string property = "";
        public float value = 0f;

        public PropValPair()
        {
        }

        public PropValPair(string property, float value)
        {
            this.property = property;
            this.value = value;
        }

        public override string ToString()
        {
            return property + " = " + value.ToString();
        }
    }
}