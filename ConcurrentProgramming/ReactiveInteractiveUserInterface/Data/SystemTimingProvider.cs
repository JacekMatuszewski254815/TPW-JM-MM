using System;

namespace TP.ConcurrentProgramming.Data
{

    public class SystemTimingProvider : ITimingProvider
    {
        private double _lastDeltaTime = 0;
        private long _frameCount = 0;
        private double _totalFrameTime = 0;
        private bool _isDisposed = false;
        private bool _isRunning = false;
        private readonly object _lockObject = new object();

        private const double IntervalMs = 16; //60fps

        public SystemTimingProvider()
        {
            _timer = new System.Timers.Timer(IntervalMs) { AutoReset = true, Enabled = false };
            _timer.Elapsed += OnTimerElapsed;
        }


        public double AverageFramesPerSecond
        {
            get
            {
                lock (_lockObject)
                {
                    return _frameCount > 0 ? _frameCount / _totalFrameTime : 0;
                }
            }
        }

        public void Start()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SystemTimingProvider));

            {
            }
        }

        public double GetDeltaTime()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SystemTimingProvider));

                return 0;

            double currentTime = _stopwatch.Elapsed.TotalSeconds;
            _lastDeltaTime = currentTime - _lastMeasurementTime;
            _lastMeasurementTime = currentTime;

            _frameCount++;
            _totalFrameTime += _lastDeltaTime;

            if (_lastDeltaTime > 1.0 / 30.0)
                _lastDeltaTime = 1.0 / 30.0;

            return _lastDeltaTime;
        }
        }

        public void Dispose()
        {
            {
                _isDisposed = true;
            }
        }
    }
}
