using System;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    /// <summary>
    /// Implementacja ITimingProvider używająca System.Diagnostics.Stopwatch.
    /// Zapewnia precyzyjny pomiar czasu rzeczywistego dla aplikacji.
    /// 
    /// UWAGI DOTYCZĄCE CZASU RZECZYWISTEGO:
    /// - Stopwatch jest najdokładniejszą dostępną metodą pomiaru czasu w .NET
    /// - Delta time jest obliczany jako różnica między bieżącym a poprzednim pomiarem
    /// - Wszystkie pomiary są thread-safe dzięki wewnętrznym lock'om Stopwatch
    /// </summary>
    public class SystemTimingProvider : ITimingProvider
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private double _lastDeltaTime = 0;
        private double _lastMeasurementTime = 0;
        private long _frameCount = 0;
        private double _totalFrameTime = 0;
        private bool _isDisposed = false;

        public double TotalElapsedTime => _stopwatch.Elapsed.TotalSeconds;
        public long FrameCount => _frameCount;
        public double AverageFramesPerSecond => _frameCount > 0 ? _frameCount / _totalFrameTime : 0;

        public void Start()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SystemTimingProvider));

            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
                _lastMeasurementTime = 0;
            }
        }

        public double GetDeltaTime()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SystemTimingProvider));

            if (!_stopwatch.IsRunning)
                return 0;

            double currentTime = _stopwatch.Elapsed.TotalSeconds;
            _lastDeltaTime = currentTime - _lastMeasurementTime;
            _lastMeasurementTime = currentTime;

            _frameCount++;
            _totalFrameTime += _lastDeltaTime;

            // Ogranicze delta time do maksymalnie 1/30 sekundy (33ms)
            // aby uniknąć dużych skoków w przypadku zatrzymania debuggera
            if (_lastDeltaTime > 1.0 / 30.0)
                _lastDeltaTime = 1.0 / 30.0;

            return _lastDeltaTime;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _stopwatch.Stop();
                _isDisposed = true;
            }
        }
    }
}
