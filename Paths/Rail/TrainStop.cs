
namespace Industropolis.Sim
{
    public class TrainStop : RailNode
    {
        public Building Building { get; }

        public TrainStop(IntVector pos, Building building) : base(pos)
        {
            Building = building;
        }
    }
}