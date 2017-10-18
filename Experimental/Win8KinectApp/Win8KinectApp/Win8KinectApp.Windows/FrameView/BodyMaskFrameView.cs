using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Win8KinectApp.FrameKinectView;
using WindowsPreview.Kinect;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.Storage.Streams;

using System.Runtime.InteropServices;


namespace Win8KinectApp
{
    class BodyMaskFrameView : FrameView
    {
        private WriteableBitmap bitmap = null;
        private Image FrameDisplayImage;

        private MultiSourceFrameReader multiSourceFrameReader = null;

        private CoordinateMapper coordinateMapper = null;
        private DepthSpacePoint[] colorMappedToDepthPoints;

        public BodyMaskFrameView(Image FDI){
            this.FrameDisplayImage = FDI;
        }
  
        public override void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sensor != null && sensor.IsOpen)
            {
                sensor.Close();
            }
        }

        public override void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.GetDefault();

            FrameDescription colorFrameDescription = this.sensor.ColorFrameSource.FrameDescription;
            colorMappedToDepthPoints = new DepthSpacePoint[colorFrameDescription.Width 
                * colorFrameDescription.Height];

            this.multiSourceFrameReader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Infrared |
                FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.BodyIndex);

            this.coordinateMapper = sensor.CoordinateMapper;

            this.multiSourceFrameReader.MultiSourceFrameArrived += this.Reader_BodyMaskFrameArrived;


            this.bitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height);

            if (sensor != null)
            {
                sensor.Open();

                if (sensor.IsOpen)
                {
                    //KinectMessage.Text = "Developing kinect for Windows v2.0 App with Visual Studio 2015 on Windows 10";

                }
            }

        }

        private void Reader_BodyMaskFrameArrived(MultiSourceFrameReader sender, MultiSourceFrameArrivedEventArgs e)
        {

            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();

            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            BodyIndexFrame bodyIndexFrame = null;

            IBuffer depthFrameDataBuffer = null;
            IBuffer bodyIndexFrameData = null;
            IBufferByteAccess bodyIndexByteAccess = null;

            try
            {
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
                bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame();
                if (depthFrame == null || colorFrame == null || bodyIndexFrame == null) return;

                // Access the depth frame data directly via 
                //LockImageBuffer to avoid making a copy
                depthFrameDataBuffer = depthFrame.LockImageBuffer();
                this.coordinateMapper.MapColorFrameToDepthSpaceUsingIBuffer(
                   depthFrameDataBuffer,
                   this.colorMappedToDepthPoints);
                // Process Color
                colorFrame.CopyConvertedFrameDataToBuffer(
                   this.bitmap.PixelBuffer,
                   ColorImageFormat.Bgra);
                // Access the body index frame data directly via 
                // LockImageBuffer to avoid making a copy
                bodyIndexFrameData = bodyIndexFrame.LockImageBuffer();
                showMappedBodyFrame(depthFrame.FrameDescription.Width,
                       depthFrame.FrameDescription.Height,
                       bodyIndexFrameData, bodyIndexByteAccess);

            }
            finally
            {
                if (depthFrame != null) depthFrame.Dispose();
                if (colorFrame != null) colorFrame.Dispose();
                if (bodyIndexFrame != null) bodyIndexFrame.Dispose();

                if (depthFrameDataBuffer != null)
                {
                    // We must force a release of the IBuffer in order to  
                    // ensure that we have dropped all references to it.
                    System.Runtime.InteropServices.Marshal.ReleaseComObject
                     (depthFrameDataBuffer);
                }
                if (bodyIndexFrameData != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(bodyIndexFrameData);
                if (bodyIndexByteAccess != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(bodyIndexByteAccess);

            }
        }

        unsafe private void showMappedBodyFrame(int depthWidth, int depthHeight, IBuffer bodyIndexFrameData, IBufferByteAccess bodyIndexByteAccess)
        {
            bodyIndexByteAccess = (IBufferByteAccess)bodyIndexFrameData;
            byte* bodyIndexBytes = null;
            bodyIndexByteAccess.Buffer(out bodyIndexBytes);

            fixed (DepthSpacePoint* colorMappedToDepthPointsPointer =
                this.colorMappedToDepthPoints)
            {
                IBufferByteAccess bitmapBackBufferByteAccess =
                    (IBufferByteAccess)this.bitmap.PixelBuffer;

                byte* bitmapBackBufferBytes = null;
                bitmapBackBufferByteAccess.Buffer(out bitmapBackBufferBytes);

                // Treat the color data as 4-byte pixels
                uint* bitmapPixelsPointer = (uint*)bitmapBackBufferBytes;

                // Loop over each row and column of the color image
                // Zero out any pixels that don't correspond to a body index
                int colorMappedLength = this.colorMappedToDepthPoints.Length;
                for (int colorIndex = 0;
                         colorIndex < colorMappedLength;
                         ++colorIndex)
                {
                    float colorMappedToDepthX =
                         colorMappedToDepthPointsPointer[colorIndex].X;
                    float colorMappedToDepthY =
                         colorMappedToDepthPointsPointer[colorIndex].Y;

                    // The sentinel value is -inf, -inf, 
                    // meaning that no depth pixel corresponds to
                    // this color pixel.
                    if (!float.IsNegativeInfinity(colorMappedToDepthX) &&
                        !float.IsNegativeInfinity(colorMappedToDepthY))
                    {
                        // Make sure the depth pixel maps to a valid 
                        // point in color space
                        int depthX = (int)(colorMappedToDepthX + 0.5f);
                        int depthY = (int)(colorMappedToDepthY + 0.5f);

                        // If the point is not valid, there is 
                        // no body index there.
                        if ((depthX >= 0)
                         && (depthX < depthWidth)
                         && (depthY >= 0)
                         && (depthY < depthHeight))
                        {
                            int depthIndex = (depthY * depthWidth) + depthX;

                            // If we are tracking a body for the current pixel,
                            // do not zero out the pixel
                            if (bodyIndexBytes[depthIndex] != 0xff)
                            {
                                // this bodyIndexByte is good and is a body,
                                // loop again.
                                continue;
                            }
                        }
                    }
                    // this pixel does not correspond to a body 
                    // so make it black and transparent
                    bitmapPixelsPointer[colorIndex] = 0;
                }
            }

            this.bitmap.Invalidate();
            FrameDisplayImage.Source = this.bitmap;
        }

        [Guid("905a0fef-bc53-11df-8c49-001e4fc686da"),
                 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IBufferByteAccess { unsafe void Buffer(out byte* pByte);}
    }
}
