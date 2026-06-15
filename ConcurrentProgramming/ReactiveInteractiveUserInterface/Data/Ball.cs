using System;

namespace TP.ConcurrentProgramming.Data
{
    public interface IBall : IDisposable
    {
        event EventHandler<IVector>? NewPositionNotification;
        IVector Position { get; set; }
        IVector Velocity { get; set; }
        double Mass { get; }
        double Radius { get; }
        void UpdatePosition(double deltaTime);
    }

    internal class Ball : IBall
    {
        private readonly object _lockObject = new object();
        private IVector _position;
        private IVector _velocity;

        private readonly ITimerService _timerService;
        private readonly IDisposable _timerSubscription;

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Position
        {
            get { lock (_lockObject) return _position; }
            set { lock (_lockObject) _position = value; }
        }

        public IVector Velocity
        {
            get { lock (_lockObject) return _velocity; }
            set { lock (_lockObject) _velocity = value; }
        }

        public double Mass { get; init; }
        public double Radius { get; init; }

        internal Ball(Vector initialPosition, Vector initialVelocity, double mass, double radius)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            Mass = mass;
            Radius = radius;

            _timerService = new TimerService();
            _timerSubscription = _timerService.Subscribe(new BallTimeObserver(this));
            _timerService.Start();
        }

        public void UpdatePosition(double deltaTime)
        {
            if (deltaTime <= 0) return;

            lock (_lockObject)
            {
                double newX = _position.x + (_velocity.x * deltaTime);
                double newY = _position.y + (_velocity.y * deltaTime);
                _position = new Vector(newX, newY);
            }

            NewPositionNotification?.Invoke(this, Position);
        }

        public void Dispose()
        {
            _timerSubscription?.Dispose();
            _timerService?.Dispose();
        }
        private class BallTimeObserver : IObserver<TimingData>
        {
            private readonly Ball _ball;
            public BallTimeObserver(Ball ball) => _ball = ball;

            public void OnNext(TimingData value)
            {
                _ball.UpdatePosition(value.DeltaTime);
            }
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }
    }
}