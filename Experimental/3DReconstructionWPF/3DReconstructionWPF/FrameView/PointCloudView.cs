using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Kinect;

namespace _3DReconstructionWPF.FrameKinectView
{
    class PointCloudView : FrameView
    {

        private MultiSourceFrameReader multiSourceFrameReader;
        private CoordinateMapper cM;


        public override void MainPage_Loaded(object sender, RoutedEventArgs e)
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

                sensor.Open();
                multiSourceFrameReader =  sensor.OpenMultiSourceFrameReader(
                    FrameSourceTypes.Depth | FrameSourceTypes.Color);

                multiSourceFrameReader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
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

            void getDepthData(MultiSourceFrame frame)
        {
            DepthFrameReference depthFrameReference = frame.DepthFrameReference;
            DepthFrame depthFrame = depthFrameReference.AcquireFrame();

            int height = depthFrame.FrameDescription.Height;
            int width = depthFrame.FrameDescription.Width;

            CameraSpacePoint[] depth2xyz = new CameraSpacePoint[height * width];





            ushort[] depthFrameData = null;
            depthFrame.CopyFrameDataToArray(depthFrameData);

            //depthFrame.Dispose(); // dont know if it works

            // Process depth frame data...
            cM.MapDepthFrameToCameraSpace(depthFrameData,depth2xyz);

            for(int i = 0; i < width*height; i++)
            {

            }
        }
    }
}
