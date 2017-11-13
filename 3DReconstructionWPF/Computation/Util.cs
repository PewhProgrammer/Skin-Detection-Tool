using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Media3D;
using _3DReconstructionWPF.Data;

namespace _3DReconstructionWPF.Computation
{
    class Util
    {
        public static BBox ComputeBoundingBox(Point3D[] points)
        {
            BBox result = new BBox();
            for(int i = 0; i < points.Length; i++)
            {
                result.Extend(points[i]);
            }

            return result;
        }
    }
}
