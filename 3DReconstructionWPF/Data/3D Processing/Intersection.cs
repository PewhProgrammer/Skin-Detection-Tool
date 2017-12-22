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
        public Node _node { get; set; }

        public float _distance { get; set; }

        public Intersection() { }

        public Intersection(Ray r, Node n, float d)
        {
            _ray = r;
            _node = n;
            _distance = d;
        }

        public bool Hit()
        {
            return _distance > 0;
        }

        public static Intersection Failure()
        {
            return new Intersection(null, null, -1);
        }
    }
}
