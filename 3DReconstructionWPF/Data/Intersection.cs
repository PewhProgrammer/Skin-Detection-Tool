using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3DReconstructionWPF.Data
{
    class Intersection
    {
        public Ray _ray {get;set;}
        public BBox _box { get; set; }

        public float _distance { get; set; }

        public Intersection() { }

        public Intersection(Ray r, BBox b, float d)
        {
            _ray = r;
            _box = b;
            _distance = d;
        }
    }
}
