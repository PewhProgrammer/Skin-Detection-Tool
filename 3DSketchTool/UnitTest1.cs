using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using _3DReconstructionWPF.Computation;

namespace _3DSketchTool
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            OneEuroFilter filter = new OneEuroFilter();
            for (int i = 0; i < 20; i++)
            {
                filter.ComputeFilteredValue(1);
            }

            Assert.AreEqual(1, filter.ComputeFilteredValue(1), "They should be eqaul");

            var testValue = filter.ComputeFilteredValue(20);
            Assert.AreEqual(testValue, 20, "nicht so nice");
        }
    }
}
