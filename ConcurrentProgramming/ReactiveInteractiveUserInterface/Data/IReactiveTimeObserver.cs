using System;

namespace TP.ConcurrentProgramming.Data
{

    public class TimingData
    {
        public double DeltaTime { get; set; }
        public long FrameNumber { get; set; }
        public double TotalElapsedTime { get; set; }
        public DateTime Timestamp { get; set; }

        public TimingData(double deltaTime, long frameNumber, double totalElapsedTime)
        {
            DeltaTime = deltaTime;
            FrameNumber = frameNumber;
            TotalElapsedTime = totalElapsedTime;
            Timestamp = DateTime.UtcNow;
        }

    }

    public interface IReactiveTimeObserver : IObserver<TimingData>
    {
        event EventHandler<TimingData>? OnTimeTick;

        bool IsActive { get; }
    }

    public abstract class ReactiveTimeObserverBase : IReactiveTimeObserver
    {
        public event EventHandler<TimingData>? OnTimeTick;
        public bool IsActive { get; protected set; } = true;

        public virtual void OnNext(TimingData value)
        {
            if (IsActive)
            {
                OnTimeTick?.Invoke(this, value);
            }
        }

        public virtual void OnError(Exception error)
        {
            IsActive = false;
        }

        public virtual void OnCompleted()
        {
            IsActive = false;
        }
    }
}
