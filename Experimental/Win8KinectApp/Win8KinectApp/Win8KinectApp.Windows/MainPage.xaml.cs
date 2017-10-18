
using Windows.UI.Xaml.Controls;
using Win8KinectApp.FrameKinectView;
using System;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Win8KinectApp
{
    public sealed partial class MainPage : Page
    {

        public enum FrameType
        {
            Infrared,
            Color,
            BodyMask,
            ColorInfrared
        }

        private static FrameType DEFAULT_FRAMETYPE = FrameType.BodyMask;
       
        public MainPage()
        {
            this.InitializeComponent();
            setupCurrentDisplay(DEFAULT_FRAMETYPE); 
        }

        private void setupCurrentDisplay(FrameType display)
        {
            FrameView frame = null;

            System.Diagnostics.Debug.WriteLine("hey");

            switch (display)
            {
                case FrameType.Infrared:
                    frame = new InfraredFrameView(FrameDisplayImage0);
                    break;
                case FrameType.Color:
                    frame = new ColorFrameView(FrameDisplayImage0);
                    break;
                case FrameType.BodyMask:
                    frame = new BodyMaskFrameView(FrameDisplayImage0);
                    break;
                case FrameType.ColorInfrared:
                    frame = new ColorFrameView(FrameDisplayImage0);
                    Loaded += new InfraredFrameView(FrameDisplayImage1).MainPage_Loaded;
                    Unloaded += new InfraredFrameView(FrameDisplayImage1).MainPage_Unloaded;
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("Something went wrong. Cancelling...");
                    return;
            }

            Loaded += frame.MainPage_Loaded;
            Unloaded += frame.MainPage_Unloaded;
        }


    }
}
