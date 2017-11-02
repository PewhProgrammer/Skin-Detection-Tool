using System;
using System.Windows;

using Microsoft.Kinect;
using System.Windows.Controls;

using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace _3DReconstructionWPF.FrameKinectView
{
    class ColorView : FrameView
    {
        // Size of the RGB pixel in the bitmap
        private const int BytesPerPixel = 4;

        private ColorFrameReader colorFrameReader = null;

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

            

            this.colorFrameReader = this.sensor.ColorFrameSource.OpenReader();

            this.colorFrameReader.FrameArrived +=
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

        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {

            // InfraredFrame is IDisposable
            using (ColorFrame coloredFrame =
                e.FrameReference.AcquireFrame())
            {
                if (coloredFrame != null)
                {
                    frameDisplayImage.Source = ToBitmap(coloredFrame);
                }
            }
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
