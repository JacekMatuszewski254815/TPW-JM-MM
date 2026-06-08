using System;

namespace TP.ConcurrentProgramming.Data
{
    /// <summary>
    /// Dane czasowe emitowane przez TimerService.
    /// Zawiera informacje o upływie czasu w bieżącej klatce.
    /// </summary>
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

        public override string ToString()
        {
            return $"Frame {FrameNumber}: ΔT={DeltaTime:F4}s, Total={TotalElapsedTime:F2}s";
        }
    }

    /// <summary>
    /// Interfejs dla reaktywnej obserwacji zdarzeń czasowych.
    /// Umożliwia subskrybowanie na zdarzenia czasu rzeczywistego.
    /// 
    /// PROGRAMOWANIE REAKTYWNE:
    /// - Observer pattern: klienci subskrybują na zdarzenia zamiast pytać o dane
    /// - Push model: serwis wysyła dane do obserwatorów
    /// - Asynchroniczny flow: zdarzenia są emitowane bez blokowania
    /// 
    /// VS INTERAKTYWNE:
    /// - Interaktywne: użytkownik/kod pyta o dane (pull model)
    /// - Reaktywne: system emituje dane do zainteresowanych obserwatorów (push model)
    /// </summary>
    public interface IReactiveTimeObserver : IObserver<TimingData>
    {
        /// <summary>
        /// Zdarzenie wywoływane za każdym razem gdy upłynie delta time.
        /// </summary>
        event EventHandler<TimingData>? OnTimeTick;

        /// <summary>
        /// Czy obserwator jest aktywny i nasłuchuje zdarzeń.
        /// </summary>
        bool IsActive { get; }
    }

    /// <summary>
    /// Reactive observer pattern adapter.
    /// Implementuje IObserver do pracy z IObservable źródłem czasu.
    /// </summary>
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
