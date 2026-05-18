using System;
using System.Collections.Generic;
using TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.Presentation.Model
{
    internal class PresentationModel : ModelAbstractApi
    {
        private readonly BusinessLogicAbstractAPI _layerBelow;
        private readonly List<IObserver<IBall>> _observers = new List<IObserver<IBall>>();
        private readonly List<IBall> _modelBalls = new List<IBall>();

        public PresentationModel()
        {
            _layerBelow = BusinessLogicAbstractAPI.CreateAPI();
        }

        public override void Start(int numberOfBalls)
        {

            _layerBelow.Start(395, 400, numberOfBalls);

            foreach (var logicBall in _layerBelow.GetBalls())
            {
                var modelBall = new ModelBall(logicBall.Position.x, logicBall.Position.y, logicBall);
                _modelBalls.Add(modelBall);

                foreach (var observer in _observers)
                {
                    observer.OnNext(modelBall);
                }
            }
        }

        public override IDisposable Subscribe(IObserver<IBall> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);

                foreach (var ball in _modelBalls)
                {
                    observer.OnNext(ball);
                }
            }
            return new Unsubscriber(_observers, observer);
        }

        public override void Dispose()
        {
            _layerBelow.Dispose();
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
            _observers.Clear();
            _modelBalls.Clear();
        }

        #region Unsubscriber
        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<IBall>> _observers;
            private readonly IObserver<IBall> _observer;

            public Unsubscriber(List<IObserver<IBall>> observers, IObserver<IBall> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
        #endregion
    }
}