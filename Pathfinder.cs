using System.Collections.Generic;

// Shitty SortedList-based priority queue
public class PriorityQueue<T1, T2> {

    private SortedList<T2, Queue<T1>> _data = new SortedList<T2, Queue<T1>>();
    private int _count = 0;

    public int Count => _count;

    public void Enqueue(T1 item, T2 key) {
        Queue<T1> entry;
        _data.TryGetValue(key, out entry);
        if (entry == null) {
            entry = new Queue<T1>();
            _data[key] = entry;
        }
        entry.Enqueue(item);
        _count++;
    }

    public T1 Dequeue() {
        if (_count > 0) {
            var key = _data.Keys[0];
            var entry = _data[key];
            T1 item = entry.Dequeue();
            _count--;
            if (entry.Count == 0) _data.Remove(key);
            return item;
        }

        return default(T1);
    }

    public void Clear() => _data.Clear();
}

public interface IPathfinder<T> {
    List<T> FindPath(T src, T dest);
}

public abstract class AStarPathfinder<T> : IPathfinder<T> {

    private Dictionary<T, float> visited = new Dictionary<T, float>();
    private Dictionary<T, T> cameFrom = new Dictionary<T, T>();
    private PriorityQueue<T, float> queue = new PriorityQueue<T, float>();
    private Dictionary<T, float> fScore = new Dictionary<T, float>();

    public IReadOnlyDictionary<T, float> Visited => visited; 

    protected float greed = 0.5f;

    public List<T> FindPath(T src, T dest) {

        visited.Clear();

        if (!Accessible(src, dest)) return null;

        visited.Add(src, 0);
        queue.Enqueue(src, 0);

        while (!visited.ContainsKey(dest) && queue.Count > 0) {
            T node = queue.Dequeue();
            float dist = visited[node];

            // Visit all neighbours
            foreach (T neighbour in GetNeighbours(node)) {
                var g = dist + GetDistance(node, neighbour); 
                if (visited.ContainsKey(neighbour) && visited[neighbour] <= g) continue;
                visited[neighbour] = g;
                cameFrom[neighbour] = node;
                fScore[neighbour] = (1 - greed) * g + greed * CalcHeuristic(neighbour, dest);
                queue.Enqueue(neighbour, fScore[neighbour]);
            }
        }

        var path = ReconstructPath(src, dest);

        cameFrom.Clear();
        fScore.Clear();
        queue.Clear();

        return path;
    }

    private List<T> ReconstructPath(T src, T dest) {
        List<T> path;
        if (visited.ContainsKey(dest)) {
            path = new List<T>();
            path.Add(dest);
            T node = dest;

            while (!node.Equals(src)) {
                node = cameFrom[node];
                path.Add(node);
            }

            path.Reverse();
        }
        else path = null;
        return path;
    }

    protected abstract bool Accessible(T src, T dest);
    protected abstract IEnumerable<T> GetNeighbours(T node);
    protected abstract float GetDistance(T src, T dest);
    protected abstract float CalcHeuristic(T src, T dest);
}

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

    protected override float GetDistance(TilePos src, TilePos dest) => src.Distance(dest);

    protected override IEnumerable<TilePos> GetNeighbours(TilePos pos) {
        foreach (var neighbour in pos.Neighbours) {
            if (!neighbour.WithinBounds(_map.Width, _map.Height)) continue;
            if (_map.GetBuilding(neighbour) != null) continue;
            yield return neighbour;
        }
    }
}
