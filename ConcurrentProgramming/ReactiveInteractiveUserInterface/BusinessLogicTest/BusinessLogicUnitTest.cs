using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                public void UpdatePosition(double deltaTime) 
                { 
                    Position = new Data.Vector(
                        Position.x + Velocity.x * deltaTime,
                        Position.y + Velocity.y * deltaTime
                    );
                }
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



        [TestMethod]
        public async Task Logger_ShouldCreateHeaderAndFormatDataCorrectly_WithAscii()
        {
            // Arrange
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"ball_diagnostics_test_{Guid.NewGuid()}.log");

            try
            {
                // Act
                using (var logger = new BallDiagnosticsLogger(tempFilePath, bufferSize: 5))
                {
                    logger.LogBallState(1, 10.12345, 20.67891, 1.5, -2.5, 5.0, 10.0);
                    await logger.FlushAsync();
                } // <--- TUTAJ logger się disposuje, zamyka wątek tła i całkowicie zwalnia plik

                // Assert
                Assert.IsTrue(File.Exists(tempFilePath), "Plik logu nie został utworzony.");

                // Bezpieczny odczyt – nikt już nie blokuje pliku
                string[] lines = await File.ReadAllLinesAsync(tempFilePath, Encoding.ASCII);

                Assert.IsTrue(lines.Length >= 2, $"Plik powinien zawierać co najmniej 2 linie, a ma: {lines.Length}");

                string expectedHeader = "Timestamp|BallId|PositionX|PositionY|VelocityX|VelocityY|Mass|Radius";
                Assert.AreEqual(expectedHeader, lines[0], "Nagłówek pliku logu jest nieprawidłowy.");

                string[] parts = lines[1].Split('|');
                Assert.AreEqual(8, parts.Length, "Linia danych nie zawiera 8 kolumn rozdzielonych znakiem '|'.");
                Assert.AreEqual("1", parts[1], "Nieprawidłowe BallId.");
                Assert.AreEqual("10.1235", parts[2], "Nieprawidłowy format PositionX (oczekiwano F4).");
                Assert.AreEqual("20.6789", parts[3], "Nieprawidłowy format PositionY (oczekiwano F4).");
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { }
                }
            }
        }

        [TestMethod]
        public async Task Logger_ShouldNotLog_WhenDisabled()
        {
            // Arrange
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"ball_diagnostics_disabled_test_{Guid.NewGuid()}.log");

            try
            {
                // Act
                using (var logger = new BallDiagnosticsLogger(tempFilePath, bufferSize: 10))
                {
                    logger.IsEnabled = false; // Wyłączamy logger
                    logger.LogBallState(1, 10, 10, 1, 1, 5, 10);
                    await logger.FlushAsync();
                } // <--- Zamknięcie pliku

                // Assert
                string[] lines = await File.ReadAllLinesAsync(tempFilePath, Encoding.ASCII);
                Assert.AreEqual(1, lines.Length, "Logger zapisał dane pomimo wyłączenia (IsEnabled = false).");
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    try { File.Delete(tempFilePath); } catch { }
                }
            }
        }

    }
}