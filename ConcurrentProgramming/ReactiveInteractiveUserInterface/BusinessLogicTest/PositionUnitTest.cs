using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class PositionUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            Random random = new Random();
            double initialX = random.NextDouble();
            double initialY = random.NextDouble();
            IPosition position = new Position(initialX, initialY);
            Assert.AreEqual<double>(initialX, position.x);
            Assert.AreEqual<double>(initialY, position.y);
        }
    }
}