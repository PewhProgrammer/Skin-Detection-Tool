using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using _3DReconstructionWPF.Computation;
using System.Windows.Media.Media3D;

namespace _3DSketchTool
{
    [TestClass]
    public class OneEuroFilterTests
    {
        [TestMethod]
        public void TestIteration20_1()
        {
            OneEuroFilter filter = new OneEuroFilter(1,0);
            for (int i = 0; i < 20; i++)
            {
                filter.Filter(1, 1);
            }

            Assert.AreEqual(1, filter.Filter(1, 1), "They should be eqaul");
            var testValue = filter.Filter(20, 1);
            Assert.IsTrue(testValue > 1 && testValue < 20, "Value should be in-between 1 and 20");
        }

        [TestMethod]
        public void TestIteration20_20()
        {
            OneEuroFilter filter = new OneEuroFilter(1, 0);
            for (int i = 0; i < 20; i++)
            {
                filter.Filter(1, 20);
            }

            Assert.AreEqual(1, filter.Filter(1, 20), "They should be eqaul");
            var testValue = filter.Filter(20, 20);
            Assert.IsTrue(testValue > 1 && testValue < 20, "Value should be in-between 1 and 20");
        }

        [TestMethod]
        public void TestPointIteration20_20()
        {
            float value = 0.5f;
            float scale = 0.1f;
            var p = new Point3D(value,value,value);
            var rand = new Random();

            OneEuroFilter filterX = new OneEuroFilter(1, 0,true);
            for (int i = 0; i < 20; i++)
            {
                // distort x-coordinate
                var q = p;

                q.X = value + rand.NextDouble() * Math.Pow(-1,i) * scale;
                q.Y = value + rand.NextDouble() * Math.Pow(-1, i) * scale;
                q.Z = value + rand.NextDouble() * Math.Pow(-1, i) * scale;
                filterX.Filter(q, 20);
            }

            //var val = filterX.Filter(new Point3D(value,value,value), 20);
            var testValue = new Point3D(value, value, value);
            p = filterX.Filter(p, 20); 
            var err = ComputeRMSE(p, testValue);

            Console.WriteLine("Filtered Point: " + p);
            Console.WriteLine("Error: " + err);

            Assert.IsTrue(err < 0.03f, "error deviation was too high");
        }

        private double ComputeRMSE(Point3D A, Point3D B)
        {

            var error = B - A;
            var err = Vector3D.DotProduct(error, error);

            return err;
        }
    }
}
