namespace Turandot
{
    public class Flag
    {
        public string name;
        public int value;

        public Flag() { }
        public Flag(string name)
        {
            this.name = name;
        }

        override public string ToString()
        {
            return name + " = " + value;
        }
    }
}