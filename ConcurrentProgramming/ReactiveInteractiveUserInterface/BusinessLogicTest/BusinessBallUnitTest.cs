using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessBallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            DataBallFixture dataBallFixture = new DataBallFixture();
            BusinessBall newInstance = new(dataBallFixture);
            int numberOfCallBackCalled = 0;

            newInstance.NewPositionNotification += (sender, position) => {
                Assert.IsNotNull(sender);
                Assert.IsNotNull(position);
                numberOfCallBackCalled++;
            };

            dataBallFixture.Move();
            Assert.AreEqual<int>(1, numberOfCallBackCalled);
        }


        private class DataBallFixture : Data.IBall
        {
            public Data.IVector Position
            {
                get => new VectorFixture(0.0, 0.0);
                set => throw new NotImplementedException();
            }

            public Data.IVector Velocity
            {
                get => new VectorFixture(0.0, 0.0);
                set => throw new NotImplementedException();
            }

            public double Mass => 5.0;
            public double Radius => 10.0;

            public event EventHandler<Data.IVector>? NewPositionNotification;

            public void UpdatePosition(double deltaTime) 
            { 
                // Mock implementation
            }

            internal void Move()
            {
                NewPositionNotification?.Invoke(this, new VectorFixture(0.0, 0.0));
            }

            public void Dispose() { }
        }

        private class VectorFixture : Data.IVector
        {
            internal VectorFixture(double X, double Y)
            {
                x = X; y = Y;
            }

            public double x { get; init; }
            public double y { get; init; }
        }

    }
}