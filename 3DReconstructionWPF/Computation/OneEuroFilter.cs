using System;
using System.Windows.Media.Media3D;
using pointmatcher.net;
using Microsoft.Kinect;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DReconstructionWPF.Computation
{

    // Implementation: https://mitsufu.wordpress.com/2012/05/09/lissage-oneeurofilter-implmentation-en-c-et-f/
    public class OneEuroFilter
    {


        public OneEuroFilter()
        {
            dummy = true;
            firstTime = true;
            this.minCutoff = 0;
            this.beta = 0;

            xFilt = new LowpassFilter();
            dxFilt = new LowpassFilter();
            dcutoff = 1;
        }

        public OneEuroFilter(double minCutoff, double beta)
        {
            firstTime = true;
            this.minCutoff = minCutoff;
            this.beta = beta;

            xFilt = new LowpassFilter();
            dxFilt = new LowpassFilter();
            dcutoff = 1;
        }

        public OneEuroFilter(double minCutoff, double beta, double hz)
        {
            firstTime = true;
            this.minCutoff = minCutoff;
            this.beta = beta;

            xFilt = new LowpassFilter();
            dxFilt = new LowpassFilter();
            dcutoff = 1;

            rate = hz;
        }

        public OneEuroFilter(double minCutoff, double beta, bool trilinear)
        {
            firstTime = true;
            this.minCutoff = minCutoff;
            this.beta = beta;

            xFilt = new LowpassFilter();
            dxFilt = new LowpassFilter();
            dcutoff = 1;

            if (trilinear)
            {
                yFilter = new OneEuroFilter(minCutoff, beta);
                zFilter = new OneEuroFilter(minCutoff, beta);
            }
        }

        protected bool firstTime;
        protected double minCutoff;
        protected double beta;
        protected LowpassFilter xFilt;
        protected LowpassFilter dxFilt;
        protected double dcutoff;
        protected double rate = 20;
        protected bool dummy = false;

        // this acts as xFilter
        protected OneEuroFilter yFilter;
        protected OneEuroFilter zFilter;

        public double MinCutoff
        {
            get { return minCutoff; }
            set { minCutoff = value; }
        }

        public double Beta
        {
            get { return beta; }
            set { beta = value; }
        }

        public Point3D Filter(Point3D x, double rate)
        {
            return new Point3D(Filter(x.X,rate), yFilter.Filter(x.Y, rate), zFilter.Filter(x.Z, rate));
        }

        public CameraSpacePoint Filter(CameraSpacePoint x)
        {
            if (dummy) return x;
            return Parser3DPoint.FromPoint3DToCameraSpace(Filter(Parser3DPoint.FromCameraSpaceToPoint3D(x), rate));
        }

        public double Filter(double x, double rate)
        {
            double dx = firstTime ? 0 : (x - xFilt.Last()) * rate;
            if (firstTime)
            {
                firstTime = false;
            }

            var edx = dxFilt.Filter(dx, Alpha(rate, dcutoff));
            var cutoff = minCutoff + beta * Math.Abs(edx);

            return xFilt.Filter(x, Alpha(rate, cutoff));
        }

        protected double Alpha(double rate, double cutoff)
        {
            var tau = 1.0 / (2 * Math.PI * cutoff);
            var te = 1.0 / rate;
            return 1.0 / (1.0 + tau / te);
        }
    }

    public class LowpassFilter
    {
        public LowpassFilter()
        {
            firstTime = true;
        }

        protected bool firstTime;
        protected double hatXPrev;

        public double Last()
        {
            return hatXPrev;
        }

        public double Filter(double x, double alpha)
        {
            double hatX = 0;
            if (firstTime)
            {
                firstTime = false;
                hatX = x;
            }
            else
                hatX = alpha * x + (1 - alpha) * hatXPrev;

            hatXPrev = hatX;

            return hatX;
        }
    }
}
