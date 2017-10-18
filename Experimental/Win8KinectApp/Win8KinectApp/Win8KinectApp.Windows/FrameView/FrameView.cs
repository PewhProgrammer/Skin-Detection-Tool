using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using WindowsPreview.Kinect;

namespace Win8KinectApp.FrameKinectView
{

    abstract class FrameView
    {

        protected KinectSensor sensor;

        abstract public void MainPage_Unloaded(object sender, RoutedEventArgs e);

        abstract public void MainPage_Loaded(object sender, RoutedEventArgs e);
    }
}
