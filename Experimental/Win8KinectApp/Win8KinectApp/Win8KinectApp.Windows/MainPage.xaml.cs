
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
            Color
        }

        private static FrameType DEFAULT_FRAMETYPE = FrameType.Infrared;
       
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
                    frame = new InfraredFrameView(FrameDisplayImage);
                    break;
                case FrameType.Color:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("Something went wrong. Cancelling...");
                    return;
                    break;
            }

            Loaded += frame.MainPage_Loaded;
            Unloaded += frame.MainPage_Unloaded;
        }


    }
}
