using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TP.ConcurrentProgramming.BusinessLogic;
using LogicIBall = TP.ConcurrentProgramming.BusinessLogic.IBall;

namespace TP.ConcurrentProgramming.Presentation.Model
{
    internal class ModelBall : IBall
    {
        private const double BallRadius = 10.0;
        private double _topBackingField;
        private double _leftBackingField;

        public ModelBall(double left, double top, LogicIBall underneathBall)
        {
            _leftBackingField = left - BallRadius;
            _topBackingField = top - BallRadius;
            underneathBall.NewPositionNotification += NewPositionNotification;
        }

        public double Top
        {
            get => _topBackingField;
            private set
            {
                if (_topBackingField == value) return;
                _topBackingField = value;
                RaisePropertyChanged();
            }
        }

        public double Left
        {
            get => _leftBackingField;
            private set
            {
                if (_leftBackingField == value) return;
                _leftBackingField = value;
                RaisePropertyChanged();
            }
        }

        public double Diameter { get; init; } = 20.0;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NewPositionNotification(object sender, IPosition e)
        {
            Top = e.y - BallRadius;
            Left = e.x - BallRadius;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}