using System;
using System.Windows;

using Microsoft.Kinect;
using System.Windows.Controls;

using System.Windows.Media.Imaging;
using System.Windows.Media;
using _3DReconstructionWPF.Computation;
using System.Windows.Media.Media3D;

namespace _3DReconstructionWPF.FrameKinectView
{
    class ColorView : FrameView
    {
        // Size of the RGB pixel in the bitmap
        private const int BytesPerPixel = 4;

        private MultiSourceFrameReader _multiSourceFrameReader = null;

        // canvas on top of rgb color stream
        private Canvas _canvas = ((MainWindow)Application.Current.MainWindow).canvas;

        Image frameDisplayImage;
        public ColorView(Image FDI)
        {
            this.frameDisplayImage = FDI;
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

                    float k = 2; 
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

                                // Refresh Points
                                if (_canvas.Children.Count > 20)
                                {
                                    _canvas.Children.RemoveRange(0, 2);
                                }

                                //Util.DrawPoint(canvas, head);
                                Util.DrawPoint(_canvas, tipRight);

                                handRight = Util.ScaleTo(handRight, _canvas.ActualWidth, _canvas.ActualHeight);
                                thumbRight = Util.ScaleTo(thumbRight, _canvas.ActualWidth, _canvas.ActualHeight);
                                tipRight = Util.ScaleTo(tipRight, _canvas.ActualWidth, _canvas.ActualHeight);

                                var vector = new Vector3D
                                {
                                    X = tipRight.Position.X - handRight.Position.X,
                                    Y = tipRight.Position.Y - handRight.Position.Y,
                                    Z = tipRight.Position.Z - handRight.Position.Z
                                } *20;


                                var thumbR = thumbRight.Position;
                                var thumbRPoint3D = new Point3D(thumbR.X, thumbR.Y, thumbR.Z);


                                var tipR = tipRight.Position;
                                var tipRPoint3D = new Point3D(tipR.X, tipR.Y, tipR.Z);

                                //Util.DrawLine(_canvas, tipRPoint3D, tipRPoint3D + vector);

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
