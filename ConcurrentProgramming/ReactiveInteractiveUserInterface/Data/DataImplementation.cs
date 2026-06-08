using System;
using System.Collections.Generic;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        private readonly List<IBall> _balls = new List<IBall>();
        private readonly Random _random = new Random();

        public override void CreateBoard(int width, int height, int ballsCount)
        {
            double radius = 10.0;
            double mass = 5.0;

            const double velocityScale = 60.0;

            for (int i = 0; i < ballsCount; i++)
            {
                double x = _random.NextDouble() * (width - 2 * radius) + radius;
                double y = _random.NextDouble() * (height - 2 * radius) + radius;

                double vx = (_random.NextDouble() * 2 - 1) * 3 * velocityScale;
                double vy = (_random.NextDouble() * 2 - 1) * 3 * velocityScale;

                _balls.Add(new Ball(new Vector(x, y), new Vector(vx, vy), mass, radius));
            }
        }

        public override IEnumerable<IBall> GetBalls()
        {
            return _balls;
        }

        public override void Dispose()
        {
            foreach (var ball in _balls)
            {
                ball.Dispose();
            }
            _balls.Clear();
        }
    }
}