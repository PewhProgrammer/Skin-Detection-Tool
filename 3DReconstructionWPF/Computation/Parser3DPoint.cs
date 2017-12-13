using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using pointmatcher.net;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;
using System.Numerics;

namespace _3DReconstructionWPF.Computation
{
    class Parser3DPoint
    {
        public static DataPoints FromPoint3DToDataPoints(Point3DCollection points)
        {
            DataPoints result = new DataPoints();
            DataPoint[] resultArray = new DataPoint[points.Count];

            for(int i = 0; i < points.Count; i++)
            {
                Vector3 n = new Vector3(0, 1, 0);
                Point3D p = points[i];
                Vector3 vec = new Vector3((float)p.X, (float)p.Y, (float)p.Z);
                DataPoint dp = new DataPoint
                {
                    point = vec ,
                    normal = n
                };
                resultArray[i] = dp;
            }
            result.points = resultArray;
            result.contiansNormals = true;
   
            return result;
        }

        public static DataPoint FromPoint3DToDataPoint(Point3D points)
        {
                Vector3 n = new Vector3(0, 1, 0);
                Vector3 vec = new Vector3((float)points.X, (float)points.Y, (float)points.Z);
                DataPoint dp = new DataPoint
                {
                    point = vec,
                    normal = n
                };

            return dp;
        }

        public static Point3DCollection FromDataPointsToPoint3DCollection(DataPoints dp)
        {
            Point3DCollection result = new Point3DCollection(dp.points.Length);
            for(int i = 0; i<dp.points.Length; i++)
            {
                Vector3 p = dp.points[i].point;
                result.Add(new Point3D(p.X, p.Y, p.Z));
            }
            return result;
        }

        public static Point3D[] From3DPointCollectionTo3DPointArray(Point3DCollection p)
        {
            Point3D[] result = new Point3D[2];
            
            for(int i = 0; i < p.Count; i++)
            {
                Point3D point = p[i];
                result[i] = p[i];
            }
            return result;
        }

        public static Point3D FromCameraSpaceToPoint3D(CameraSpacePoint csp)
        {
            return new Point3D
            {
                X = csp.X,
                Y = csp.Y,
                Z = csp.Z

            };
        }

        public static Point3DCollection FromCameraSpaceToPoint3DCollection(CameraSpacePoint[] depth2xyz, int size)
        {
            Point3DCollection points = new Point3DCollection();
            float xMax = float.MinValue;
            float yMax = float.MinValue;
            float zMax = float.MinValue;

            float xMin = float.MaxValue;
            float yMin = float.MaxValue;
            float zMin = float.MaxValue;

            for (int i = 0; i < size; i++)
            {
                CameraSpacePoint p = depth2xyz[i];

                //filter out points
                if (p.X > -2 && p.Y > -2 && p.Z > 0 && p.X < 3 && p.Y < 3 && p.Z < 2)
                    points.Add(new Point3D(p.X, p.Y, p.Z));
                else continue;
                if (p.X > xMax) xMax = p.X;
                if (p.Y > yMax) yMax = p.Y;
                if (p.Z > zMax) zMax = p.Z;

                if (p.X < xMin) xMin = p.X;
                if (p.Y < yMin) yMin = p.Y;
                if (p.Z < zMin) zMin = p.Z;
                //Log.writeLog("found point: (" + p.X + ", " + p.Y + ", " + p.Z + ")");
            }

            //max reaches until 5
            Log.writeLog("Max. Points: (" + Math.Round(xMax,2) + ", " + Math.Round(yMax, 2) + ", " + Math.Round(zMax, 2) + ")");
            //min reaches until - inifinity
            Log.writeLog("Min. Points: (" + Math.Round(xMin, 2) + ", " + Math.Round(yMin, 2) + ", " + Math.Round(zMin, 2) + ")");
            return points;
        }

        /// <summary>
        /// populate point cloud around p to weight it more
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Point3DCollection GetPopulatedPointCloud(Point3D p, EuclideanTransform transform)
        {
            Point3DCollection result = new Point3DCollection();
            float countPoints = 24;

            result.Add(p);

            for (int i = 0; i < countPoints; i++)
            {
                double a = ((double)i / 100.0f);

                /*
                result.Add(new Point3D(p.X - a,p.Y,p.Z));
                result.Add(new Point3D(p.X + a, p.Y, p.Z));

                result.Add(new Point3D(p.X, p.Y - a, p.Z));
                result.Add(new Point3D(p.X, p.Y + a, p.Z));

                result.Add(new Point3D(p.X, p.Y, p.Z - a));
                result.Add(new Point3D(p.X, p.Y, p.Z + a));
                */

                result.Add(_transformPoint3D(new Point3D(p.X - a, p.Y - a, p.Z + a), p, transform));
                result.Add(_transformPoint3D(new Point3D(p.X + a, p.Y - a, p.Z + a), p, transform));

                result.Add(_transformPoint3D(new Point3D(p.X - a, p.Y - a, p.Z - a), p, transform));
                result.Add(_transformPoint3D(new Point3D(p.X + a, p.Y - a, p.Z - a), p, transform));

                result.Add(_transformPoint3D(new Point3D(p.X - a, p.Y + a, p.Z + a), p, transform));
                result.Add(_transformPoint3D(new Point3D(p.X + a, p.Y + a, p.Z + a), p, transform));

                result.Add(_transformPoint3D(new Point3D(p.X - a, p.Y + a, p.Z - a), p, transform));
                result.Add(_transformPoint3D(new Point3D(p.X + a, p.Y + a, p.Z - a), p, transform));

            }

            return result;
        }

        public static Point3D _transformPoint3D(Point3D p, Point3D origin, EuclideanTransform transform)
        {
            var vectorRotationFocus = p - origin;
            var transformedPoint = transform.Apply(Parser3DPoint.FromPoint3DToDataPoint(new Point3D(vectorRotationFocus.X, vectorRotationFocus.Y, vectorRotationFocus.Z)).point);
            return new Point3D(transformedPoint.X+ origin.X, transformedPoint.Y + origin.Y, transformedPoint.Z + origin.Z);
        }

        public static Point3DCollection GetPopulatedPointCloud(Point3D p,Point3D q, Point3D a, Point3D b, EuclideanTransform transformation)
        {

            Point3DCollection result = new Point3DCollection();

            // only keep rotation of initaltransformation matrix
            transformation = transformation.Inverse();
            transformation.translation = System.Numerics.Vector3.Zero;
            

            Point3DCollection collection = Parser3DPoint.GetPopulatedPointCloud(
            p ,transformation
            );

            Point3D k = collection[0];

            for (int i = 0; i < collection.Count; i++)
            {
                result.Add(collection[i]);
            }

            collection = Parser3DPoint.GetPopulatedPointCloud(
            q, transformation
            );

            for (int i = 0; i < collection.Count; i++)
            {
                result.Add(collection[i]);
            }

            collection = Parser3DPoint.GetPopulatedPointCloud(
            a, transformation
            );

            for (int i = 0; i < collection.Count; i++)
            {
                result.Add(collection[i]);
            }

            collection = Parser3DPoint.GetPopulatedPointCloud(
            b, transformation
            );

            for (int i = 0; i < collection.Count; i++)
            {
                result.Add(collection[i]);
            }


            return result;
        }
    }
}
