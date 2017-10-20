
using Windows.UI.Xaml.Controls;
using Win8KinectApp.FrameKinectView;
using System.Windows.Input;


using System;
using System.Windows;

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
            
            switch (display)
            {
                case FrameType.Infrared:
                    addDisplay(new InfraredFrameView(FrameDisplayImage0));
                    break;
                case FrameType.Color:
                    addDisplay(new ColorFrameView(FrameDisplayImage0));
                    break;
                case FrameType.BodyMask:
                    addDisplay(new BodyMaskFrameView(FrameDisplayImage0));
                    addDisplay(new ColorFrameView(FrameDisplayImage1));
                    break;
                case FrameType.ColorInfrared:
                    addDisplay(new ColorFrameView(FrameDisplayImage1));
                    addDisplay(new InfraredFrameView(FrameDisplayImage0));
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("Something went wrong. Cancelling...");
                    return;
            }


        }

        private void addDisplay(FrameView fr)
        {
            Loaded += fr.MainPage_Loaded;
            Unloaded += fr.MainPage_Unloaded;
        }


    }
}
