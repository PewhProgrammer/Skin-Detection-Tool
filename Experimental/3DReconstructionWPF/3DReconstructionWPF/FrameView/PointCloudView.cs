using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Kinect;
using _3DReconstructionWPF.GUI;
using System.Windows.Media.Media3D;

namespace _3DReconstructionWPF.FrameKinectView
{
    class PointCloudView : FrameView
    {

        private MultiSourceFrameReader multiSourceFrameReader;
        private CoordinateMapper cM;
        private Renderer renderer;

        public PointCloudView(Renderer rend)
        {
            this.renderer = rend;
            initPCV();
        }


        public override void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //initPCV();
        }

        private void initPCV()
        {
            this.sensor = KinectSensor.GetDefault();

            if (sensor != null)
            {
                sensor.Open();

                if (sensor.IsOpen)
                {
                    //KinectMessage.Text = "Developing kinect for Windows v2.0 App with Visual Studio 2015 on Windows 10";

                }
            }

            if (sensor != null)
            {
                Log.writeLog("Processing point cloud data from kinect");
                cM = sensor.CoordinateMapper;

                multiSourceFrameReader = sensor.OpenMultiSourceFrameReader(
                    FrameSourceTypes.Depth | FrameSourceTypes.Color);

                //multiSourceFrameReader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;


                sensor.Open();

                if (sensor.IsOpen)
                {
                    //KinectMessage.Text = "Developing kinect for Windows v2.0 App with Visual Studio 2015 on Windows 10";

                }
            }
            else Log.writeLog("Failed to retrieve kinect sensor!");
        }

        public override void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sensor != null && sensor.IsOpen)
            {
                sensor.Close();
            }
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            // InfraredFrame is IDisposable
            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();
            getDepthData(multiSourceFrame);

            //could get color data as well
        }

        public Point3DCollection getDepthDataFromLatestFrame()
        {
            int i = 0;
            MultiSourceFrame frame = null;
            while(frame == null || i < 1000)
            {
                frame = multiSourceFrameReader.AcquireLatestFrame();
                i++;
            }

            if(frame != null)
            return getDepthData(frame);

            return null;
        }

            Point3DCollection getDepthData(MultiSourceFrame frame)
        {
            Log.writeLog("captured Frame");
            

            DepthFrameReference depthFrameReference = frame.DepthFrameReference;
            DepthFrame depthFrame = depthFrameReference.AcquireFrame();

            if (depthFrame == null) return null;

            int height = depthFrame.FrameDescription.Height;
            int width = depthFrame.FrameDescription.Width;

            CameraSpacePoint[] depth2xyz = new CameraSpacePoint[height * width];


            ushort[] depthFrameData = new ushort[height*width];
            depthFrame.CopyFrameDataToArray(depthFrameData);

            //depthFrame.Dispose(); // dont know if it works

            // Process depth frame data...
            cM.MapDepthFrameToCameraSpace(depthFrameData,depth2xyz);
            float xMax = float.MinValue;
            float yMax = float.MinValue;
            float zMax = float.MinValue;

            float xMin = float.MaxValue;
            float yMin = float.MaxValue;
            float zMin = float.MaxValue;


            Point3DCollection points = new Point3DCollection();

            for (int i = 0; i < width*height; i++)
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
