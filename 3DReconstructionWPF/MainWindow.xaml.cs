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

        private static FrameType DEFAULT_FRAMETYPE = FrameType.Color;
        private AnnotationHandler.AnnotationType _annotation = AnnotationHandler.AnnotationType.Default;
        public enum FrameType
        {
            Infrared, //Not implemented in current project
            Color,
            BodyMask //Not implemented in current project
        }


        private Renderer _renderer;
        private FrameView _currentFrameView;
        private PointCloudView _pcv;

        private Point _mousePosition;
        private Point3D _cameraPosition = new Point3D(0.2, 0.2, 5);
        private Transform3D _cameraFoval = new TranslateTransform3D(new Vector3D(0, 0, 0));

        private KinectSensor _sensor;

        private ICP _icp;
        private ICP.ICPData _icpData;

        private Point3DCollection _displayPointCloud;
        private Point3DCollection _reference;
        private Point3DCollection _readingFeatures;
        private Point3DCollection _referenceFeatures;

        private int _cycleRuns = 0;
        private float _rotateValue = 5;
        private float _scale = 0.2f;
        private float _transformSizeValue = 1f;
        private bool _leftButtonDown = false;



        public MainWindow()
        {
            Init();
            SetupCurrentDisplay(DEFAULT_FRAMETYPE);
        }

        private void SetupCurrentDisplay(FrameType display)
        {

            switch (display)
            {
                case FrameType.Infrared:
                    break;
                case FrameType.Color:
                    _currentFrameView = new ColorView(FrameDisplayImage);
                    AddDisplay(_currentFrameView);
                    break;
                case FrameType.BodyMask:
                    break;
                default:
                    Log.writeLog("Display FrameType for Kinect not defined!");
                    return;
            }
        }

        private void Init()
        {


            /// TEST ////
            var refPoint = new Point3D(-1, -1, -1);
            var transPoint = new Point3D(2, 2, 2);

            var transform = Util.ComputeInitialTransformation(refPoint,transPoint);

            Ray ray = new Ray(new Point3D(0, 0, 10), new Vector3D(0.1f,0.1f,-1));
            BBox box = new BBox(new Point3D(-1 ,- 1, - 1), new Point3D(1,1,1));
            var hit = box.Intersect(ray);

            Log.writeLog("test end");

            /// TEST END ///
            InitializeComponent();
            Log.initLog(textBox);

            _renderer = new Renderer(group);

            _pcv = new PointCloudView(_renderer);

            _sensor = KinectSensor.GetDefault();
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

            _icpData = new ICP.ICPData(null, trans);

            _icp = new ICP();

            label_Cycle.Content = "cycle: " + _cycleRuns;

            if (_sensor != null)
            {

                if (_sensor.IsOpen && _sensor.IsAvailable)
                {
                    Log.writeLog("Kinect capture data available!");
                }
                else Log.writeLog("Kinect not found!");
            }
        }

        private void StartScan_Click(object sender, RoutedEventArgs e)
        {

            //if (!checkKinectConnection()) return;
            _reference = _displayPointCloud;
            _referenceFeatures = _readingFeatures;

            var depthData = _pcv.GetDepthDataFromLatestFrame();


            _displayPointCloud = depthData.Item1;
            _readingFeatures = depthData.Item2;
            //displayPointCloud = rend.ReadData();


            if (_displayPointCloud != null)
            {

                if (_cycleRuns > 0)
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
                    _displayPointCloud = Util.RotatePoint3DCollection(_displayPointCloud, m);
                    _readingFeatures = Util.RotatePoint3DCollection(_readingFeatures, m);


                    _renderer.CreatePointCloud(_displayPointCloud, Brushes.YellowGreen);
                }
                else _renderer.CreatePointCloud(_displayPointCloud, Brushes.White);

                _cycleRuns++;
                label_Cycle.Content = "cycle: " + _cycleRuns;
            }
            else Log.writeLog("Could not retrieve depth frame");

        }

        private void Transform_Click(object sender, RoutedEventArgs e)
        {
            if (_reference != null)
                TransformPC(_readingFeatures, _referenceFeatures);
            else Log.writeLog("missing reference or pending point data!");
        }


        /*
         Point cloud color:
         AntiqueWhite is ref
         YellowGreen is pending
         blue is established
             */
        private void TransformPC(Point3DCollection source, Point3DCollection reference)
        {

            // As far as i know, source[i] maps to reference[i]
            // compute initial transformation

            var initialTransformation = Util.ComputeInitialTransformation(source[0], reference[0]);

            //compute transformation from reference
            _icpData = _icp.ComputeICP(
                Parser3DPoint.FromPoint3DToDataPoints(source),
                Parser3DPoint.FromPoint3DToDataPoints(reference),
                _icpData.transform.Inverse());

            var p = ICP.ApplyTransformation(_icpData.transform,Parser3DPoint.FromPoint3DToDataPoints(_displayPointCloud));
            _displayPointCloud = Parser3DPoint.FromDataPointsToPoint3DCollection(p);

            _renderer.CreatePointCloud(_displayPointCloud, Brushes.BlueViolet);
        }

        private Boolean CheckKinectConnection()
        {
            if (!(_sensor.IsOpen && _sensor.IsAvailable))
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
            _rotateValue -= 5f;
            //viewport.Camera.Transform = cameraFoval;

            Transform3D cameraRotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), _rotateValue));
            viewport.Camera.Transform = cameraRotation;
        }

        private void RotateRight_Click(object sender, RoutedEventArgs e)
        {
            _rotateValue += 5f;
            //5 is computated euclid distance between zero point and default camera position

            double newX = 5 * Math.Cos(90);
            double newY = 5 * Math.Sin(90);

            Point3D newPos = new Point3D(newX, 0, newY);

            Transform3D cameraTranslation = new TranslateTransform3D(new Point3D(newX, 0, newY) - _cameraPosition);
            viewport.Camera.Transform = cameraTranslation;


            //Log.writeLog("vec: " + (new Point3D(newX, 0, newY) - cameraPosition).ToString());
            //Log.writeLog("new pos: " + newPos.ToString());

            Transform3D cameraRotation = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), _rotateValue));
            viewport.Camera.Transform = cameraRotation;
        }

        private void SavePointCloud_Click(object sender, RoutedEventArgs e)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter("pointCloud.txt", true))
            {
                for (int i = 0; i < _displayPointCloud.Count; i++)
                {
                    Point3D p = _displayPointCloud[i];
                    file.WriteLine(p.ToString());
                }
            }
            Log.writeLog("Saved the point cloud");
        }

        private void Readjust_Click(object sender, RoutedEventArgs e)
        {


            _displayPointCloud = _pcv.GetDepthDataFromLatestFrame().Item1;
            //displayPointCloud = rend.ReadData();

            if (_displayPointCloud != null)
            {

                Log.writeLog("--------------------");
                BVH bvh = new BVH();
                for (int i = 0; i < _displayPointCloud.Count; i++)
                {
                    bvh.AddToScene(_displayPointCloud[i]);
                }


                if (_cycleRuns > 0)
                {
                    _renderer.CreatePointCloud(_displayPointCloud, Brushes.YellowGreen);
                }
                else _renderer.CreatePointCloud(_displayPointCloud, Brushes.White);

                _cycleRuns++;
                label_Cycle.Content = "cycle: " + _cycleRuns;
            }
            else Log.writeLog("Could not retrieve depth frame");
        }

        private void Annotate_Click(object sender, RoutedEventArgs e)
        {
            Log.writeLog("ANNOTATION NOT YET IMPLEMENTED");
            AnnotationHandler.Annotate(_annotation);
            //AnimationStoryboard.Storyboard.Pause(this);
        }

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


            _rotateValue += ((float)diffX * 0.008f + (float)diffY * 0.008f) * 0.5f;

    
            return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(diffY*_scale, diffX*_scale, 0), _rotateValue));
        }

        private void ScanFeatures_Click(object sender, RoutedEventArgs e)
        {
            _reference = _displayPointCloud;
            _displayPointCloud = _pcv.GetDepthDataFromLatestFrame().Item1;


            if (_displayPointCloud != null)
            {

                if (_cycleRuns > 0)
                {
                    Log.writeLog("--------------------");

                    _transformSizeValue -= 0.18f;

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

                    Point3D[] k = new Point3D[_displayPointCloud.Count];
                    _displayPointCloud.CopyTo(k, 0);
                    int pcSize = _displayPointCloud.Count;
                    _displayPointCloud.Clear();

                    m.Transform(k);

                    /*for (int i = 0; i < pcSize; i++)
                    {
                        if(i > 3000)
                        displayPointCloud.Add(k[i]);
                    }*/


                    _renderer.CreatePointCloud(_displayPointCloud, Brushes.YellowGreen);
                }
                else _renderer.CreatePointCloud(_displayPointCloud, Brushes.White);

                _cycleRuns++;
                label_Cycle.Content = "cycle: " + _cycleRuns;
            }
            else Log.writeLog("Could not retrieve depth frame");
        }
    }
}
