using System;
using System.Timers;

namespace TP.ConcurrentProgramming.Data
{
    public class SystemTimingProvider : ITimingProvider
    {
        private readonly System.Timers.Timer _timer;
        private double _lastDeltaTime = 0;
        private double _elapsedTimeSeconds = 0;
        private long _frameCount = 0;
        private double _totalFrameTime = 0;
        private bool _isDisposed = false;
        private bool _isRunning = false;
        private readonly object _lockObject = new object();

        private const double IntervalMs = 16; // ~60 FPS

        public SystemTimingProvider()
        {
            _timer = new System.Timers.Timer(IntervalMs) { AutoReset = true, Enabled = false };
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            lock (_lockObject)
            {
                var delta = IntervalMs / 1000.0;
                _lastDeltaTime = delta;
                _elapsedTimeSeconds += delta;
                _frameCount++;
                _totalFrameTime += delta;

                if (_lastDeltaTime > 1.0 / 30.0)
                    _lastDeltaTime = 1.0 / 30.0;
            }
        }

        public double TotalElapsedTime
        {
            get
            {
                lock (_lockObject)
                {
                    return _elapsedTimeSeconds;
                }
            }
        }

        public long FrameCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _frameCount;
                }
            }
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

            lock (_lockObject)
            {
                if (_isRunning)
                    return;

                _isRunning = true;
                _timer.Start();
            }
        }

        public double GetDeltaTime()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SystemTimingProvider));

            lock (_lockObject)
            {
                if (!_isRunning)
                    return 0;

                return _lastDeltaTime;
            }
        }

        public void Dispose()
        {
            lock (_lockObject)
            {
                if (_isDisposed)
                    return;

                _timer.Elapsed -= OnTimerElapsed;
                _timer.Stop();
                _timer.Dispose();
                _isRunning = false;
                _isDisposed = true;
            }
        }
    }
}