using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using Microsoft.Kinect;

namespace _3DReconstructionWPF.FrameKinectView
{

    abstract class FrameView
    {

        protected KinectSensor sensor;

        abstract public void MainPage_Unloaded(object sender, RoutedEventArgs e);

        abstract public void MainPage_Loaded(object sender, RoutedEventArgs e);
    }
}
