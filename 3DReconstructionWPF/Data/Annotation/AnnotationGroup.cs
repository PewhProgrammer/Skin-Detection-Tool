using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Media3D;
using System.Windows.Media;
using _3DReconstructionWPF.Data;

namespace _3DReconstructionWPF.Data
{
    /// <summary>
    /// Modifieable 3D points. Mainly Stores 3d point and allows modification/annotations on it
    /// </summary>
    class AnnotationGroup
    {

        private AnnotationHandler.AnnotationType _type;
        private HashSet<Point3D> _points;

        public AnnotationGroup(HashSet<Point3D> p,AnnotationHandler.AnnotationType t)
        {
            _points = p;
            _type = t;
        }

    }
}
