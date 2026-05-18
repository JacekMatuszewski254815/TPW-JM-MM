using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            var fakeData = new SimpleDataMock();
            using (BusinessLogicImplementation newInstance = new(fakeData))
            {
                Assert.IsNotNull(newInstance);
            }
        }

        [TestMethod]
        public void DisposeTestMethod()
        {
            var fakeData = new SimpleDataMock();
            BusinessLogicImplementation newInstance = new(fakeData);

            newInstance.Dispose();

            Assert.IsTrue(fakeData.DisposedCalled);
        }

        [TestMethod]
        public void StartTestMethod()
        {
            var fakeData = new SimpleDataMock();
            using (BusinessLogicImplementation newInstance = new(fakeData))
            {
                int numberOfBalls2Create = 5;
                newInstance.Start(600, 400, numberOfBalls2Create);

                Assert.IsTrue(fakeData.CreateBoardCalled);
                Assert.AreEqual(numberOfBalls2Create, fakeData.BallsCountRequested);
            }
        }

        [TestMethod]
        public void BallCollision()
        {
            var fakeData = new CollisionDataMock();
            using (var logic = new BusinessLogicImplementation(fakeData))
            {
                logic.Start(100, 100, 2);

                var balls = new List<BusinessBall>();
                foreach (var b in logic.GetBalls()) { balls.Add((BusinessBall)b); }

                //symulowanie overlapu
                balls[0].DataBallReference.Position = new Data.Vector(50.0, 50.0);
                balls[0].DataBallReference.Velocity = new Data.Vector(2.0, 0.0);

                balls[1].DataBallReference.Position = new Data.Vector(55.0, 50.0);
                balls[1].DataBallReference.Velocity = new Data.Vector(-2.0, 0.0);

                fakeData.FirstBall.RaiseNewPosition();

                Assert.IsTrue(balls[0].DataBallReference.Velocity.x < 0, "Kulka 1 powinna odbić się i zmienić zwrot prędkości na ujemny.");
                Assert.IsTrue(balls[1].DataBallReference.Velocity.x > 0, "Kulka 2 powinna odbić się i zmienić zwrot prędkości na dodatni.");
            }
        }


        private class CollisionDataMock : Data.DataAbstractAPI
        {
            public FakeCollisionBall FirstBall { get; } = new FakeCollisionBall();
            public FakeCollisionBall SecondBall { get; } = new FakeCollisionBall();

            public override void CreateBoard(int w, int h, int count) { }
            public override IEnumerable<Data.IBall> GetBalls() => new List<Data.IBall> { FirstBall, SecondBall };
            public override void Dispose() { }

            public class FakeCollisionBall : Data.IBall
            {
                public Data.IVector Position { get; set; } = new Data.Vector(0, 0);
                public Data.IVector Velocity { get; set; } = new Data.Vector(0, 0);
                public double Mass => 5.0;
                public double Radius => 10.0;
                public event EventHandler<Data.IVector>? NewPositionNotification;
                public void RaiseNewPosition() => NewPositionNotification?.Invoke(this, Position);
                public void Dispose() { }
            }
        }


        private class SimpleDataMock : Data.DataAbstractAPI
        {
            public bool DisposedCalled { get; private set; } = false;
            public bool CreateBoardCalled { get; private set; } = false;
            public int BallsCountRequested { get; private set; } = 0;

            public override void CreateBoard(int width, int height, int ballsCount)
            {
                CreateBoardCalled = true;
                BallsCountRequested = ballsCount;
            }

            public override IEnumerable<Data.IBall> GetBalls() => new List<Data.IBall>();

            public override void Dispose()
            {
                DisposedCalled = true;
            }
        }

    }
}