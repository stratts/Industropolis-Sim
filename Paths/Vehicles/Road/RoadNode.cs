
namespace Industropolis.Sim
{
    public class RoadNode : VehicleNode
    {

        public RoadNode(IntVector pos) : base(pos)
        {
        }

        public override PathCategory Category => PathCategory.Road;
    }
}