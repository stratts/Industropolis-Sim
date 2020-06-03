using System;
using System.Collections.Generic;

public class AStarPathfinder<T> : IPathfinder<T>
{

    private Dictionary<T, NodeData> visited = new Dictionary<T, NodeData>();
    private PriorityQueue<T, float> queue = new PriorityQueue<T, float>();

    private float greed = 0.5f;

    private class NodeData
    {
        public T CameFrom;
        public float Dist;

        public NodeData(T cameFrom, float dist)
        {
            CameFrom = cameFrom;
            Dist = dist;
        }
    }

    public AStarPathfinder(float greediness = 0.5f)
    {
        greed = greediness;
    }

    public List<T> FindPath(IGraph<T> graph, T src, T dest)
    {

        if (!graph.Accessible(src, dest)) return null;

        visited.Add(src, new NodeData(src, 0));
        queue.Enqueue(src, 0);

        while (!visited.ContainsKey(dest) && queue.Count > 0)
        {
            T node = queue.Dequeue();
            float dist = visited[node].Dist;

            // Visit all neighbours
            foreach (T neighbour in graph.GetConnections(node))
            {
                var g = dist + graph.CalculateCost(node, neighbour);
                visited.TryGetValue(neighbour, out NodeData v);
                if (v == null) visited[neighbour] = new NodeData(node, g);
                else if (v.Dist <= g) continue;
                else
                {
                    v.Dist = g;
                    v.CameFrom = node;
                }
                var f = (1 - greed) * g + greed * graph.EstimateCost(neighbour, dest);
                queue.Enqueue(neighbour, f);
            }
        }

        var path = ReconstructPath(src, dest);

        queue.Clear();
        visited.Clear();

        return path;
    }

    private List<T> ReconstructPath(T src, T dest)
    {
        List<T> path;
        if (visited.ContainsKey(dest))
        {
            path = new List<T>();
            path.Add(dest);
            T node = dest;

            while (!node.Equals(src))
            {
                node = visited[node].CameFrom;
                path.Add(node);
            }

            path.Reverse();
        }
        else path = null;
        return path;
    }
}