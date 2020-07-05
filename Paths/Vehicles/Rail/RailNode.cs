
namespace Industropolis.Sim
{
    public class RailNode : VehicleNode
    {

        public RailNode(IntVector pos) : base(pos)
        {
        }

        public override PathCategory Category => PathCategory.Rail;
    }
}