namespace Turandot
{
    public class XboxLink
    {
        public enum ControlSource { None, A, B, X, Y};

        public ControlSource control = ControlSource.None;
        public bool display = false;
        public int x;
        public int y;

        public XboxLink() { }
    }
}