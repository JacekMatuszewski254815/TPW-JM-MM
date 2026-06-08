using System;

namespace TP.ConcurrentProgramming.Data
{
    /// <summary>
    /// Abstrakcja do pomiaru czasu rzeczywistego w aplikacjach.
    /// Umożliwia pomiaru upływu czasu (delta time) między klatkami.
    /// 
    /// PROGRAMOWANIE CZASU RZECZYWISTEGO:
    /// - Czas rzeczywisty = pomiar rzeczywistego upływu czasu w systemie
    /// - Delta time = różnica czasu między ostatnią a bieżącą klatką
    /// - Użycie: new_position = current_position + velocity * delta_time
    /// </summary>
    public interface ITimingProvider : IDisposable
    {
        /// <summary>
        /// Rejestruje początek pomiaru czasu.
        /// Należy wywołać na początku pętli animacji.
        /// </summary>
        void Start();

        /// <summary>
        /// Zwraca czas, który upłynął od ostatniego wywołania GetDeltaTime().
        /// </summary>
        /// <returns>Delta time w sekundach</returns>
        double GetDeltaTime();

        /// <summary>
        /// Całkowity czas, jaki upłynął od Start().
        /// </summary>
        double TotalElapsedTime { get; }

        /// <summary>
        /// Liczba klatek, które upłynęły.
        /// </summary>
        long FrameCount { get; }

        /// <summary>
        /// Średnia liczba klatek na sekundę.
        /// </summary>
        double AverageFramesPerSecond { get; }
    }
}
