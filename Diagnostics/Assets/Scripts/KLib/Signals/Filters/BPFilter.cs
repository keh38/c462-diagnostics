using UnityEngine;

namespace KLib.Signals.Filters
{
    public class BPFilter
    {
        public float CF;
        public float BW;
        public float Fs;
        public int numStages;

        private float a0, a1, a2;
        private float b1, b2;

        private IIRFilter[] _iirFilters;


        public BPFilter(int numStages, float CF, float BW, float Fs)
        {
            this.numStages = numStages;
            this.CF = CF;
            this.BW = BW;
            this.Fs = Fs;

            _iirFilters = new IIRFilter[numStages];
            for (int k = 0; k < numStages; k++) _iirFilters[k] = new IIRFilter();

            ComputeCoefficients();
            DistributeCoefficients();
        }

        public void SetProperties(float CF, float BW)
        {
            this.CF = CF;
            this.BW = BW;

            ComputeCoefficients();
            DistributeCoefficients();
        }

        private void ComputeCoefficients()
        {
            float R = 1 - 3 * BW / Fs;
            float coscf = Mathf.Cos(2 * Mathf.PI * CF / Fs);
            float K = 1 - 2 * R * coscf + R * R;
            K /= (2 - 2 * coscf);

            a0 = 1 - K;
            a1 = 2 * (K - R) * coscf;
            a2 = R * R - K;
            b1 = 2 * R * coscf;
            b2 = -R * R;
        }

        private void DistributeCoefficients()
        {
            foreach (var f in _iirFilters) f.SetCoefficients(a0, a1, a2, b1, b2);
        }

        public float Filter(float x)
        {
            float xf = x;
            foreach (var f in _iirFilters) xf = f.Filter(xf);
            return xf;
        }

    }
}