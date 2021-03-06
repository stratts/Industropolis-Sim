using System.Collections.Generic;

// Less efficient pathfinder that follows paths along tiles, instead of nodes 
namespace Industropolis.Sim
{
    public class TilePathGraph : IGraph<IntVector>
    {
        Map _map;

        public TilePathGraph(Map map) => _map = map;

        public bool Accessible(IntVector src, IntVector dest) => true;

        public float CalculateCost(IntVector src, IntVector dest) => src.Distance(dest);

        public float EstimateCost(IntVector src, IntVector dest) => src.Distance(dest);

        public IEnumerable<IntVector> GetConnections(IntVector cameFrom, IntVector pos)
        {
            VehicleNode? n = _map.GetNode(pos);
            VehiclePath? p = _map.GetPath(pos);
            if (n != null)
            {
                foreach (var connection in n.Connections.Keys)
                {
                    if (_map.GetPath(cameFrom) is VehiclePath prevPath)
                    {
                        if (prevPath.Category != n.Connections[connection].Category) continue;
                    }
                    if (!n.HasPathTo(connection)) continue;
                    yield return pos + pos.Direction8(connection.Pos);
                }
            }
            if (p != null)
            {
                yield return pos + pos.Direction8(p.Source.Pos);
                yield return pos + pos.Direction8(p.Dest.Pos);
            }
        }
    }
}
