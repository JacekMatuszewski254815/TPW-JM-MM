using System;
using System.Threading.Tasks;

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
        private bool _isRunning = true;
        private Task _movementTask;
        private readonly object _lockObject = new object();
        private IVector _position;
        private IVector _velocity;
        private ITimerService _timerService;
        private IDisposable _timerSubscription;

        internal Ball(Vector initialPosition, Vector initialVelocity, double mass, double radius)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            Mass = mass;
            Radius = radius;

            _timerService = new TimerService();
        }

        internal Ball(Vector initialPosition, Vector initialVelocity, double mass, double radius, ITimerService timerService)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            Mass = mass;
            Radius = radius;
            _timerService = timerService ?? new TimerService();
        }

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

        public void UpdatePosition(double deltaTime)
        {
            if (deltaTime <= 0)
                return;

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
            _isRunning = false;
            _timerSubscription?.Dispose();
            _timerService?.Dispose();
            try { _movementTask?.Wait(); } catch { }
        }
    }
}
