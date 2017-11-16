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
        private BBox _box;


        private bool _leaf;

        public Node()
        {
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


            // Intersect with left and right boundingBox

            // Depending on previous results,
            // Intersect with left or right child

            //according to previousDistance, the right result will either be RHit or LHit

            return new Intersection();
        }

        public void Add(Point3D p)
        {
            _objects.Add(p);
            _box.Extend(p);
        }
    }
}
