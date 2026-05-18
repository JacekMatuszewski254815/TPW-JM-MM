using Microsoft.VisualStudio.TestTools.UnitTesting;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class BallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Vector testingVector = new Vector(10.0, 20.0);

            using (Ball newInstance = new(testingVector, testingVector, 5.0, 10.0))
            {
                Assert.IsNotNull(newInstance);
                Assert.AreEqual(5.0, newInstance.Mass);
                Assert.AreEqual(10.0, newInstance.Radius);
            }
        }
    }
}