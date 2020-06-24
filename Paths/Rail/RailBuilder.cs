

public class RailBuilder : PathBuilder<RailNode, Rail>
{
    public RailBuilder(Map map) : base(map.Rails)
    {
    }

    public override RailNode MakeNode(IntVector pos, PathCategory category) => new RailNode(pos, category);

    public override Rail MakePath(PathType type, RailNode source, RailNode dest)
    {
        switch (type)
        {
            case PathType.Rail: return new Rail(source, dest);
            default: return new Rail(source, dest);
        }
    }
}