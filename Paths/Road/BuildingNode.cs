
public class BuildingNode : RoadNode
{
    public Building Building { get; }

    public BuildingNode(IntVector pos, Building building) : base(pos, PathCategory.Road)
    {
        Building = building;
    }
}