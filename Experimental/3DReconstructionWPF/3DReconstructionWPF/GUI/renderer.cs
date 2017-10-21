using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DReconstructionWPF.GUI
{
    class Renderer
    {
        private MeshGeometry3D pointCloudMesh;
        private Model3DGroup group;

        public Renderer(Model3DGroup group)
        {
            this.group = group;
            createAxis();
        }

        public void CreatePointCloud(Point3DCollection points){
            pointCloudMesh = new MeshGeometry3D();

       

      for (int i = 0; i < points.Count; i++)
      {
                //System.Threading.Thread.Sleep(10);
                //Log.writeLog("Point created: (" + points[i].X+","+points[i].Y+","+points[i].Z+")");
                AddCubeToMesh(pointCloudMesh, points[i], 0.005);
      }
            Log.writeLog(points.Count + " vertices added");

      pointCloudMesh.Freeze();


            GeometryModel3D mGeometry = new GeometryModel3D(pointCloudMesh, new DiffuseMaterial(Brushes.YellowGreen));
            mGeometry.Transform = new Transform3DGroup();
            group.Children.Add(mGeometry);
        }

        private void createAxis()
        {
            MeshGeometry3D axisMesh = new MeshGeometry3D();
            //x-axis
            for(int i = -100;i < 100; i++)
            {
                Point3D point = new Point3D(i/30.0f,0,0);
                //Log.writeLog("Axis Point created: " + "(" + point.X + ", " + point.Y + ", " + point.Z + ")");
                AddCubeToMesh(axisMesh, point, 0.005);
            }

            addToScene(axisMesh, Brushes.Red);
            axisMesh = new MeshGeometry3D();

            //y-axis
            for (int i = -100; i < 100; i++)
            {
                Point3D point = new Point3D(0, i / 30.0f, 0);
                //Log.writeLog("Axis Point created: " + "(" + point.X + ", " + point.Y + ", " + point.Z + ")");
                AddCubeToMesh(axisMesh, point, 0.005);
            }

            addToScene(axisMesh, Brushes.Green);

            axisMesh = new MeshGeometry3D();

            //z-axis
            for (int i = -5; i < 5; i++)
            {
                Point3D point = new Point3D(0, 0, i / 1.0f);
                //Log.writeLog("Axis Point created: " + "(" + point.X + ", " + point.Y + ", " + point.Z + ")");
                AddCubeToMesh(axisMesh, point, 0.005);
            }

            addToScene(axisMesh, Brushes.Blue);

        }

        private void addToScene(MeshGeometry3D mesh, Brush k)
        {
            GeometryModel3D mGeometry = new GeometryModel3D(mesh, new DiffuseMaterial(k));
            mGeometry.Transform = new Transform3DGroup();
            group.Children.Add(mGeometry);
        }

    public Point3DCollection ReadData()
    {
      // read 3d points from a file or create at runtime
      return CreateSphere(70, 70);
    }

    private Point3DCollection CreateSphere(int hDiv, int vDiv)
    {
      double maxTheta = Math.PI * 2;
      double minY = -1.0;
      double maxY = 1.0;

      double dt = maxTheta / hDiv;
      double dy = (maxY - minY) / vDiv;

      MeshGeometry3D mesh = new MeshGeometry3D();
      Point3DCollection points = new Point3DCollection();

      for (int yi = 0; yi <= vDiv; yi++)
      {
        double y = minY + yi * dy;

        for (int ti = 0; ti <= hDiv; ti++)
        {
          double t = ti * dt;
          double r = Math.Sqrt(1 - y * y);
          double x = r * Math.Cos(t);
          double z = r * Math.Sin(t);
          points.Add(new Point3D(x, y, z));
        }
      }

      return points;
    }

    private void AddCubeToMesh(MeshGeometry3D mesh, Point3D center, double size)
    {
      if (mesh != null)
      {
        int offset = mesh.Positions.Count;

        mesh.Positions.Add(new Point3D(center.X - size, center.Y + size, center.Z - size));
        mesh.Positions.Add(new Point3D(center.X + size, center.Y + size, center.Z - size));
        mesh.Positions.Add(new Point3D(center.X + size, center.Y + size, center.Z + size));
        mesh.Positions.Add(new Point3D(center.X - size, center.Y + size, center.Z + size));
        mesh.Positions.Add(new Point3D(center.X - size, center.Y - size, center.Z - size));
        mesh.Positions.Add(new Point3D(center.X + size, center.Y - size, center.Z - size));
        mesh.Positions.Add(new Point3D(center.X + size, center.Y - size, center.Z + size));
        mesh.Positions.Add(new Point3D(center.X - size, center.Y - size, center.Z + size));

        mesh.TriangleIndices.Add(offset + 3);
        mesh.TriangleIndices.Add(offset + 2);
        mesh.TriangleIndices.Add(offset + 6);

        mesh.TriangleIndices.Add(offset + 3);
        mesh.TriangleIndices.Add(offset + 6);
        mesh.TriangleIndices.Add(offset + 7);

        mesh.TriangleIndices.Add(offset + 2);
        mesh.TriangleIndices.Add(offset + 1);
        mesh.TriangleIndices.Add(offset + 5);

        mesh.TriangleIndices.Add(offset + 2);
        mesh.TriangleIndices.Add(offset + 5);
        mesh.TriangleIndices.Add(offset + 6);

        mesh.TriangleIndices.Add(offset + 1);
        mesh.TriangleIndices.Add(offset + 0);
        mesh.TriangleIndices.Add(offset + 4);

        mesh.TriangleIndices.Add(offset + 1);
        mesh.TriangleIndices.Add(offset + 4);
        mesh.TriangleIndices.Add(offset + 5);

        mesh.TriangleIndices.Add(offset + 0);
        mesh.TriangleIndices.Add(offset + 3);
        mesh.TriangleIndices.Add(offset + 7);

        mesh.TriangleIndices.Add(offset + 0);
        mesh.TriangleIndices.Add(offset + 7);
        mesh.TriangleIndices.Add(offset + 4);

        mesh.TriangleIndices.Add(offset + 7);
        mesh.TriangleIndices.Add(offset + 6);
        mesh.TriangleIndices.Add(offset + 5);

        mesh.TriangleIndices.Add(offset + 7);
        mesh.TriangleIndices.Add(offset + 5);
        mesh.TriangleIndices.Add(offset + 4);

        mesh.TriangleIndices.Add(offset + 2);
        mesh.TriangleIndices.Add(offset + 3);
        mesh.TriangleIndices.Add(offset + 0);

        mesh.TriangleIndices.Add(offset + 2);
        mesh.TriangleIndices.Add(offset + 0);
        mesh.TriangleIndices.Add(offset + 1);
      }
    }
    }
}
