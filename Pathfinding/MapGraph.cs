using System;
using System.Collections.Generic;


public class MapGraph : IGraph<TilePos>
{
    private MapInfo _map;

    public MapGraph(MapInfo map) {
        _map = map;
    }

    public bool Accessible(TilePos src, TilePos dest) {
        if (_map.GetBuilding(dest) != null) return false;
        return true;
    }

    public float EstimateCost(TilePos src, TilePos dest) => src.Distance(dest);

    public float CalculateCost(TilePos src, TilePos dest) {
        TilePos diff = dest - src;
        if (Math.Abs(diff.X) == Math.Abs(diff.Y)) return 1.4f;
        else return 1;
    }

    public IEnumerable<TilePos> GetConnections(TilePos pos) {
        foreach (var neighbour in pos.Neighbours) {
            if (!neighbour.WithinBounds(_map.Width, _map.Height)) continue;
            if (_map.GetBuilding(neighbour) != null) continue;
            yield return neighbour;
        }
    }
}
