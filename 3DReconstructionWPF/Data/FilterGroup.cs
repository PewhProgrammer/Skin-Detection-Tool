using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using _3DReconstructionWPF.Computation;

namespace _3DReconstructionWPF.Data
{
    class FilterGroup
    {

        //represents every filter for feature points

        public enum Description
        {
            FingertipRight,
            HandRight,
            Fingertip,
            ThumbTip,
            Hand,
            Elbow
        }


        public static bool Enabled = true;
        private static OneEuroFilter _dummyFilter = new OneEuroFilter();
        private static Dictionary<Description, OneEuroFilter> _filters = new Dictionary<Description, OneEuroFilter>();

        public static void InitFilters()
        {
            AddFilter(Description.FingertipRight, new OneEuroFilter(1, 0, true));
            AddFilter(Description.HandRight, new OneEuroFilter(1, 0, true));
            AddFilter(Description.Fingertip, new OneEuroFilter(1, 0, true));
            AddFilter(Description.ThumbTip, new OneEuroFilter(1, 0, true));
            AddFilter(Description.Hand, new OneEuroFilter(1, 0, true));
            AddFilter(Description.Elbow, new OneEuroFilter(1, 0, true));
        }

        public static void AddFilter(Description d,OneEuroFilter e)
        {
            _filters.Add(d, e);
        }

        public static void ChangeMinimumCutoff(float cutoff)
        {
            Parallel.ForEach(_filters, (pair) =>
            
                 pair.Value.MinCutoff = cutoff
            );
        }

        public static void ChangeCutoffSlope(float slope)
        {
            Parallel.ForEach(_filters, (pair) =>
                 pair.Value.Beta = slope
            );
        }

        public static OneEuroFilter GetFilter(Description d)
        {
            if(Enabled)
                return _filters[d];

            return _dummyFilter;
        }
    }
}
