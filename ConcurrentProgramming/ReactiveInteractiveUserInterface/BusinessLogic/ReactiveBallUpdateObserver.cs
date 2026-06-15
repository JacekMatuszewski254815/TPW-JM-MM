/*
using System;
using System.Collections.Generic;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class ReactiveBallUpdateObserver : ReactiveTimeObserverBase
    {
        private readonly BusinessLogicImplementation _businessLogic;

        public ReactiveBallUpdateObserver(BusinessLogicImplementation businessLogic)
        {
            _businessLogic = businessLogic ?? throw new ArgumentNullException(nameof(businessLogic));
        }

        public override void OnNext(TimingData value)
        {
            if (!IsActive || value == null)
                return;

            try
            {
                foreach (var ball in _businessLogic.GetBalls())
                {
                    var dataBall = ((BusinessBall)ball).DataBallReference;
                    dataBall.UpdatePosition(value.DeltaTime);
                }

                base.OnNext(value);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
    }
}
*/
