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
    }

    internal class Ball : IBall
    {
        private bool _isRunning = true;
        private Task _movementTask;
        private readonly object _lockObject = new object();
        private IVector _position;
        private IVector _velocity;

        internal Ball(Vector initialPosition, Vector initialVelocity, double mass, double radius)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            Mass = mass;
            Radius = radius;

            _movementTask = Task.Run(MoveLoop);
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

        private async Task MoveLoop()
        {
            while (_isRunning)
            {
                lock (_lockObject)
                {
                    _position = new Vector(_position.x + _velocity.x, _position.y + _velocity.y);
                }

                NewPositionNotification?.Invoke(this, Position);

                await Task.Delay(16);
            }
        }

        public void Dispose()
        {
            _isRunning = false;
            try { _movementTask.Wait(); } catch { }
        }
    }
}