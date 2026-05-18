namespace TP.ConcurrentProgramming.BusinessLogic
{
    public interface IPosition
    {
        double x { get; }
        double y { get; }
    }

    public class Position : IPosition
    {
        public double x { get; }
        public double y { get; }

        public Position(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}