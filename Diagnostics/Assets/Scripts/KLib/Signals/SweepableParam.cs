
namespace KLib.Signals
{
    public class SweepableParam
    {
        public string name;
        public string units;
        public float defaultValue;

        public SweepableParam(string name, string units, float defaultValue)
        {
            this.name = name;
            this.units = units;
            this.defaultValue = defaultValue;
        }
    }
}
