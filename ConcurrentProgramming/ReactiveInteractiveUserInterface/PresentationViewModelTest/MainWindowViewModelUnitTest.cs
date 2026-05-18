using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TP.ConcurrentProgramming.Presentation.Model;

namespace TP.ConcurrentProgramming.Presentation.ViewModel.Test
{
    public class MainWindowViewModel : IDisposable
    {
        private readonly ModelAbstractApi _model;
        private IDisposable? _subscription;
        public ObservableCollection<IBall> Balls { get; } = new ObservableCollection<IBall>();

        public MainWindowViewModel(ModelAbstractApi model)
        {
            _model = model;
            _subscription = _model.Subscribe(new BallObserver(Balls));
        }

        public void Start(int numberOfBalls)
        {
            _model.Start(numberOfBalls);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            Balls.Clear();
        }

        private class BallObserver : IObserver<IBall>
        {
            private readonly ObservableCollection<IBall> _balls;
            public BallObserver(ObservableCollection<IBall> balls) { _balls = balls; }
            public void OnCompleted() { }
            public void OnError(Exception error) { }
            public void OnNext(IBall value) { _balls.Add(value); }
        }
    }

    [TestClass]
    public class MainWindowViewModelUnitTest
    {
        [TestMethod]
        public void ConstructorAndStartTest()
        {
            ModelFixture fakeModel = new();
            using (MainWindowViewModel viewModel = new(fakeModel))
            {
                Assert.IsNotNull(viewModel.Balls);
                viewModel.Start(5);
                Assert.AreEqual(5, viewModel.Balls.Count);
            }
        }

        private class ModelFixture : ModelAbstractApi
        {
            private IObserver<IBall>? _observer;

            public override void Start(int numberOfBalls)
            {
                for (int i = 0; i < numberOfBalls; i++)
                {
                    _observer?.OnNext(new FakeModelBall());
                }
            }

            public override IDisposable Subscribe(IObserver<IBall> observer)
            {
                _observer = observer;
                return new NullDisposable();
            }

            public override void Dispose() { }

            private class NullDisposable : IDisposable
            {
                public void Dispose() { }
            }

            private class FakeModelBall : IBall
            {
                public double Top => 0.0;
                public double Left => 0.0;
                public double Diameter => 20.0;
                public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
            }
        }

    }
}