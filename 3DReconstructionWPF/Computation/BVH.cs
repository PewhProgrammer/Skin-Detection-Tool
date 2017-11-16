using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _3DReconstructionWPF.Data;
using System.Windows.Media.Media3D;

namespace _3DReconstructionWPF.Computation
{
    class BVH
    {

        private bool _built = false;
        private float depth = 0.0f;

        private Node _root;

        public Intersection Intersect(Ray r, float prevDistance)
        {
            return _root.SearchIntersection(r, prevDistance);
        }

        public void InitIndexing()
        {
            if (_built) return;
            _built = true;
            depth = 8f + 1.3f + (float)Math.Log(_root._objects.Count);
            BuildTree(_root);
        }

        public void AddToScene(Point3D p)
        {
            _root.Add(p);
        }

        public void BuildTree(Node node)
        {
            node._left = new Node();
            node._right = new Node();


        }

        public void SplitPlane(Node node)
        {

        }

        private Tuple<float,float> ComputeSplitInTheMiddle()
        {

            return new Tuple<float,float>(0, 0);
        }

        private Tuple<float,float> ComputeSAH()
        {

            return new Tuple<float, float>(0, 0);
        }
    }
}
