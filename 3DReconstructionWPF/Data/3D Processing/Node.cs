using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3DReconstructionWPF.Data
{
    class Node
    {
        public HashSet<Point3D> _objects { get; set; }
        public Node _left { get; set; }
        public Node _right { get; set; }
        public BBox _box;

        public int _id;
        public bool _leaf;

        public Node()
        {
            _objects = new HashSet<Point3D>();
            _box = BBox.Empty();
        }

        /// <summary>
        /// Used to determine lowest level tree traversal
        /// </summary>
        /// <returns></returns>
        public bool IsLeaf()
        {
            return (_objects.Count > 0 || _left == null || _right == null || _leaf);
        }

        public Intersection SearchIntersection(Ray r, float previousDistance)
        {
            // If leaf, then compute primitive intersection or 
            // return intersection with box <-- 

            if (IsLeaf())
            {
                var hit = _box.Intersect(r);
                if (hit.Item1 < hit.Item2)
                {
                    return new Intersection(r, this, hit.Item1);
                }
                else return Intersection.Failure();

                /*
                Intersection mainBox = Intersection.Failure();
                foreach (var primitive in this._objects)
                {
                    Intersection hit = primitive.Intersect(r, previousDistance);
                    if (hit.Hit())
                    {
                        previousDistance = hit._distance;
                        mainBox = hit;
                    }
                }
                return mainBox;
                */
            }


            // Intersect with left and right boundingBox

            var LBoundHit = _left._box.Intersect(r);
            var RBoundHit = _right._box.Intersect(r);
            Intersection LHit = Intersection.Failure(), RHit = Intersection.Failure();

            // Depending on previous results,
            // Intersect with left or right child

            if (LBoundHit.Item1 <= LBoundHit.Item2) LHit = _left.SearchIntersection(r, previousDistance);
            if (LHit.Hit()) previousDistance = LHit._distance;

            if (RBoundHit.Item1 <= RBoundHit.Item2) RHit = _right.SearchIntersection(r, previousDistance);
            if (RHit.Hit()) return RHit;


            //according to previousDistance, the right result will either be RHit or LHit
            return LHit;
        }

        public void Add(Point3D p)
        {
            _objects.Add(p);
            _box.Extend(p);
        }
    }
}
