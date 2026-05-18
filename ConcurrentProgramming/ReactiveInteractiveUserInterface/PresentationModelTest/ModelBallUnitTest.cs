using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;

namespace TP.ConcurrentProgramming.Presentation.Model.Test
{
    [TestClass]
    public class ModelBallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            ModelBall ball = new ModelBall(10.0, 10.0, new BusinessLogicIBallFixture());
            Assert.AreEqual<double>(0.0, ball.Top);
            Assert.AreEqual<double>(0.0, ball.Left);
        }

        [TestMethod]
        public void PositionChangeNotificationTestMethod()
        {
            int notificationCounter = 0;
            BusinessLogicIBallFixture logicBall = new BusinessLogicIBallFixture();
            ModelBall ball = new ModelBall(10.0, 10.0, logicBall);

            ball.PropertyChanged += (sender, args) => {
                notificationCounter++;
            };

            Assert.AreEqual(0, notificationCounter);

            logicBall.RaiseMove(50.0, 60.0);

            Assert.IsTrue(notificationCounter > 0);
            Assert.AreEqual<double>(40.0, ball.Left); // 50.0 - Radius (10.0)
            Assert.AreEqual<double>(50.0, ball.Top);  // 60.0 - Radius (10.0)
        }

        private class BusinessLogicIBallFixture : BusinessLogic.IBall
        {
            public event EventHandler<BusinessLogic.IPosition>? NewPositionNotification;

            public BusinessLogic.IPosition Position { get; set; } = new PositionFixture(10.0, 10.0);
            public double Radius => 10.0;

            public void RaiseMove(double x, double y)
            {
                Position = new PositionFixture(x, y);
                NewPositionNotification?.Invoke(this, Position);
            }

            public void Dispose() { }

            private class PositionFixture : BusinessLogic.IPosition
            {
                public PositionFixture(double x, double y) { this.x = x; this.y = y; }
                public double x { get; }
                public double y { get; }
            }
        }

    }
}