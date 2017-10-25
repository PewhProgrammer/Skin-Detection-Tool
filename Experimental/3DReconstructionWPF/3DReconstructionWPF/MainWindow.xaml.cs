using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using System.Windows.Media.Media3D;
using Microsoft.Kinect;
using _3DReconstructionWPF.GUI;
using _3DReconstructionWPF.FrameKinectView;





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

        private int runs = -1;
        private int cycleRuns = 0;

        private FrameView currentFrameView;
        private PointCloudView pcv;

        private Transform3D cameraFoval = new TranslateTransform3D(new Vector3D(0, 0, 0));
        private Point3D cameraPosition = new Point3D(0.2, 0.2, 5);

        private float rotateValue = 5;

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
                    AddDisplay(currentFrameView);
                    break;
                case FrameType.BodyMask:
                    break;
                default:
                    Log.writeLog("Display FrameType for Kinect not defined!");
                    return;
            }
        }

        private void init()
        {
            InitializeComponent();
            Log.initLog(textBox);

            rend = new Renderer(group);

           pcv = new PointCloudView(rend);
        }


        private void startScan_Click(object sender, RoutedEventArgs e)
        {
            Log.writeLog("Fetching kinect data...");

            Point3DCollection depthPoints = pcv.getDepthDataFromLatestFrame();
            if (depthPoints != null)
            {

                rend.CreatePointCloud(depthPoints);
                Log.writeLog("Analysing process finished.");
                cycleRuns++;
                label_Cycle.Content = "cycle: " + cycleRuns + " out of " + runs;
            }
            else Log.writeLog("Could not retrieve depth frame");

        }

        private void AddDisplay(FrameView fr)
        {
            Loaded += fr.MainPage_Loaded;
            Unloaded += fr.MainPage_Unloaded;
        }

        private void RotateLeft_Click(object sender, RoutedEventArgs e)
        {
            rotateValue -= 5f;
            //viewport.Camera.Transform = cameraFoval;

            Transform3D cameraRotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotateValue));
            viewport.Camera.Transform = cameraRotation;
        }

        private void RotateRight_Click(object sender, RoutedEventArgs e)
        {
            rotateValue += 5f;
            //5 is computated euclid distance between zero point and default camera position

            double newX = 5 * Math.Cos(90);
            double newY = 5 * Math.Sin(90);

            Point3D newPos = new Point3D(newX, 0, newY);

            Transform3D cameraTranslation = new TranslateTransform3D(new Point3D(newX, 0, newY) - cameraPosition);
            viewport.Camera.Transform = cameraTranslation;


            //Log.writeLog("vec: " + (new Point3D(newX, 0, newY) - cameraPosition).ToString());
            //Log.writeLog("new pos: " + newPos.ToString());

            Transform3D cameraRotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), rotateValue));
            viewport.Camera.Transform = cameraRotation;
        }
    }
}
