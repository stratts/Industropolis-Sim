using System;
using System.Collections.Generic;


namespace Industropolis.Sim
{
    public class MapGraph : IGraph<IntVector>
    {
        private Map _map;

        public MapGraph(Map map)
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

        public IEnumerable<IntVector> GetConnections(IntVector cameFrom, IntVector pos)
        {
            foreach (var neighbour in pos.Neighbours)
            {
                if (!_map.ValidPos(neighbour)) continue;
                if (_map.GetBuilding(neighbour) != null) continue;
                yield return neighbour;
            }
        }
    }
}
