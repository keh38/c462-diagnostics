namespace KLib.Signals.Filters
{
    public class IIRFilter
    {
        private float a0, a1, a2;
        private float b1, b2;
        private float x1, x2;
        private float y1, y2;

        public IIRFilter()
        {
            Initialize();
        }

        public IIRFilter(float a0, float a1, float a2, float b1, float b2)
        {
            SetCoefficients(a0, a1, a2, b1, b2);
            Initialize();
        }

        public void SetCoefficients(float a0, float a1, float a2, float b1, float b2)
        {
            this.a0 = a0;
            this.a1 = a1;
            this.a2 = a2;
            this.b1 = b1;
            this.b2 = b2;
        }

        public void Initialize()
        {
            x1 = x2 = y1 = y2 = 0;
        }

        public float Filter(float x)
        {
            float xf = a0 * x + a1 * x1 + a2 * x2 + b1 * y1 + b2 * y2;
            x2 = x1;
            x1 = x;
            y2 = y1;
            y1 = xf;

            return xf;
        }

    }
}