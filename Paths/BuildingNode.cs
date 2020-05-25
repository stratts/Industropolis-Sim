
public class BuildingNode : PathNode {
    public Building Building { get; }

    public BuildingNode(IntVector pos, Building building) : base(pos) {
        Building = building;
    }
}