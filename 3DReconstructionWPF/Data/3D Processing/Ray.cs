using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3DReconstructionWPF.Data
{
    class Ray
    {
        public Point3D _point { get; set; }
        public Vector3D _vector { get; set; }

        public Ray(Point3D p,Vector3D v)
        {
            _point = p;

            v.Normalize();
            _vector = v;
        }

        public Point3D GetPoint(float distance)
        {
            
            return _point + (_vector * distance);
        }


    }
}
