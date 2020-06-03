using System;
using System.Collections.Generic;


public class MapGraph : IGraph<IntVector>
{
    private MapInfo _map;

    public MapGraph(MapInfo map)
    {
        _map = map;
    }

    public bool Accessible(IntVector src, IntVector dest)
    {
        if (_map.GetBuilding(dest) != null) return false;
        return true;
    }

    public float EstimateCost(IntVector src, IntVector dest) => src.Distance(dest);

    public float CalculateCost(IntVector src, IntVector dest)
    {
        IntVector diff = dest - src;
        if (Math.Abs(diff.X) == Math.Abs(diff.Y)) return 1.4f;
        else return 1;
    }

    public IEnumerable<IntVector> GetConnections(IntVector pos)
    {
        foreach (var neighbour in pos.Neighbours)
        {
            if (!neighbour.WithinBounds(_map.Width, _map.Height)) continue;
            if (_map.GetBuilding(neighbour) != null) continue;
            yield return neighbour;
        }
    }
}
