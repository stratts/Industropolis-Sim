using System;
using System.Collections.Generic;


public class TilePathfinder : AStarPathfinder<TilePos>
{
    private MapInfo _map;

    public TilePathfinder(MapInfo map) {
        _map = map;
        greed = 0.6f;
    }

    protected override bool Accessible(TilePos src, TilePos dest) {
        if (_map.GetBuilding(dest) != null) return false;
        return true;
    }

    protected override float CalcHeuristic(TilePos src, TilePos dest) => src.Distance(dest);

    protected override float GetNeighbourDistance(TilePos src, TilePos dest) {
        TilePos diff = dest - src;
        if (Math.Abs(diff.X) == Math.Abs(diff.Y)) return 1.4f;
        else return 1;
    }

    protected override IEnumerable<TilePos> GetNeighbours(TilePos pos) {
        foreach (var neighbour in pos.Neighbours) {
            if (!neighbour.WithinBounds(_map.Width, _map.Height)) continue;
            if (_map.GetBuilding(neighbour) != null) continue;
            yield return neighbour;
        }
    }
}
