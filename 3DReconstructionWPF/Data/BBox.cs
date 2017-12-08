using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Media3D;
using _3DReconstructionWPF.Computation;

namespace _3DReconstructionWPF.Data
{
    class BBox
    {

        private Point3D _maxPoint;
        private Point3D _minPoint;

        public BBox()
        {
            _maxPoint = new Point3D()
            {
                X = float.MinValue,
                Y = float.MinValue,
                Z = float.MinValue
            };

            _minPoint = new Point3D()
            {
                X = float.MaxValue,
                Y = float.MaxValue,
                Z = float.MaxValue
            };
        }

        public BBox(Point3D max, Point3D min)
        {
            _maxPoint = max;
            _minPoint = min;
        }

        public void Extend(Point3D p)
        {
            double x = p.X;
            double y = p.Y;
            double z = p.Z;

            if (x > _maxPoint.X) _maxPoint.X = x;
            if (x < _minPoint.X) _minPoint.X = x;

            if (y > _maxPoint.Y) _maxPoint.Y = y;
            if (y < _minPoint.Y) _minPoint.Y = y;

            if (z > _maxPoint.Z) _maxPoint.Z = z;
            if (z < _minPoint.Z) _minPoint.Z = z;
        }

        public Tuple<float,float> Intersect(Ray ray)
        {
            // slab technique implementation

            //if unbound return intersection immediately?

            float invRayDir, near, far;
            float t_0 = -float.MaxValue; // needs to be overwritten later 
            float t_1 = float.MaxValue;

            

                invRayDir =(float)( 1f / ray._vector.X); // more efficient
                near = (float)(_minPoint.X - ray._point.X) * invRayDir;
                far = (float)(_maxPoint.X - ray._point.X) * invRayDir;
                if (near > far) Util.Swap(ref near, ref far); // assign near and far

                //biggest near and smallest far for slabs
                t_0 = near > t_0 ? near : t_0;
                t_1 = far < t_1 ? far : t_1;

            //////////////// Y


            invRayDir = (float)(1f / ray._vector.Y); // more efficient
            near = (float)(_minPoint.Y - ray._point.Y) * invRayDir;
            far = (float)(_maxPoint.Y - ray._point.Y) * invRayDir;
            if (near > far) Util.Swap(ref near, ref far); // assign near and far

            //biggest near and smallest far for slabs
            t_0 = near > t_0 ? near : t_0;
            t_1 = far < t_1 ? far : t_1;

            //////// Z

            invRayDir = (float)(1f / ray._vector.Z); // more efficient
            near = (float)(_minPoint.Z - ray._point.Z) * invRayDir;
            far = (float)(_maxPoint.Z - ray._point.Z) * invRayDir;
            if (near > far)
                Util.Swap(ref near, ref far); // assign near and far

            //biggest near and smallest far for slabs
            t_0 = near > t_0 ? near : t_0;
            t_1 = far < t_1 ? far : t_1;

            //if biggest near is smaller than smallest t1, we have intersection
            //if not then t0 > t1 and we throw failure
            return new Tuple<float, float>(t_0, t_1);
        }

        public Point3D GetMaxPoint()
        {
            return _maxPoint;
        }

        public Point3D GetMinPoint()
        {
            return _minPoint;
        }

        public static BBox Empty()
        {
            return new BBox(
                new Point3D(float.MinValue, float.MinValue, float.MinValue),
                new Point3D(float.MaxValue, float.MaxValue, float.MaxValue)
                );
        }
    }
}
