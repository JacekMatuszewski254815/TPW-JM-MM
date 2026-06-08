using System;
using System.Collections.Generic;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    /// <summary>
    /// Reaktywny obserwator dla aktualizacji pozycji kul na podstawie czasu.
    /// 
    /// PROGRAMOWANIE REAKTYWNE:
    /// - Obserwuje zdarzenia czasowe z TimerService
    /// - Reaguje na każde zdarzenie (TimingData) poprzez updateowanie pozycji kul
    /// - Push model: serwis wysyła delta time, kule reagują automatycznie
    /// 
    /// VS SEKWENCYJNE:
    /// - Sekwencyjne: pętla główna pyta "czy czas się zmienił?" (pull)
    /// - Reaktywne: czas emituje zdarzenia do zainteresowanych (push)
    /// </summary>
    internal class ReactiveBallUpdateObserver : ReactiveTimeObserverBase
    {
        private readonly BusinessLogicImplementation _businessLogic;

        public ReactiveBallUpdateObserver(BusinessLogicImplementation businessLogic)
        {
            _businessLogic = businessLogic ?? throw new ArgumentNullException(nameof(businessLogic));
        }

        /// <summary>
        /// Wywoływane za każdym razem gdy upłynie delta time.
        /// Aktualizuje pozycję wszystkich kul z uwzględnieniem czasu rzeczywistego.
        /// </summary>
        public override void OnNext(TimingData value)
        {
            if (!IsActive || value == null)
                return;

            try
            {
                // Aktualizuj pozycje wszystkich kul z uwzględnieniem delta time
                // Wzór: new_position = current_position + velocity * delta_time
                foreach (var ball in _businessLogic.GetBalls())
                {
                    var dataBall = ((BusinessBall)ball).DataBallReference;
                    dataBall.UpdatePosition(value.DeltaTime);
                }

                // Notyfikuj obserwatorów poprzez zdarzenie (jeśli ktoś subskrybuje)
                base.OnNext(value);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }
}
