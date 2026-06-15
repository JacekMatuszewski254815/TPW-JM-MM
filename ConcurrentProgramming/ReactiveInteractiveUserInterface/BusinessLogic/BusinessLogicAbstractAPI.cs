using System;
using System.Collections.Generic;
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    public abstract class BusinessLogicAbstractAPI : IDisposable
    {
        public abstract void Start(int width, int height, int ballsCount);
        public abstract IEnumerable<IBall> GetBalls();
        public abstract void Dispose();
        public abstract void EnableDiagnostics(string filePath = null, int bufferSize = 100);
        public abstract void DisableDiagnostics();

        public static BusinessLogicAbstractAPI CreateAPI(DataAbstractAPI? dataApi = null)
        {
            return new BusinessLogicImplementation(dataApi ?? DataAbstractAPI.CreateAPI());
        }
    }
}