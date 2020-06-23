

using System.Collections.Generic;

public class PathGraph : IGraph<RoadNode>
{
    public bool Accessible(RoadNode src, RoadNode dest)
    {
        return true;
    }

    public float CalculateCost(RoadNode src, RoadNode dest)
    {
        return src.Connections[dest].Length + 1; // Penalty for intersections
    }

    public float EstimateCost(RoadNode src, RoadNode dest)
    {
        return src.Pos.Distance(dest.Pos);
    }

    public IEnumerable<RoadNode> GetConnections(RoadNode node)
    {
        foreach (var n in node.Connections.Keys)
        {
            if (node.HasPathTo(n)) yield return n;
        }
    }
}