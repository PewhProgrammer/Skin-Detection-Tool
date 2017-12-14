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
        private int _nodeCount;

        private Node _root;

        public BVH()
        {
            _root = new Node();
        }

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

        public void AddAllToScene(Point3DCollection p)
        {
            for(int i= 0, len = p.Count; i < len; i++)
            {
                AddToScene(p[i]);
            }
        }

        public void BuildTree(Node node)
        {
            node._left = new Node();
            node._right = new Node();

            //init node with ids
            node._left = new Node();
            node._right = new Node();

            node._left._id = _nodeCount;
            this._nodeCount++;
            node._right._id = _nodeCount;
            this._nodeCount++;

            //use heuristic to split
            SplitPlane(node);

            //decide whether to build more 
            float sizeOrigin = node._objects.Count;
            node._objects.Clear();

            if (node._left._objects.Count > 10 && node._left._objects.Count != sizeOrigin)
                BuildTree(node._left);
            else
                node._left._leaf = true;


            if (node._right._objects.Count > 10 && node._right._objects.Count != sizeOrigin)
                BuildTree(node._right);
            else
                node._right._leaf = true;
        }

        public void SplitPlane(Node node)
        {
            var split = ComputeSplitInTheMiddle(node);
            //var split = ComputeSAH(node);

            if(split.Item1 == 0)
            {
                for (var i = 0; i < node._objects.Count; ++i)
                {
                    var iter = node._objects.ElementAt(i);

                    //median of prim bounds
                    if (iter.X <= split.Item2) node._left.Add(iter);
                    else node._right.Add(iter);
                }
            }

            if (split.Item1 == 1)
            {
                for (var i = 0; i < node._objects.Count; ++i)
                {
                    var iter = node._objects.ElementAt(i);

                    //median of prim bounds
                    if (iter.Y <= split.Item2) node._left.Add(iter);
                    else node._right.Add(iter);
                }
            }

            if (split.Item1 == 2)
            {
                for (var i = 0; i < node._objects.Count; ++i)
                {
                    var iter = node._objects.ElementAt(i);

                    //median of prim bounds
                    if (iter.Z <= split.Item2) node._left.Add(iter);
                    else node._right.Add(iter);
                }
            }
        }

        private Tuple<float,float> ComputeSplitInTheMiddle(Node node)
        {
            Point3D boxMax = node._box.GetMaxPoint();
            Point3D boxMin = node._box.GetMinPoint();

            double x = boxMax.X - boxMin.X;
            double y = boxMax.Y - boxMin.Y;
            double z = boxMax.Z - boxMin.Z;


            if (x > y & x > z)
            {
                return new Tuple<float, float>(0, (float)(boxMax.X + boxMin.X) * 0.5f);
            }
            else if (y > z)
            {
                return new Tuple<float, float>(1, (float)(boxMax.Y + boxMin.Y) * 0.5f);
            }
            else
            {
                return new Tuple<float, float>(2, (float)(boxMax.Z + boxMin.Z) * 0.5f);
            }

        }

        private Tuple<float,float> ComputeSAH(Node node)
        {

            return new Tuple<float, float>(0, 0);
        }

        public static BVH InitBVH(Point3DCollection points)
        {
            BVH _structure = new BVH();
            for (int i = 0; i < points.Count; i++)
            {
                _structure.AddToScene(points[i]);
            }
            _structure.InitIndexing();
            return _structure;
        }
    }
}
