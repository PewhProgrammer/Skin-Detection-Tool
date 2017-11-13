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
            Log.writeLog("Max Pointcloud Points: (" + xMax + ", " + yMax + ", " + zMax + ")");
            //min reaches until - inifinity
            Log.writeLog("Min Pointcloud Points: (" + xMin + ", " + yMin + ", " + zMin + ")");
            return points;
        }
    }
}
