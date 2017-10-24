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
using System.Windows.Threading;




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
        private static FrameType DEFAULT_FRAMETYPE = FrameType.Color;

        private FrameView currentFrameView;

        public MainWindow()
        {
            init();
            setupCurrentDisplay(DEFAULT_FRAMETYPE);
            Log.writeLog("<Press on start Scan>");
        }

        private void setupCurrentDisplay(FrameType display)
        {
            

            switch (display)
            {
                case FrameType.Infrared:
                    break;
                case FrameType.Color:
                    Log.writeLog("Creating color image.");
                    currentFrameView = new ColorView(FrameDisplayImage);
                    addDisplay(currentFrameView);
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
            Log.writeLog("Fetching kinect data...");

            addDisplay(new PointCloudView()); //start pointcloud processing

            rend = new Renderer(group);

            int runs = 10;


            for (int i = 0; i <= runs; i++)
            {
                System.Threading.Thread.Sleep(100);
                Point3DCollection points = rend.ReadData();
                rend.CreatePointCloud(points);

                label_Cycle.Content = "cycle: " + i + " out of " + runs;

                System.Windows.Forms.Application.DoEvents();
            }



            Log.writeLog("Analysing process finished.");

        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
