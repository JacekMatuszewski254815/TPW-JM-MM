using System;

namespace TP.ConcurrentProgramming.Data
{

    public interface ITimingProvider : IDisposable
    {
        void Start();

        double GetDeltaTime();

        double TotalElapsedTime { get; }

        long FrameCount { get; }

        double AverageFramesPerSecond { get; }
    }
}
