using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DReconstructionWPF.Computation
{
    public class OneEuroFilter
    {
        private bool _firstTimeFlag = false;
        private double _updateRate = 20;

        private double _dcutoff = 20 ;
        private double _mincutoff = 5;
        private double _cutoffSlope = 20;

        private LowPassFilter _lowPassFilter;

        public OneEuroFilter()
        {
            _lowPassFilter = new LowPassFilter();
        }


        public double ComputeFilteredValue(double x)
        {
            var dx = 0d;
            if (_firstTimeFlag)
            {
                _firstTimeFlag = false;
                dx = 0;
            }
            else dx = (x - _lowPassFilter.GetHatXPrev()) * _updateRate;

            var edx = _lowPassFilter.Filter(dx, Alpha(_updateRate, _dcutoff));
            var cutoff = _mincutoff + _cutoffSlope * Math.Abs(edx);
            return _lowPassFilter.Filter(x, Alpha(_updateRate, cutoff));
        }

        private double Alpha(double rate, double cutoff)
        {
            var tau = 1.0f / (2 * Math.PI * cutoff);
            var te = 1.0f / rate;
            return 1.0f / (1.0f + tau / te);
        }

        private class LowPassFilter
        {
            private bool _firstTimeFlag = false;
            private double _hatXPrev = 0;

            public double Filter(double x, double alpha)
            {
                if (_firstTimeFlag)
                { _firstTimeFlag = false; _hatXPrev = x; }
                var hatx = alpha * x + (1 - alpha) * _hatXPrev;
                _hatXPrev = hatx;
                return hatx;
            }

            public double GetHatXPrev()
            {
                return _hatXPrev;
            }


        }

        
    }
}
