using System;
using System.ComponentModel;

namespace TP.ConcurrentProgramming.Presentation.Model
{
    public interface IBall : INotifyPropertyChanged
    {
        double Top { get; }
        double Left { get; }
        double Diameter { get; }
    }

    public abstract class ModelAbstractApi : IObservable<IBall>, IDisposable
    {
        public static ModelAbstractApi CreateModel()
        {
            return modelInstance.Value;
        }

        public abstract void Start(int numberOfBalls);
        public abstract IDisposable Subscribe(IObserver<IBall> observer);
        public abstract void Dispose();

        private static readonly Lazy<ModelAbstractApi> modelInstance = new Lazy<ModelAbstractApi>(() => new PresentationModel());
    }
}