using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Media3D;
using _3DReconstructionWPF.Data;

using Microsoft.Kinect;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _3DReconstructionWPF.Computation
{
    class Util
    {
        public static BBox ComputeBoundingBox(Point3D[] points)
        {
            BBox result = new BBox();
            for(int i = 0; i < points.Length; i++)
            {
                result.Extend(points[i]);
            }

            return result;
        }

        public static void DrawPoint(Canvas canvas, Joint joint)
        {
            // 1) Check whether the joint is tracked.
            if (joint.TrackingState == TrackingState.NotTracked) return;

            // 2) Map the real-world coordinates to screen pixels.
            joint = ScaleTo(joint,canvas.ActualWidth, canvas.ActualHeight);

            // 3) Create a WPF ellipse.
            Ellipse ellipse = new Ellipse
            {
                Width = 5,
                Height = 5,
                Fill = new SolidColorBrush(Colors.LightBlue)
            };

            // 4) Position the ellipse according to the joint's coordinates.
            Canvas.SetLeft(ellipse, joint.Position.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, joint.Position.Y - ellipse.Height / 2);

            // 5) Add the ellipse to the canvas.
            canvas.Children.Add(ellipse);
        }

        public static Point3DCollection RotatePoint3DCollection(Point3DCollection p ,Matrix3D m)
        {


            /*Matrix3D m = new Matrix3D(
                1,0, 0, 0,
                0,1, 0, 0,
                0, 0, 1, 0,
                1, 0, 0, 1); //last column */

            int pcSize = p.Count;
            Point3D[] k = new Point3D[pcSize];
            p.CopyTo(k, 0);
            p.Clear();

            m.Transform(k);

            for (int i = 0; i < pcSize; i++)
            {
                p.Add(k[i]);
            }

            return p;
        }

        public static void DrawLine(Canvas canvas, Point3D p, Point3D q)
        {
            Line line = new Line
            {
                X1 = p.X,
                Y1 = p.Y,
                X2 = q.X,
                Y2 = q.Y,
                StrokeThickness = 5,
                Stroke = new SolidColorBrush(Colors.BlueViolet)
            };

            canvas.Children.Add(line);
        }

        public static Joint ScaleTo(Joint joint,double width, double height)
        {
            return ScaleTo(joint, width, height, 1.0f, 1.0f);
        }

        public static Joint ScaleTo(Joint joint, double width, double height, float skeletonMaxX, float skeletonMaxY)
        {
            joint.Position = new CameraSpacePoint
            {
                X = Scale(width, skeletonMaxX, joint.Position.X),
                Y = Scale(height, skeletonMaxY, -joint.Position.Y),
                Z = joint.Position.Z
            };

            return joint;
        }

        private static float Scale(double maxPixel, double maxSkeleton, float position)
        {
            float value = (float)((((maxPixel / maxSkeleton) / 2) * position) + (maxPixel / 2));

            if (value > maxPixel)
            {
                return (float)maxPixel;
            }

            if (value < 0)
            {
                return 0;
            }

            return value;
        }

    }
}
