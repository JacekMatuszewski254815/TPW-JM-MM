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

        [TestMethod]
        public async Task RealTime_TimerService_EmitRegularTicks()
        {
            var timingProvider = new SystemTimingProvider();
            using (var timerService = new TimerService(timingProvider))
            {
                var receivedTicks = new List<TimingData>();
                var completionSource = new TaskCompletionSource<bool>();

                var observer = new AnonymousObserver<TimingData>(
                    onNext: data =>
                    {
                        lock (receivedTicks)
                        {
                            receivedTicks.Add(data);
                            if (receivedTicks.Count >= 5)
                            {
                                completionSource.TrySetResult(true);
                            }
                        }
                    },
                    onError: ex => completionSource.TrySetException(ex),
                    onCompleted: () => completionSource.TrySetResult(true)
                );

                using (timerService.Subscribe(observer))
                {
                    timingProvider.Start();
                    await Task.Delay(50);

                    timerService.Start();

                    var delayTask = Task.Delay(1000);
                    var completedTask = await Task.WhenAny(completionSource.Task, delayTask);

                    timerService.Stop();

                    Assert.AreSame(completionSource.Task, completedTask, "Wątek czasu rzeczywistego nie wyemitował klatek w oczekiwanym czasie (Timeout).");
                }

                lock (receivedTicks)
                {
                    Assert.IsTrue(receivedTicks.Count >= 5, $"Zarejestrowano zbyt mało klatek: {receivedTicks.Count}");

                    for (int i = 0; i < receivedTicks.Count; i++)
                    {
                        var tick = receivedTicks[i];

                        Assert.IsTrue(tick.FrameNumber >= 0, $"Nieprawidłowy numer klatki: {tick.FrameNumber}");

                        Assert.IsTrue(tick.DeltaTime >= 0, $"DeltaTime nie może być ujemna. Otrzymano: {tick.DeltaTime}");
                        Assert.IsTrue(tick.DeltaTime <= 1.0 / 30.0, $"DeltaTime przekroczyła limit 1/30s. Otrzymano: {tick.DeltaTime}");
                    }
                }
            }
        }

        private class AnonymousObserver<T> : IObserver<T>
        {
            private readonly Action<T> _onNext;
            private readonly Action<Exception> _onError;
            private readonly Action _onCompleted;

            public AnonymousObserver(Action<T> onNext, Action<Exception> onError, Action onCompleted)
            {
                _onNext = onNext;
                _onError = onError;
                _onCompleted = onCompleted;
            }

            public void OnNext(T value) => _onNext(value);
            public void OnError(Exception error) => _onError(error);
            public void OnCompleted() => _onCompleted();
        }
    }
}