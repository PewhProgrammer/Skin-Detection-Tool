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
        private ColorView _rgbv;
        private PointCloudView _pcv;

        private Point _mousePosition;
        private Point3D _cameraPosition = new Point3D(0.2, 0.2, 5);
        private Point3D _thumbReading, _thumbReference;
        private Transform3D _cameraFoval = new TranslateTransform3D(new Vector3D(0, 0, 0));
        private pointmatcher.net.EuclideanTransform _initialTransformation;

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

            /// TEST ///
            //ExecuteTests();
        }

        private void SetupCurrentDisplay(FrameType display)
        {

            switch (display)
            {
                case FrameType.Infrared:
                    break;
                case FrameType.Color:
                    _rgbv = new ColorView(FrameDisplayImage, _renderer);
                    AddDisplay(_rgbv);
                    break;
                case FrameType.BodyMask:
                    break;
                default:
                    Log.writeLog("Display FrameType for Kinect not defined!");
                    return;
            }
        }

        private void ExecuteTests()
        {
            /// TEST ////
            var refPoint = new Point3D(-1, -1, -1);
            var transPoint = new Point3D(2, 2, 2);

            var transform = Util.ComputeInitialTransformation(refPoint, transPoint);

            Ray ray = new Ray(new Point3D(0, 0, 10), new Vector3D(0.01f, 0.08f, -1));
            BBox box = new BBox(new Point3D(-1, -1, -1), new Point3D(1, 1, 1));
            var hit = box.Intersect(ray);

            var points = _renderer.ReadData();
            _rgbv._bvh = BVH.InitBVH(points);

            Intersection inter = _rgbv._bvh.Intersect(ray, float.MaxValue);
            if (inter.Hit()) Log.writeLog("Hit detected!!");

            Point3DCollection collection = new Point3DCollection(inter._node._objects);

            //_renderer.CreatePointCloud(collection, Brushes.White);
            Log.writeLog("test end");


            /// TEST END ///
        }

        private void Init()
        {
            InitializeComponent();
            Log.initLog(textBox);

            _renderer = new Renderer(group);
            _pcv = new PointCloudView(_renderer);

            _sensor = KinectSensor.GetDefault();

            _initialTransformation = new pointmatcher.net.EuclideanTransform
            {
                translation = System.Numerics.Vector3.Zero,
                rotation = System.Numerics.Quaternion.Normalize(System.Numerics.Quaternion.CreateFromRotationMatrix(new System.Numerics.Matrix4x4(
                        1, 0, 0, 0,
                        0, 1, 0, 0,
                        0, 0, 1, 0,
                        0, 0, 0, 1
                    )))
            };

            _icpData = new ICP.ICPData(null, _initialTransformation);
            _icp = new ICP();
            label_Cycle.Content = "cycle: " + _cycleRuns;

            if (_sensor != null) { if (_sensor.IsOpen && _sensor.IsAvailable) Log.writeLog("Kinect capture data available!"); }


        }

        private void StartScan_Click(object sender, RoutedEventArgs e)
        {

            //if (!checkKinectConnection()) return;
            _reference = _displayPointCloud;
            _referenceFeatures = _readingFeatures;


            /* var depthData = _pcv.GetDepthDataFromLatestFrame();
             _displayPointCloud = depthData.Item1;
             _readingFeatures = depthData.Item2;*/


            //_displayPointCloud = depthData.Item2;



            //_displayPointCloud = _renderer.ReadData();
            //_rgbv._bvh = BVH.InitBVH(_displayPointCloud);

            var point1 = new Point3D(-0.8f, 0, 0);
            var point2 = new Point3D(-1f, 0, 0);
            var point3 = new Point3D(-0.6f, 0.5f, 0);
            var point4 = new Point3D(0.8f, -0.5f, 0);

            var pcReference = new Point3DCollection
                {
                    point1,
                    point2,
                    point3
                };

            if (_cycleRuns == 0)
            {
                // populate all feature points
                _displayPointCloud = Parser3DPoint.GetPopulatedPointCloud(point1, point2, point3, point4, _initialTransformation);
                _readingFeatures = _displayPointCloud;

                //maintain most important feature point;
                _thumbReference = _readingFeatures[0];

                _renderer.CreatePointCloud(_displayPointCloud, Brushes.White);
                Log.writeLog("Scanned reference skeleton");
            }
            else
            {
                Log.writeLog("--------------------");

                var rotationAngle = 0.707106781187f;

                Matrix3D m = new Matrix3D(
                rotationAngle, 0, rotationAngle, 0,
                0, 1, 0, 0,
                -rotationAngle, 0, rotationAngle, 0,
                1, 0, 0, 1);

                // Transform the thumb according to m
                _thumbReading = m.Transform(_readingFeatures[0]);
                point1 = m.Transform(point1);
                point2 = m.Transform(point2);
                point3 = m.Transform(point3);
                point4 = m.Transform(point4);

                var pcReading = new Point3DCollection
                {
                    point1,
                    point2,
                    point3
                };

                _initialTransformation = Util.ComputeInitialTransformation(pcReading, pcReference);

                pcReading.Add(point4);
                _renderer.CreatePointCloud(pcReading, Brushes.Violet);

                //_readingFeatures = Util.RotatePoint3DCollection(_readingFeatures, m);

                var pc = new Point3DCollection();//Util.TransformPoint3DCollection(_readingFeatures, _initialTransformation);
                //pc.Add(_thumbReading);
                _thumbReading = Parser3DPoint._transformPoint3D(_thumbReading, new Point3D(0, 0, 0), _initialTransformation);
                pc.Add(_thumbReading);
                _thumbReading = Parser3DPoint._transformPoint3D(point2, new Point3D(0, 0, 0), _initialTransformation);
                pc.Add(_thumbReading);
                _thumbReading = Parser3DPoint._transformPoint3D(point3, new Point3D(0, 0, 0), _initialTransformation);
                pc.Add(_thumbReading);
                _thumbReading = Parser3DPoint._transformPoint3D(point4, new Point3D(0, 0, 0), _initialTransformation);
                pc.Add(_thumbReading);

                _renderer.CreatePointCloud(pc, Brushes.YellowGreen, false);
                //_renderer.CreatePointCloud(_displayPointCloud, Brushes.YellowGreen);
            }

            _cycleRuns++;
            label_Cycle.Content = "cycle: " + _cycleRuns;

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

            //compute transformation from reference
            _icpData = _icp.ComputeICP(
                Parser3DPoint.FromPoint3DToDataPoints(source),
                Parser3DPoint.FromPoint3DToDataPoints(reference),
                _initialTransformation);

            _icpData.transform = _initialTransformation;

            var p = ICP.ApplyTransformation(_icpData.transform, Parser3DPoint.FromPoint3DToDataPoints(_displayPointCloud));
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

        private void Annotate_Click(object sender, RoutedEventArgs e)
        {
            Log.writeLog("ANNOTATION NOT YET IMPLEMENTED");
            //AnnotationHandler.Annotate(_annotation);
            //AnimationStoryboard.Storyboard.Pause(this);

            Point3DCollection A = new Point3DCollection();
            Point3DCollection B = new Point3DCollection();

            
            var p1 = new Point3D(-0.8f, 0, 0);
            var p2 = new Point3D(-1f, 0, 0);
            var p3 = new Point3D(-0.6f, 0.5f, 0);
            
            /*
            var p1 = new Point3D(1, 0, 0);
            var p2 = new Point3D(2, 0, 0);
            var p3 = new Point3D(3, 0, 0);
            */

            B.Add(p1);
            B.Add(p2);
            B.Add(p3);


            var rotationAngle = 0.707106781187f;

            var m = new Matrix3D(
            rotationAngle, rotationAngle, 0, 0,
            -rotationAngle, rotationAngle, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);


            A.Add(m.Transform(p1));
            A.Add(m.Transform(p2));
            A.Add(m.Transform(p3));

            var transform = Util.ComputeInitialTransformation(A, B);

            var err = ComputeRMSE(A, B, transform);
            if (err > 0.01f) Log.writeLog("Error derivation is too high: " + err);
            else Log.writeLog("Error derivation is correct: " + err);
        }

        private double ComputeRMSE(Point3DCollection A, Point3DCollection B, pointmatcher.net.EuclideanTransform transform)
        {
            var k = System.Numerics.Vector3.One;
            var sum = 0.0d;
            for (int i = 0; i < B.Count; i++)
            {
                k = transform.Apply(new System.Numerics.Vector3((float)A[i].X, (float)A[i].Y, (float)A[i].Z));
                var convertK = new Point3D(k.X, k.Y, k.Z);
                var error = B[i] - convertK;
                sum += Vector3D.DotProduct(error, error);

            }

            return Math.Sqrt(sum / A.Count);
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


            return new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(diffY * _scale, diffX * _scale, 0), _rotateValue));
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
