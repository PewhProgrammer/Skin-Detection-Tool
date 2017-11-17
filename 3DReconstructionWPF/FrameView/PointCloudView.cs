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
using _3DReconstructionWPF.Data;

namespace _3DReconstructionWPF.FrameKinectView
{
    class PointCloudView : FrameView
    {

        private MultiSourceFrameReader multiSourceFrameReader;
        private CoordinateMapper cM;
        private Renderer renderer;
        public BVH _bvh { get; set; }

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
            {
                return getDepthData(frame);
            }

            return null;
        }

            Point3DCollection getDepthData(MultiSourceFrame frame)
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

                    float k = 2;
                }
            }

            var depthFrameReference = frame.DepthFrameReference;
            var depthFrame = depthFrameReference.AcquireFrame();

            if (depthFrame == null) return null;

            int height = depthFrame.FrameDescription.Height;
            int width = depthFrame.FrameDescription.Width;

            CameraSpacePoint[] depth2xyz = new CameraSpacePoint[height * width];
            CameraSpacePoint elbowLeftPosition = new CameraSpacePoint();

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

                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint thumbRight = body.Joints[JointType.ThumbRight];

                                // Tip
                                Joint tipRight = body.Joints[JointType.HandTipRight];
                                Joint tipLeft = body.Joints[JointType.HandTipLeft];


                                Joint handLeft = body.Joints[JointType.HandLeft];
                                Joint thumbLeft = body.Joints[JointType.ThumbLeft];

                                // elbow
                                Joint elbowLeft = body.Joints[JointType.ElbowLeft];

                                var vector = new Vector3D
                                {
                                    X = tipRight.Position.X - handRight.Position.X,
                                    Y = tipRight.Position.Y - handRight.Position.Y,
                                    Z = tipRight.Position.Z - handRight.Position.Z
                                };

                                vector.Normalize();
                                var tipR = tipRight.Position;
                                Ray ray = new Ray(new Point3D(tipR.X, tipR.Y, tipR.Z), vector);

                                if (_bvh != null)
                                {
                                    Intersection inter = _bvh.Intersect(ray, float.MaxValue);
                                    if(inter._distance > 0) // Found Intersection
                                    {
                                        AnnotationHandler.AddIntersectedPoint(inter._ray.GetPoint(inter._distance));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            ushort[] depthFrameData = new ushort[height * width];

            depthFrame.CopyFrameDataToArray(depthFrameData);

            // Process depth frame data...
            cM.MapDepthFrameToCameraSpace(depthFrameData, depth2xyz);

            for (int i = 0; i < depthFrameData.Length; i++)
            {
                if (array[i] == 255 || depth2xyz[i].Z > elbowLeftPosition.Z)
                {
                    depth2xyz[i].Z = -10000;
                }
            }




            return Parser3DPoint.FromCameraSpaceToPoint3DCollection(depth2xyz, height * width);
        }

        Point3DCollection GetDepthDataArm(MultiSourceFrame frame)
        {
            using(var bodyFrame = frame.BodyFrameReference.AcquireFrame())
            {
                if(bodyFrame != null)
                {
                    var bodies = new Body[bodyFrame.BodyFrameSource.BodyCount];
                    bodyFrame.GetAndRefreshBodyData(bodies);

                    foreach( var body in bodies)
                    {
                        if(body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Find the joints
                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint thumbRight = body.Joints[JointType.ThumbRight];

                                Joint handLeft = body.Joints[JointType.HandLeft];
                                Joint thumbLeft = body.Joints[JointType.ThumbLeft];
                            }
                        }
                    }
                }
            }


            return null;
        }
    }
}
