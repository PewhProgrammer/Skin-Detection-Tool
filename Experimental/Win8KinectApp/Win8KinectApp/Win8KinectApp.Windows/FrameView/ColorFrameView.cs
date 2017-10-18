using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

using Win8KinectApp.FrameKinectView;
using WindowsPreview.Kinect;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls;

namespace Win8KinectApp
{
    class ColorFrameView : FrameView
    {

        // Size of the RGB pixel in the bitmap
        private const int BytesPerPixel = 4;

        private WriteableBitmap bitmap = null;

        private Image FrameDisplayImage;

        private ColorFrameReader colorFrameReader = null;

        public ColorFrameView(Image FDI){
            this.FrameDisplayImage = FDI;
        }

        public override void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.GetDefault();

            FrameDescription coloredFrameDescription =
                this.sensor.ColorFrameSource.FrameDescription;

           
            this.bitmap =
                new WriteableBitmap(coloredFrameDescription.Width, coloredFrameDescription.Height);

            this.colorFrameReader = this.sensor.ColorFrameSource.OpenReader();

            this.colorFrameReader.FrameArrived +=
                this.Reader_ColorFrameArrived;

            if (sensor != null)
            {
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

        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            bool coloredFrameProcessed = false;

            // InfraredFrame is IDisposable
            using (ColorFrame coloredFrame =
                e.FrameReference.AcquireFrame())
            {
                if (coloredFrame != null)
                {
                    FrameDescription coloredFrameDescription =
                coloredFrame.FrameDescription;

                    // verify data and write the new infrared frame data
                    // to the display bitmap
                    if ((coloredFrameDescription.Width == this.bitmap.PixelWidth) && 
                        (coloredFrameDescription.Height == this.bitmap.PixelHeight))
                    {
                        if (coloredFrame.RawColorImageFormat == ColorImageFormat.Bgra)
                        {
                            coloredFrame.CopyRawFrameDataToBuffer(this.bitmap.PixelBuffer);
                        }
                        else
                        {
                            coloredFrame.CopyConvertedFrameDataToBuffer(this.bitmap.PixelBuffer,
                                ColorImageFormat.Bgra);
                           
                        }

                        coloredFrameProcessed = true;
                    }
                }
            }

            if (coloredFrameProcessed)
            {
                this.bitmap.Invalidate();
                FrameDisplayImage.Source = this.bitmap;
            }
        }
    }
}
