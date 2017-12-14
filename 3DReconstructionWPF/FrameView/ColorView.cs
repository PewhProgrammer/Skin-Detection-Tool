using System;
using System.Windows;

using Microsoft.Kinect;
using System.Windows.Controls;

using System.Windows.Media.Imaging;
using System.Windows.Media;
using _3DReconstructionWPF.Computation;
using _3DReconstructionWPF.Data;
using System.Windows.Media.Media3D;
using _3DReconstructionWPF.GUI;

namespace _3DReconstructionWPF.FrameKinectView
{
    class ColorView : FrameView
    {
        // Size of the RGB pixel in the bitmap
        private const int BytesPerPixel = 4;

        private MultiSourceFrameReader _multiSourceFrameReader = null;
        public BVH _bvh;
        private ProcessingStage _processingStage;

        // canvas on top of rgb color stream
        private Canvas _canvas = ((MainWindow)Application.Current.MainWindow).canvas;
        private Renderer _renderer;

        Image frameDisplayImage;
        public ColorView(Image FDI,Renderer renderer)
        {
            this.frameDisplayImage = FDI;
            _renderer = renderer;
        }

        public override void SetProcessingStage(ProcessingStage processingStage)
        {
            _processingStage = processingStage;
        }

        public override void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.GetDefault();

            FrameDescription coloredFrameDescription =
                this.sensor.ColorFrameSource.FrameDescription;



            this._multiSourceFrameReader = this.sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body | FrameSourceTypes.BodyIndex);

            this._multiSourceFrameReader.MultiSourceFrameArrived +=
                this.Reader_ColorFrameArrived;

            if (sensor != null)
            {
                sensor.Open();

                if (sensor.IsOpen)
                {
                    
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

        private void Reader_ColorFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();
            // Get color data
           using (ColorFrame coloredFrame =
                reference.ColorFrameReference.AcquireFrame())
            {
                if (coloredFrame != null)
                {
                   frameDisplayImage.Source = ToBitmap(coloredFrame);
                    _processingStage.CompleteProcessingStage(ProcessingStage.Description.KinectStream);
                }
            }

            // filter background with bodyIndex
            using (BodyIndexFrame bodyIndexFrame = reference.BodyIndexFrameReference.AcquireFrame())
            {
                if(bodyIndexFrame != null)
                {
                    var description = bodyIndexFrame.FrameDescription;
                    byte[] array = new byte[description.Width*description.Height];

                    bodyIndexFrame.CopyFrameDataToArray(array);
                }
            }

            // display hand tracking

            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var bodies = new Body[frame.BodyFrameSource.BodyCount];
                    
                    frame.GetAndRefreshBodyData(bodies);

                    foreach (var body in bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                _processingStage.CompleteProcessingStage(ProcessingStage.Description.FeatureDetection);
                                Joint head = body.Joints[JointType.Head];

                                float x = head.Position.X;
                                float y = head.Position.Y;
                                float z = head.Position.Z;


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


                                // canvas ray
                                var vector = new Vector3D
                                {
                                    X = tipRight.Position.X - handRight.Position.X,
                                    Y = tipRight.Position.Y - handRight.Position.Y,
                                    Z = tipRight.Position.Z - handRight.Position.Z
                                };


                                var tipR = tipRight.Position;

                                if (_bvh != null)
                                {
                                    Ray ray = new Ray(new Point3D(tipR.X, tipR.Y, tipR.Z), vector);

                                    // for sphere testing, set origin at zero
                                    // Ray ray = new Ray(new Point3D(0,0,0), vector);

                                    Intersection inter = _bvh.Intersect(ray, float.MaxValue);
                                    if (inter._distance > 0) // Found Intersection
                                    {
                                        //AnnotationHandler.AddIntersectedPoint(inter._ray.GetPoint(inter._distance));
                                        Log.writeLog("detected Hit on Sphere");
                                        _renderer.CreatePointCloud(new Point3DCollection(inter._node._objects), Brushes.OrangeRed,false);
                                    }
                                }


                                // Refresh Points
                                while (_canvas.Children.Count > 10)
                                {
                                    _canvas.Children.RemoveRange(0, 2);
                                }

                                //Util.DrawPoint(canvas, head);
                                Util.DrawPoint(_canvas, tipRight);

                                ColorSpacePoint colorPoint = this.sensor.CoordinateMapper.MapCameraPointToColorSpace(handRight.Position);
                                int offset = 200;

                                handRight = Util.ScaleTo(handRight, _canvas.ActualWidth, _canvas.ActualHeight);
                                thumbRight = Util.ScaleTo(thumbRight, _canvas.ActualWidth, _canvas.ActualHeight);
                                tipRight = Util.ScaleTo(tipRight, _canvas.ActualWidth, _canvas.ActualHeight);

                                vector = new Vector3D
                                {
                                    X = tipRight.Position.X - handRight.Position.X,
                                    Y = tipRight.Position.Y - handRight.Position.Y,
                                    Z = tipRight.Position.Z - handRight.Position.Z
                                }*10;


                                var thumbR = thumbRight.Position;
                                var thumbRPoint3D = new Point3D(thumbR.X, thumbR.Y, thumbR.Z);


                                tipR = tipRight.Position;
                                var tipRPoint3D = new Point3D(tipR.X, tipR.Y, tipR.Z);

                                Util.DrawLine(_canvas, tipRPoint3D, tipRPoint3D + vector,Colors.Violet,2);

                                BuildBoundingBoxAroundLeftArm(
                                    Util.ScaleTo(tipLeft, _canvas.ActualWidth, _canvas.ActualHeight),
                                    Util.ScaleTo(elbowLeft, _canvas.ActualWidth, _canvas.ActualHeight));




                            }
                        }
                    }
                }
            }

        }

        private void BuildBoundingBoxAroundLeftArm(Joint tipLeft, Joint elbowLeft)
        {
            // Find all 4 perpendicular lines to this axis 
            Util.DrawLine(_canvas,
                Parser3DPoint.FromCameraSpaceToPoint3D(tipLeft.Position),
                Parser3DPoint.FromCameraSpaceToPoint3D(elbowLeft.Position
                ));
        }

        private ImageSource ToBitmap(ColorFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            byte[] pixels = new byte[width * height * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToArray(pixels);
            }
            else
            {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }
    }
}
