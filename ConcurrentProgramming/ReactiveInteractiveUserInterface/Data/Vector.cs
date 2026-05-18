namespace TP.ConcurrentProgramming.Data
{
    public interface IVector
    {
        double x { get; }
        double y { get; }
    }

    public class Vector : IVector
    {
        public double x { get; }
        public double y { get; }

        public Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}