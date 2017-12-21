using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Kinect;
using _3DReconstructionWPF.GUI;
using System.Windows.Media.Media3D;
using System.Windows.Media;

using _3DReconstructionWPF.Computation;
using _3DReconstructionWPF.Data;

namespace _3DReconstructionWPF.FrameKinectView
{
    class PointCloudView : FrameView
    {

        private MultiSourceFrameReader multiSourceFrameReader;
        private CoordinateMapper cM;
        private Renderer renderer;
        public BVH _bvh { get; set; }
        public ProcessingStage _processingStage;

        public PointCloudView(Renderer rend)
        {
            this.renderer = rend;
            InitPCV();
        }


        public override void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //initPCV();
        }

        private void InitPCV()
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
                    FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.Body | FrameSourceTypes.BodyIndex);

                //multiSourceFrameReader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;


                sensor.Open();
                
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
            var multiSourceFrame = e.FrameReference.AcquireFrame();
            GetDepthData(multiSourceFrame);

            //could get color data as well
        }

        public Tuple<Point3DCollection,Point3DCollection> GetDepthDataFromLatestFrame()
        {
            int i = 0;
            MultiSourceFrame frame = null;
            while(frame == null || i < 1000)
            {
                frame = multiSourceFrameReader.AcquireLatestFrame();
                i++;
            }

            if(frame != null)  return GetDepthData(frame);

            return null;
        }

        Tuple<Point3DCollection, Point3DCollection> GetDepthData(MultiSourceFrame frame)
        {

            byte[] array = null;

            // filter background with bodyIndex
            using (BodyIndexFrame bodyIndexFrame = frame.BodyIndexFrameReference.AcquireFrame())
            {
                if (bodyIndexFrame != null)
                {
                    var description = bodyIndexFrame.FrameDescription;
                    array = new byte[description.Width * description.Height];
                    bodyIndexFrame.CopyFrameDataToArray(array);
                }
            }

            var depthFrameReference = frame.DepthFrameReference;
            var depthFrame = depthFrameReference.AcquireFrame();

            if (depthFrame == null) return null;

            int height = depthFrame.FrameDescription.Height;
            int width = depthFrame.FrameDescription.Width;

            CameraSpacePoint[] depth2xyz = new CameraSpacePoint[height * width];
            CameraSpacePoint shoulderPos = new CameraSpacePoint();
            BBox box = new BBox();
            Point3DCollection result = new Point3DCollection();

            using (var bodyFrame = frame.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var bodies = new Body[bodyFrame.BodyFrameSource.BodyCount];

                    bodyFrame.GetAndRefreshBodyData(bodies);

                    foreach (var body in bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {

                                // Find the joints

                                Joint thumbRight = body.Joints[JointType.ThumbRight];
                                Joint thumbLeft = body.Joints[JointType.ThumbLeft];

                                // Tip
                                Joint tipRight = body.Joints[JointType.HandTipRight];
                                Joint tipLeft = body.Joints[JointType.HandTipLeft];

                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint handLeft = body.Joints[JointType.HandLeft];

                                // elbow
                                Joint elbowLeft = body.Joints[JointType.ElbowLeft];

                                // shoulder
                                shoulderPos = body.Joints[JointType.ShoulderLeft].Position;


                                tipLeft.Position = FilterGroup.GetFilter(FilterGroup.Description.Fingertip).Filter(tipLeft.Position);
                                handLeft.Position = FilterGroup.GetFilter(FilterGroup.Description.Hand).Filter(handLeft.Position);
                                thumbLeft.Position = FilterGroup.GetFilter(FilterGroup.Description.ThumbTip).Filter(thumbLeft.Position);
                                elbowLeft.Position = FilterGroup.GetFilter(FilterGroup.Description.Elbow).Filter(elbowLeft.Position);

                                result = new Point3DCollection
                                {
                                    Parser3DPoint.FromCameraSpaceToPoint3D(handLeft.Position),
                                    Parser3DPoint.FromCameraSpaceToPoint3D(tipLeft.Position),
                                    Parser3DPoint.FromCameraSpaceToPoint3D(thumbLeft.Position),
                                    Parser3DPoint.FromCameraSpaceToPoint3D(elbowLeft.Position)
                                };


                                //box.Extend(Parser3DPoint.FromCameraSpaceToPoint3D(shoulderPos));
                                box.Extend(Parser3DPoint.FromCameraSpaceToPoint3D(elbowLeft.Position));
                                box.Extend(Parser3DPoint.FromCameraSpaceToPoint3D(handLeft.Position));
                                box.Extend(Parser3DPoint.FromCameraSpaceToPoint3D(tipLeft.Position));
                                box.Extend(Parser3DPoint.FromCameraSpaceToPoint3D(thumbLeft.Position));

                                var vector = new Vector3D
                                {
                                    X = tipRight.Position.X - handRight.Position.X,
                                    Y = tipRight.Position.Y - handRight.Position.Y,
                                    Z = tipRight.Position.Z - handRight.Position.Z
                                };

                                vector.Normalize();
                                var tipR = tipRight.Position;
                            }
                        }
                    }
                }
            }

            ushort[] depthFrameData = new ushort[height * width];

            depthFrame.CopyFrameDataToArray(depthFrameData);
            var radius = 0.08f;

            // Process depth frame data...
            cM.MapDepthFrameToCameraSpace(depthFrameData, depth2xyz);

            for (int i = 0; i < depthFrameData.Length; i++)
            {
                //filter everything around the box
                if (array[i] == 255 || 
                    (box.GetMaxPoint().Z + radius) < depth2xyz[i].Z ||
                     (box.GetMaxPoint().Y + radius) < depth2xyz[i].Y ||
                      (box.GetMaxPoint().X + radius) < depth2xyz[i].X ||
                      (box.GetMinPoint().Z - radius) > depth2xyz[i].Z ||
                     (box.GetMinPoint().Y - radius) > depth2xyz[i].Y ||
                      (box.GetMinPoint().X - radius) > depth2xyz[i].X
                      )
                {
                    depth2xyz[i].Z = -10000;
                }
            }

            /*
            var boxCloud = new Point3DCollection
                {
                    box.GetMinPoint(),
                    box.GetMaxPoint()
                };

            renderer.CreatePointCloud(boxCloud, Brushes.DeepPink,false, 0.0235f);
            */
            return new Tuple<Point3DCollection, Point3DCollection>(Parser3DPoint.FromCameraSpaceToPoint3DCollection(depth2xyz, height * width),result);
        }

        public override void SetProcessingStage(ProcessingStage processingStage)
        {
            _processingStage = processingStage;
        }
    }
}
