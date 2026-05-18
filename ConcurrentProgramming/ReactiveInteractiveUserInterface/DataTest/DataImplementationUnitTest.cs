using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataImplementationUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                IEnumerable<IBall>? ballsList = newInstance.GetBalls();
                Assert.IsNotNull(ballsList);
                int numberOfBalls = 0;
                foreach (var ball in ballsList) { numberOfBalls++; }
                Assert.AreEqual<int>(0, numberOfBalls);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            DataImplementation newInstance = new DataImplementation();
            newInstance.CreateBoard(600, 400, 5);

            newInstance.Dispose();

            IEnumerable<IBall>? ballsList = newInstance.GetBalls();
            int count = 0;
            foreach (var ball in ballsList) { count++; }
            Assert.AreEqual<int>(0, count);
        }

        [TestMethod]
        public void StartTestMethod()
        {
            using (DataImplementation newInstance = new DataImplementation())
            {
                int numberOfBalls2Create = 10;
                newInstance.CreateBoard(600, 400, numberOfBalls2Create);

                IEnumerable<IBall>? ballsList = newInstance.GetBalls();
                int actualCount = 0;
                foreach (var ball in ballsList)
                {
                    actualCount++;
                    Assert.IsTrue(ball.Position.x >= 0);
                    Assert.IsTrue(ball.Position.y >= 0);
                    Assert.IsNotNull(ball);
                }
                Assert.AreEqual<int>(numberOfBalls2Create, actualCount);
            }
        }
    }
}