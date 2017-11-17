using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows.Media.Media3D;
using _3DReconstructionWPF.Computation;

namespace _3DReconstructionWPF.Data
{

    /// <summary>
    /// Stores Point which are drawn. Computes Bounding Boxes and annotates areas with respective sensor type
    /// </summary>
    class AnnotationHandler
    {
        /// <summary>
        /// Different sensor types
        /// </summary>
        public enum AnnotationType
        {
            Touch,
            Default
        }

        public static HashSet<Point3D> _currentPointSet;
        public static  LinkedList<AnnotationGroup> _annotatedPointsList; 

        public AnnotationHandler()
        {
            _currentPointSet = new HashSet<Point3D>();
            _annotatedPointsList = new LinkedList<AnnotationGroup>();
        }

        /// <summary>
        /// Add all points, which were found during intersection process
        /// </summary>
        /// <param name="p"></param>
        public static void AddIntersectedPoint(Point3D p)
        {
            // Should be the intersection poitn with the box
            _currentPointSet.Add(p);
        }

        /// <summary>
        /// Annotates drawn area. Could be computational inefficient
        /// </summary>
        public static void Annotate(AnnotationType t)
        {
            // Get Bounding Box of _currentPointSet
            BBox box = Util.ComputeBoundingBox(_currentPointSet.ToArray());

            // Determine all Points which fit into the boundings of the box
            HashSet<Point3D> annotated = new HashSet<Point3D>();

            // Add all points to annotationGroup
            _annotatedPointsList.AddLast(new AnnotationGroup(annotated,t));
        }

        /// <summary>
        /// Used to draw all points in the 3DView
        /// </summary>
        /// <returns> AnnotationGroup(includes all points being annotated) </returns>
        public static AnnotationGroup GetLatestAnnotation()
        {
            if (_annotatedPointsList.Count > 0) return _annotatedPointsList.Last();
            return null;
        }


    }
}
