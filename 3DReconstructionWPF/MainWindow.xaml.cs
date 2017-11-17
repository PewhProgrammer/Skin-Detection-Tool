using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using System.Windows.Media.Media3D;
using Microsoft.Kinect;
using _3DReconstructionWPF.GUI;
using _3DReconstructionWPF.FrameKinectView;
using _3DReconstructionWPF.Computation;

using System.Windows.Media;
using _3DReconstructionWPF.Data;
using System.Windows.Input;

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
        private KinectSensor sensor;

        private ICP icp;

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

            sensor = KinectSensor.GetDefault();
            var rotationAngle = 0.707106781187f; // 0.707106781187f

            pointmatcher.net.EuclideanTransform trans = new pointmatcher.net.EuclideanTransform
            {
                translation = System.Numerics.Vector3.UnitX,
                rotation = System.Numerics.Quaternion.CreateFromRotationMatrix(new System.Numerics.Matrix4x4(
                    rotationAngle, rotationAngle, 0, 0,
                        -rotationAngle, rotationAngle, 0, 0,
                        0, 0, 1, 0,
                        1, 0, 0, 1
                    )),
                /*rotation = System.Numerics.Quaternion.Normalize(System.Numerics.Quaternion.CreateFromRotationMatrix(new System.Numerics.Matrix4x4(
                        1, 0, 0, 0,
                        0, 1, 0, 0,
                        0, 0, 1, 0,
                        0, 0, 0, 1
                    )))*/
            };

            icpData = new ICP.ICPData(null, trans);

            icp = new ICP();

            label_Cycle.Content = "cycle: " + cycleRuns;

            if (sensor != null)
            {

                if (sensor.IsOpen && sensor.IsAvailable)
                {
                    Log.writeLog("Kinect capture data available!");
                }
                else Log.writeLog("Kinect not found!");
            }
        }

        private Point3DCollection displayPointCloud;
        private Point3DCollection _reference;

        private Point3DCollection _referenceFeatures;
        private Point3DCollection _readingFeatures;

        private float transformSizeValue = 1f;

        private void StartScan_Click(object sender, RoutedEventArgs e)
        {

            //if (!checkKinectConnection()) return;
            _reference = displayPointCloud;
            _referenceFeatures = _readingFeatures;

            var depthData = pcv.getDepthDataFromLatestFrame();


            displayPointCloud = depthData.Item1;
            _readingFeatures = depthData.Item2;
            //displayPointCloud = rend.ReadData();


            if (displayPointCloud != null)
            {

                if (cycleRuns > 0)
                {
                    Log.writeLog("--------------------");

                    var rotationAngle = 0.707106781187f;

                    Matrix3D m = new Matrix3D(
               rotationAngle, 0, rotationAngle, 0,
                0, 1, 0, 0,
                -rotationAngle, 0, rotationAngle, 0,
                1, 0, 0, 1);

                    m = new Matrix3D(
               rotationAngle, rotationAngle, 0, 0,
                -rotationAngle, rotationAngle, 0, 0,
                0, 0, 1, 0,
                1, 0, 0, 1);
                    displayPointCloud = Util.RotatePoint3DCollection(displayPointCloud, m);
                    _readingFeatures = Util.RotatePoint3DCollection(_readingFeatures, m);


                    rend.CreatePointCloud(displayPointCloud, Brushes.YellowGreen);
                }
                else rend.CreatePointCloud(displayPointCloud, Brushes.White);

                cycleRuns++;
                label_Cycle.Content = "cycle: " + cycleRuns;
            }
            else Log.writeLog("Could not retrieve depth frame");

        }

        private void Transform_Click(object sender, RoutedEventArgs e)
        {
            if (_reference != null)
                TransformPC(_readingFeatures, _referenceFeatures);
            else Log.writeLog("missing reference or pending point data!");
        }

        private ICP.ICPData icpData;

        /*
         Point cloud color:
         AntiqueWhite is ref
         YellowGreen is pending
         blue is established
             */
        private void TransformPC(Point3DCollection source, Point3DCollection reference)
        {
            //compute transformation from reference
            icpData = icp.ComputeICP(
                Parser3DPoint.FromPoint3DToDataPoints(source),
                Parser3DPoint.FromPoint3DToDataPoints(reference),
                icpData.transform.Inverse());

            var p = ICP.ApplyTransformation(icpData.transform,Parser3DPoint.FromPoint3DToDataPoints(displayPointCloud));
            displayPointCloud = Parser3DPoint.FromDataPointsToPoint3DCollection(p);

            rend.CreatePointCloud(displayPointCloud, Brushes.BlueViolet);
        }

        private Boolean checkKinectConnection()
        {
            if (!(sensor.IsOpen && sensor.IsAvailable))
            {
                Log.writeLog("Could not establish connection to kinect device. Aborting...");
                return false;
            }
            return true;
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

        private void SavePointCloud_Click(object sender, RoutedEventArgs e)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter("pointCloud.txt", true))
            {
                for (int i = 0; i < displayPointCloud.Count; i++)
                {
                    Point3D p = displayPointCloud[i];
                    file.WriteLine(p.ToString());
                }
            }
            Log.writeLog("Saved the point cloud");
        }

        private void Readjust_Click(object sender, RoutedEventArgs e)
        {


            displayPointCloud = pcv.getDepthDataFromLatestFrame().Item1;
            //displayPointCloud = rend.ReadData();

            if (displayPointCloud != null)
            {

                Log.writeLog("--------------------");
                BVH bvh = new BVH();
                for (int i = 0; i < displayPointCloud.Count; i++)
                {
                    bvh.AddToScene(displayPointCloud[i]);
                }


                if (cycleRuns > 0)
                {
                    rend.CreatePointCloud(displayPointCloud, Brushes.YellowGreen);
                }
                else rend.CreatePointCloud(displayPointCloud, Brushes.White);

                cycleRuns++;
                label_Cycle.Content = "cycle: " + cycleRuns;
            }
            else Log.writeLog("Could not retrieve depth frame");
        }

        private AnnotationHandler.AnnotationType _annotation = AnnotationHandler.AnnotationType.Default;

        private void Annotate_Click(object sender, RoutedEventArgs e)
        {
            Log.writeLog("ANNOTATION NOT YET IMPLEMENTED");
            AnnotationHandler.Annotate(_annotation);
            //AnimationStoryboard.Storyboard.Pause(this);
        }

        private void minuteHand_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                //my code: moving the needle
                Log.writeLog("moved mouse");
            }
        }

        private bool _leftButtonDown = false;
        private Point _mousePosition;
        private float _scale = 0.2f;

        private void OnDragSourceMouseLeftButtonDown(object sender,
       MouseButtonEventArgs e)
        {
            _leftButtonDown = true;
            _mousePosition = e.GetPosition(e.Source as FrameworkElement);
        }

        private void OnDragSourceMouseMove(object sender, MouseEventArgs e)
        {
            if (!_leftButtonDown) return;

            viewport.Camera.Transform = ComputeDragOffsetX(_mousePosition, e.GetPosition((e.Source as FrameworkElement)));
            //viewport.Camera.Transform = ComputeDragOffsetY(_mousePosition, e.GetPosition((e.Source as FrameworkElement)));
        }
        private void OnDragSourceMouseLeftButtonUp(object sender,
     MouseButtonEventArgs e)
        {
            _leftButtonDown = false;
        }

        private Transform3D ComputeDragOffsetX(Point from, Point to)
        {
            //viewport.Camera.Transform = cameraFoval;
            double diffX = from.X - to.X;
            double diffY = from.Y - to.Y;


            rotateValue += ((float)diffX * 0.008f + (float)diffY * 0.008f) * 0.5f;

    
            return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(diffY*_scale, diffX*_scale, 0), rotateValue));
        }

        private void ScanFeatures_Click(object sender, RoutedEventArgs e)
        {
            _reference = displayPointCloud;
            displayPointCloud = pcv.getDepthDataFromLatestFrame().Item1;


            if (displayPointCloud != null)
            {

                if (cycleRuns > 0)
                {
                    Log.writeLog("--------------------");

                    transformSizeValue -= 0.18f;

                    Matrix3D m = new Matrix3D(
                        0.707106781187f, 0.707106781187f, 0, 0,
                        -0.707106781187f, 0.707106781187f, 0, 0,
                        0, 0, 1, 0,
                        1, 0, 0, 1);  //last column */

                    /*Matrix3D m = new Matrix3D(
                        1,0, 0, 0,
                        0,1, 0, 0,
                        0, 0, 1, 0,
                        1, 0, 0, 1);*/

                    Point3D[] k = new Point3D[displayPointCloud.Count];
                    displayPointCloud.CopyTo(k, 0);
                    int pcSize = displayPointCloud.Count;
                    displayPointCloud.Clear();

                    m.Transform(k);

                    /*for (int i = 0; i < pcSize; i++)
                    {
                        if(i > 3000)
                        displayPointCloud.Add(k[i]);
                    }*/


                    rend.CreatePointCloud(displayPointCloud, Brushes.YellowGreen);
                }
                else rend.CreatePointCloud(displayPointCloud, Brushes.White);

                cycleRuns++;
                label_Cycle.Content = "cycle: " + cycleRuns;
            }
            else Log.writeLog("Could not retrieve depth frame");
        }
    }
}
