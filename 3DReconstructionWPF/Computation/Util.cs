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
using pointmatcher.net;

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
            DrawLine(canvas, p, q, Colors.Azure,5);
        }

        public static void DrawLine(Canvas canvas, Point3D p, Point3D q, Color c,int stroke)
        {
            if (c == null)
            {
                c = Colors.Azure;
            }

            Line line = new Line
            {
                X1 = p.X,
                Y1 = p.Y,
                X2 = q.X,
                Y2 = q.Y,
                StrokeThickness = stroke,
                Stroke = new SolidColorBrush(c)
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

        // derived from: 
        // https://math.stackexchange.com/questions/180418/calculate-rotation-matrix-to-align-vector-a-to-vector-b-in-3d //
        public static EuclideanTransform ComputeInitialTransformation(Point3D q1, Point3D p1)
        {
            // p2 and q2 are anchors and we map p1 to q1 vector

            var p2 = new Point3D(0, 0, 0);
            var q2 = new Point3D(0, 0, 0);

            var vectorP = p1 - p2;
            var vectorQ = q1 - q2;
            var vectorPQ = q2 - p2; // Translate anchor points according to map
            var vectorPQNum = new System.Numerics.Vector3((float)vectorPQ.X, (float)vectorPQ.Y, (float)vectorPQ.Z);

            vectorP.Normalize();
            vectorQ.Normalize();

            var cross = Vector3D.CrossProduct(vectorQ, vectorP);
            var s = cross.Length;
            var c = Vector3D.DotProduct(vectorP, vectorQ);

            // compute skew-symmetric cross-product matrix of v
            var skewSymmetricMatrix = new System.Numerics.Matrix4x4(
                0,(float)-cross.Z, (float)cross.Y,0,
                (float)cross.Z,0, (float)-cross.X,0,
                (float)-cross.Y, (float)cross.X,0,0,
                0,0,0,1);

            var squaredSkewMatrix = System.Numerics.Matrix4x4.Multiply(skewSymmetricMatrix, skewSymmetricMatrix);
            var lastFraction = (1 - c) / (1- (c * c));

            var R = System.Numerics.Matrix4x4.Identity + skewSymmetricMatrix + (squaredSkewMatrix * (float)lastFraction);


            var m = new EuclideanTransform
            {
                translation = new System.Numerics.Vector3(0,0,0),
                rotation = System.Numerics.Quaternion.CreateFromRotationMatrix(R),
            };

            //only rotate p1 and translate both p values
            var rotateP = ConvertPoint3DToVector3(p1);
            var rotatedP1 = m.Apply(rotateP);

            var vectorTranslation = (ConvertPoint3DToVector3(q1) - rotatedP1);

            m.translation = vectorTranslation;

             // Check that p1 is going to be transformed to q1
            var pNum = new System.Numerics.Vector3((float)p1.X, (float)p1.Y, (float)p1.Z);
            var result = m.Apply(pNum);
            

            return m;
        }

        private static System.Numerics.Vector3 ConvertPoint3DToVector3(Point3D p)
        {
            return new System.Numerics.Vector3((float)p.X, (float)p.Y, (float)p.Z);
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}
