using System;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    public interface IBall : IDisposable
    {
        event EventHandler<IPosition>? NewPositionNotification;
        IPosition Position { get; }
        double Radius { get; }
    }

    internal class BusinessBall : IBall
    {
        internal Data.IBall DataBallReference { get; }

        public BusinessBall(Data.IBall dataBall)
        {
            DataBallReference = dataBall;
            DataBallReference.NewPositionNotification += OnDataBallPositionChanged;
        }

        public event EventHandler<IPosition>? NewPositionNotification;

        public IPosition Position => new Position(DataBallReference.Position.x, DataBallReference.Position.y);
        public double Radius => DataBallReference.Radius;

        private void OnDataBallPositionChanged(object? sender, IVector e)
        {
            NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
        }

        public void Dispose()
        {
            DataBallReference.NewPositionNotification -= OnDataBallPositionChanged;
        }
    }
}