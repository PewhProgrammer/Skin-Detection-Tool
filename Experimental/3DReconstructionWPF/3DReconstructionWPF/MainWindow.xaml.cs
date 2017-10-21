using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Media;

using System.Windows.Media.Media3D;
using Microsoft.Kinect;
using _3DReconstructionWPF.GUI;
using _3DReconstructionWPF.FrameKinectView;

using System.Windows.Controls;



namespace _3DReconstructionWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public enum FrameType
        {
            Infrared, //Not implemented in current project
            Color,
            BodyMask //Not implemented in current project
        }

        private Renderer rend;
        private static FrameType DEFAULT_FRAMETYPE = FrameType.BodyMask;

        public MainWindow()
        {
            init();
            setupCurrentDisplay(DEFAULT_FRAMETYPE);
        }

        private void setupCurrentDisplay(FrameType display)
        {

            switch (display)
            {
                case FrameType.Infrared:
                    break;
                case FrameType.Color:
                    Log.writeLog("Creating color image.");
                    addDisplay(new ColorView(FrameDisplayImage));
                    break;
                case FrameType.BodyMask:
                    break;
                default:
                    Log.writeLog("Display FrameType for Kinect not defined!");
                    return;
            }
        }

        private void addDisplay(FrameView fr)
        {
            Loaded += fr.MainPage_Loaded;
            Unloaded += fr.MainPage_Unloaded;
        }

        private void init()
        {
            InitializeComponent();
            Log.initLog(textBox);

            //KinectSensor sensor = KinectSensor.GetDefault();
            //sensor.Open();

        }

        private void startScan_Click(object sender, RoutedEventArgs e)
        {
    
            rend = new Renderer(group);

            Point3DCollection points = rend.ReadData();
            rend.CreatePointCloud(points);
           
        }
    }
}
