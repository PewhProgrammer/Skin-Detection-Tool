using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WindowsPreview.Kinect;
using Windows.UI.Xaml.Media.Imaging;

using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Win8KinectApp.FrameKinectView;

namespace Win8KinectApp
{
    class InfraredFrameView : FrameView
    {

        // Size of the RGB pixel in the bitmap
        private const int BytesPerPixel = 4;

        private WriteableBitmap bitmap = null;

        //Infrared Frame
        private InfraredFrameReader infraredFrameReader = null;
        private ushort[] infraredFrameData = null;
        private byte[] infraredPixels = null;

        /** Pixel Variables **/

        /// <summary>
        /// The highest value that can be returned in the InfraredFrame.
        /// It is cast to a float for readability in the visualization code.
        /// </summary>
        private const float InfraredSourceValueMaximum =
            (float)ushort.MaxValue;

        /// </summary>
        /// Used to set the lower limit, post processing, of the
        /// infrared data that we will render.
        /// Increasing or decreasing this value sets a brightness
        /// "wall" either closer or further away.
        /// </summary>
        private const float InfraredOutputValueMinimum = 0.01f;

        /// <summary>
        /// The upper limit, post processing, of the
        /// infrared data that will render.
        /// </summary>
        private const float InfraredOutputValueMaximum = 1.0f;

        /// <summary>
        /// The InfraredSceneValueAverage value specifies the 
        /// average infrared value of the scene. 
        /// This value was selected by analyzing the average
        /// pixel intensity for a given scene.
        /// This could be calculated at runtime to handle different
        /// IR conditions of a scene (outside vs inside).
        /// </summary>
        private const float InfraredSceneValueAverage = 0.08f;

        /// <summary>
        /// The InfraredSceneStandardDeviations value specifies 
        /// the number of standard deviations to apply to
        /// InfraredSceneValueAverage.
        /// This value was selected by analyzing data from a given scene.
        /// This could be calculated at runtime to handle different
        /// IR conditions of a scene (outside vs inside).
        /// </summary>
        private const float InfraredSceneStandardDeviations = 3.0f;

        private Image FrameDisplayImage;

        public InfraredFrameView(Image FDI)
        {
            this.FrameDisplayImage = FDI;
        }

        override
        public void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sensor != null && sensor.IsOpen)
            {
                sensor.Close();
            }
        }

        override
        public void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.GetDefault();

            // get the infraredFrameDescription from the
            // InfraredFrameSource
            FrameDescription infraredFrameDescription =
                this.sensor.InfraredFrameSource.FrameDescription;

            // open the reader for the infrared frames
            this.infraredFrameReader =
                this.sensor.InfraredFrameSource.OpenReader();

            // wire handler for frame arrival
            this.infraredFrameReader.FrameArrived +=
                this.Reader_InfraredFrameArrived;

            // allocate space to put the pixels being 
            // received and converted
            this.infraredFrameData =
                new ushort[infraredFrameDescription.Width *
                infraredFrameDescription.Height];
            this.infraredPixels =
                new byte[infraredFrameDescription.Width *
                infraredFrameDescription.Height * BytesPerPixel];

            // create the bitmap to display
            this.bitmap =
                new WriteableBitmap(infraredFrameDescription.Width,
                infraredFrameDescription.Height);

            if (sensor != null)
            {
                sensor.Open();

                if (sensor.IsOpen)
                {
                    //KinectMessage.Text = "Developing kinect for Windows v2.0 App with Visual Studio 2015 on Windows 10";

                }
            }
        }

        private void Reader_InfraredFrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            bool infraredFrameProcessed = false;

            // InfraredFrame is IDisposable
            using (InfraredFrame infraredFrame =
                e.FrameReference.AcquireFrame())
            {
                if (infraredFrame != null)
                {
                    FrameDescription infraredFrameDescription =
                infraredFrame.FrameDescription;

                    // verify data and write the new infrared frame data
                    // to the display bitmap
                    if (((infraredFrameDescription.Width *
                        infraredFrameDescription.Height)
                     == this.infraredFrameData.Length) &&
                        (infraredFrameDescription.Width ==
                        this.bitmap.PixelWidth) &&
                (infraredFrameDescription.Height ==
                    this.bitmap.PixelHeight))
                    {
                        // Copy the pixel data from the image to a 
                        // temporary array
                        infraredFrame.CopyFrameDataToArray(
                            this.infraredFrameData);

                        infraredFrameProcessed = true;
                    }
                }
            }

            // we got a frame, convert and render
            if (infraredFrameProcessed)
            {
                ConvertInfraredDataToPixels();
                RenderPixelArray(this.infraredPixels);
            }
        }

        // Reader_InfraredFrameArrived() before this...
        private void ConvertInfraredDataToPixels()
        {
            // Convert the infrared to RGB
            int colorPixelIndex = 0;
            for (int i = 0; i < this.infraredFrameData.Length; ++i)
            {
                // normalize the incoming infrared data (ushort) to 
                // a float ranging from InfraredOutputValueMinimum
                // to InfraredOutputValueMaximum] by

                // 1. dividing the incoming value by the 
                // source maximum value
                float intensityRatio = (float)this.infraredFrameData[i] /
             InfraredSourceValueMaximum;

                // 2. dividing by the 
                // (average scene value * standard deviations)
                intensityRatio /=
                 InfraredSceneValueAverage * InfraredSceneStandardDeviations;

                // 3. limiting the value to InfraredOutputValueMaximum
                intensityRatio = Math.Min(InfraredOutputValueMaximum,
                    intensityRatio);

                // 4. limiting the lower value InfraredOutputValueMinimum
                intensityRatio = Math.Max(InfraredOutputValueMinimum,
                    intensityRatio);

                // 5. converting the normalized value to a byte and using 
                // the result as the RGB components required by the image
                byte intensity = (byte)(intensityRatio * 255.0f);
                this.infraredPixels[colorPixelIndex++] = intensity; //Blue
                this.infraredPixels[colorPixelIndex++] = intensity; //Green
                this.infraredPixels[colorPixelIndex++] = intensity; //Red
                this.infraredPixels[colorPixelIndex++] = 255;       //Alpha           
            }
        }

        // ConvertInfraredDataToPixels() before this...
        private void RenderPixelArray(byte[] pixels)
        {
            pixels.CopyTo(this.bitmap.PixelBuffer);
            this.bitmap.Invalidate();
            FrameDisplayImage.Source = this.bitmap;
        }
    }
}
