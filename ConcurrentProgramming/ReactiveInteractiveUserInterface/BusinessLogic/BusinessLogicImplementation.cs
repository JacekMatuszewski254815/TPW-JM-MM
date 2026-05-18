using System;
using System.Collections.Generic;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        private readonly DataAbstractAPI _dataApi;
        private readonly List<IBall> _businessBalls = new List<IBall>();
        private readonly object _collisionLock = new object(); //sekcja krytyczna

        private int _boardWidth;
        private int _boardHeight;

        public BusinessLogicImplementation(DataAbstractAPI dataApi)
        {
            _dataApi = dataApi;
        }

        public override void Start(int width, int height, int ballsCount)
        {
            _boardWidth = width;
            _boardHeight = height;

            _dataApi.CreateBoard(width, height, ballsCount);
            foreach (var dataBall in _dataApi.GetBalls())
            {
                var bBall = new BusinessBall(dataBall);
                _businessBalls.Add(bBall);

                dataBall.NewPositionNotification += (sender, position) => OnBallMoved((BusinessBall)bBall);
            }
        }

        private void OnBallMoved(BusinessBall currentBall)
        {
            lock (_collisionLock)
            {
                CheckWallCollisions(currentBall);
                CheckBallCollisions(currentBall);
            }
        }

        private void CheckWallCollisions(BusinessBall ball)
        {
            double newX = ball.DataBallReference.Position.x;
            double newY = ball.DataBallReference.Position.y;
            double vX = ball.DataBallReference.Velocity.x;
            double vY = ball.DataBallReference.Velocity.y;
            double radius = ball.Radius;

            if (newX - radius <= 0) { newX = radius; vX = -vX; }
            else if (newX + radius >= _boardWidth) { newX = _boardWidth - radius; vX = -vX; }

            if (newY - radius <= 0) { newY = radius; vY = -vY; }
            else if (newY + radius >= _boardHeight) { newY = _boardHeight - radius; vY = -vY; }

            ball.DataBallReference.Velocity = new Data.Vector(vX, vY);
            if (newX != ball.DataBallReference.Position.x || newY != ball.DataBallReference.Position.y)
            {
                ball.DataBallReference.Position = new Data.Vector(newX, newY);
            }
        }

        private void CheckBallCollisions(BusinessBall currentBall)
        {
            foreach (BusinessBall otherBall in _businessBalls)
            {
                if (otherBall == currentBall) continue;

                double deltaX = currentBall.DataBallReference.Position.x - otherBall.DataBallReference.Position.x;
                double deltaY = currentBall.DataBallReference.Position.y - otherBall.DataBallReference.Position.y;
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                if (distance <= (currentBall.Radius + otherBall.Radius))
                {
                    double m1 = currentBall.DataBallReference.Mass;
                    double m2 = otherBall.DataBallReference.Mass;

                    double v1x = currentBall.DataBallReference.Velocity.x;
                    double v1y = currentBall.DataBallReference.Velocity.y;
                    double v2x = otherBall.DataBallReference.Velocity.x;
                    double v2y = otherBall.DataBallReference.Velocity.y;

                    //wzory na zderzenie sprężyste
                    double newV1x = ((m1 - m2) * v1x + 2 * m2 * v2x) / (m1 + m2);
                    double newV1y = ((m1 - m2) * v1y + 2 * m2 * v2y) / (m1 + m2);
                    double newV2x = ((2 * m1) * v1x + (m2 - m1) * v2x) / (m1 + m2);
                    double newV2y = ((2 * m1) * v1y + (m2 - m1) * v2y) / (m1 + m2);

                    currentBall.DataBallReference.Velocity = new Data.Vector(newV1x, newV1y);
                    otherBall.DataBallReference.Velocity = new Data.Vector(newV2x, newV2y);

                    //zapobiegnięcie overlapowi
                    double overlap = (currentBall.Radius + otherBall.Radius) - distance;
                    double moveX = (deltaX / distance) * (overlap / 2);
                    double moveY = (deltaY / distance) * (overlap / 2);

                    currentBall.DataBallReference.Position = new Data.Vector(currentBall.DataBallReference.Position.x + moveX, currentBall.DataBallReference.Position.y + moveY);
                    otherBall.DataBallReference.Position = new Data.Vector(otherBall.DataBallReference.Position.x - moveX, otherBall.DataBallReference.Position.y - moveY);
                }
            }
        }

        public override IEnumerable<IBall> GetBalls() => _businessBalls;

        public override void Dispose()
        {
            foreach (var ball in _businessBalls) ball.Dispose();
            _businessBalls.Clear();
            _dataApi.Dispose();
        }
    }
}