using System.Collections.Generic;

// Less efficient pathfinder that follows paths along tiles, instead of nodes 
public class TilePathGraph : IGraph<IntVector>
{
    Map _map;

    public TilePathGraph(Map map) => _map = map;

    public bool Accessible(IntVector src, IntVector dest) => true;

    public float CalculateCost(IntVector src, IntVector dest) => src.Distance(dest);

    public float EstimateCost(IntVector src, IntVector dest) => src.Distance(dest);

    public IEnumerable<IntVector> GetConnections(IntVector pos)
    {
        PathNode? n = _map.GetNode(pos);
        Path? p = _map.GetPath(pos);
        if (n != null)
        {
            foreach (var connection in n.Connections.Keys)
            {
                yield return pos + pos.Direction(connection.Pos);
            }
        }
        if (p != null)
        {
            yield return pos + pos.Direction(p.Source.Pos);
            yield return pos + pos.Direction(p.Dest.Pos);
        }
    }
}