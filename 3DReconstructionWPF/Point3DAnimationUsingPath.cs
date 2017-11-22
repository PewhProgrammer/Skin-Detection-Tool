using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace _3DReconstructionWPF
{
    public class Point3DAnimationUsingPath : AnimationTimeline
    {
        #region AnimationTimeline abstract overrides

        public override Type TargetPropertyType
        {
            
            get { return typeof(Point3D); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new Point3DAnimationUsingPath { Z = this.Z };
        }

        #endregion

        #region AnimationTimeline virtual overrides

        public override object GetCurrentValue(
            object defaultOriginValue, object defaultDestinationValue,
            AnimationClock animationClock)
        {
    
            PathGeometry path = this.PathGeometry;

            Point point;
            Point tangent;

            path.GetPointAtFractionLength(
                animationClock.CurrentProgress.Value, out point, out tangent);

            return new Point3D(point.X, point.Y, Z);
        }

        #endregion

        public double Z { get; set; }

        public PathGeometry PathGeometry
        {
            get { return (PathGeometry)GetValue(PathGeometryProperty); }
            set { SetValue(PathGeometryProperty, value); }
        }

        public static readonly DependencyProperty PathGeometryProperty =
            DependencyProperty.Register(
                "PathGeometry",
                typeof(PathGeometry),
                typeof(Point3DAnimationUsingPath));
    }
}