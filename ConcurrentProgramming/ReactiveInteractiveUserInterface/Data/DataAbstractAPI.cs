using System;
using System.Collections.Generic;

namespace TP.ConcurrentProgramming.Data
{
    public abstract class DataAbstractAPI : IDisposable
    {
        public abstract void CreateBoard(int width, int height, int ballsCount);
        public abstract IEnumerable<IBall> GetBalls();
        public abstract void Dispose();

        public static DataAbstractAPI CreateAPI()
        {
            return new DataImplementation();
        }
    }
}