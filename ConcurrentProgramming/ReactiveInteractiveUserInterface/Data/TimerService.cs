using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    /// <summary>
    /// Reaktywny serwis czasu.
    /// Emituje zdarzenia czasowe do wszystkich zarejestrowanych obserwatorów.
    /// 
    /// PROGRAMOWANIE REAKTYWNE + WSPÓŁBIEŻNE:
    /// - Observable pattern: Many-to-One (jeden serwis, wielu obserwatorów)
    /// - Thread-safe: sekcje krytyczne chronią listę obserwatorów
    /// - Asynchroniczny flow: emitowanie zdarzeń w tle
    /// 
    /// SEKCJE KRYTYCZNE:
    /// - _observersLock: chronizes access to _observers list
    /// - Zapobiega race conditions gdy obserwatorzy się dołączają/odłączają
    /// </summary>
    public interface ITimerService : IObservable<TimingData>, IDisposable
    {
        /// <summary>
        /// Uruchamia serwis czasu. Zaczyna emitować zdarzenia czasowe.
        /// </summary>
        void Start();

        /// <summary>
        /// Zatrzymuje serwis czasu.
        /// </summary>
        void Stop();

        /// <summary>
        /// Liczba aktualnie zarejestrowanych obserwatorów.
        /// </summary>
        int ObserverCount { get; }
    }

    public class TimerService : ITimerService
    {
        private readonly ITimingProvider _timingProvider;
        private readonly List<IObserver<TimingData>> _observers = new List<IObserver<TimingData>>();
        private readonly object _observersLock = new object(); // SEKCJA KRYTYCZNA
        private Task _emitTask;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;
        private bool _isDisposed = false;
        private long _frameNumber = 0;

        /// <summary>
        /// Interwał między emitowaniem zdarzeń (w ms).
        /// Domyślnie: 16ms ≈ 60 FPS
        /// </summary>
        private const int EmitIntervalMs = 16;

        public int ObserverCount
        {
            get
            {
                lock (_observersLock)
                {
                    return _observers.Count;
                }
            }
        }

        public TimerService(ITimingProvider timingProvider = null)
        {
            _timingProvider = timingProvider ?? new SystemTimingProvider();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(TimerService));

            if (_isRunning)
                return;

            _isRunning = true;
            _timingProvider.Start();
            _emitTask = EmitTimingEventsAsync(_cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _isRunning = false;
        }

        private async Task EmitTimingEventsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (_isRunning && !cancellationToken.IsCancellationRequested)
                {
                    double deltaTime = _timingProvider.GetDeltaTime();
                    var timingData = new TimingData(
                        deltaTime,
                        _frameNumber++,
                        _timingProvider.TotalElapsedTime
                    );

                    // Emituj do wszystkich obserwatorów w sekcji krytycznej
                    List<IObserver<TimingData>> observersCopy;
                    lock (_observersLock)
                    {
                        observersCopy = new List<IObserver<TimingData>>(_observers);
                    }

                    foreach (var observer in observersCopy)
                    {
                        try
                        {
                            observer.OnNext(timingData);
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                        }
                    }

                    await Task.Delay(EmitIntervalMs, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Oczekiwane przy zamykaniu
            }
            finally
            {
                NotifyAllCompleted();
            }
        }

        public IDisposable Subscribe(IObserver<TimingData> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            lock (_observersLock)
            {
                _observers.Add(observer);
            }

            return new Unsubscriber(this, observer);
        }

        private void Unsubscribe(IObserver<TimingData> observer)
        {
            lock (_observersLock)
            {
                _observers.Remove(observer);
            }
        }

        private void NotifyAllCompleted()
        {
            List<IObserver<TimingData>> observersCopy;
            lock (_observersLock)
            {
                observersCopy = new List<IObserver<TimingData>>(_observers);
            }

            foreach (var observer in observersCopy)
            {
                observer.OnCompleted();
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _isRunning = false;

            try
            {
                _cancellationTokenSource?.Cancel();
                _emitTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during TimerService disposal: {ex.Message}");
            }
            finally
            {
                lock (_observersLock)
                {
                    _observers.Clear();
                }
                _cancellationTokenSource?.Dispose();
                _timingProvider?.Dispose();
            }
        }

        #region Unsubscriber
        private class Unsubscriber : IDisposable
        {
            private readonly TimerService _timerService;
            private readonly IObserver<TimingData> _observer;

            public Unsubscriber(TimerService timerService, IObserver<TimingData> observer)
            {
                _timerService = timerService;
                _observer = observer;
            }

            public void Dispose()
            {
                _timerService.Unsubscribe(_observer);
            }
        }
        #endregion
    }
}
