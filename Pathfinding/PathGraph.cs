

using System.Collections.Generic;

public class PathGraph : IGraph<PathNode>
{
    public bool Accessible(PathNode src, PathNode dest)
    {
        return true;
    }

    public float CalculateCost(PathNode src, PathNode dest)
    {
        return src.Connections[dest].Length + 1; // Penalty for intersections
    }

    public float EstimateCost(PathNode src, PathNode dest)
    {
        return src.Pos.Distance(dest.Pos);
    }

    public IEnumerable<PathNode> GetConnections(PathNode node)
    {
        return node.Connections.Keys;
    }
}