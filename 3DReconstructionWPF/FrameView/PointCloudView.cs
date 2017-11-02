using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Kinect;
using _3DReconstructionWPF.GUI;
using System.Windows.Media.Media3D;

using _3DReconstructionWPF.Computation;

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

            //depthFrame.Dispose(); //

            // Process depth frame data...
            cM.MapDepthFrameToCameraSpace(depthFrameData,depth2xyz);

            return Parser3DPoint.FromCameraSpaceToPoint3DCollection(depth2xyz, height * width);
        }
    }
}
