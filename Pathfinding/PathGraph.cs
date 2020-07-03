

using System.Collections.Generic;

namespace Industropolis.Sim
{
    public class PathGraph<T> : IGraph<T> where T : IPathNode<T>
    {
        public bool Accessible(T src, T dest)
        {
            return true;
        }

        public float CalculateCost(T src, T dest)
        {
            return src.GetCostTo(dest) + 1; // Penalty for intersections
        }

        public float EstimateCost(T src, T dest)
        {
            return src.Pos.Distance(dest.Pos);
        }

        public IEnumerable<T> GetConnections(T node)
        {
            foreach (var n in node.Nodes)
            {
                if (node.HasPathTo(n)) yield return n;
            }
        }
    }
}
