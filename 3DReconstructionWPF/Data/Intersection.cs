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
        private Ray _ray {get;set;}
        private BBox _box { get; set; }

        private float _distance { get; set; }

        public Intersection() { }

        public Intersection(Ray r, BBox b, float d)
        {
            _ray = r;
            _box = b;
            _distance = d;
        }
    }
}
