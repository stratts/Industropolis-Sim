
namespace Industropolis.Sim
{
    public class BuildingNode : VehicleNode
    {
        public Building Building { get; }

        public override PathCategory Category { get; }

        public BuildingNode(IntVector pos, Building building, PathCategory category) : base(pos)
        {
            Building = building;
            Category = category;
        }
    }
}
