using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml;

namespace Win8KinectApp.FrameKinectView
{
    abstract class FrameView
    {
        abstract public void MainPage_Unloaded(object sender, RoutedEventArgs e);

        abstract public void MainPage_Loaded(object sender, RoutedEventArgs e);
    }
}
