using System;
using System.Collections.Generic;

public static class PathUtils
{
    public static (TPath, TPath) Split<TPath, TNode>(TPath path, TNode node)
        where TPath : BasePath<TNode> where TNode : IPathNode
    {
        var path1 = (TPath)Activator.CreateInstance(path.GetType(), path.Source, node);
        var path2 = (TPath)Activator.CreateInstance(path.GetType(), node, path.Dest);
        path.OnPathSplit();
        return (path1, path2);
    }

    public static TPath Merge<TPath, TNode>(TPath path1, TPath path2)
        where TPath : BasePath<TNode> where TNode : IPathNode
    {
        var nodes1 = new HashSet<TNode>(new[] { path1.Source, path1.Dest });
        var nodes2 = new HashSet<TNode>(new[] { path2.Source, path2.Dest });

        nodes1.SymmetricExceptWith(nodes2);
        var ends = new List<TNode>(nodes1);

        var path = (TPath)Activator.CreateInstance(path1.GetType(), ends[0], ends[1]);
        return path;
    }
}

public abstract class BasePath<T> : MapObject where T : IPathNode
{
    public abstract PathCategory Category { get; }
    public abstract PathType PathType { get; }

    public T Source { get; private set; }
    public T Dest { get; private set; }
    public float Length { get; private set; }
    public IntVector Direction { get; private set; }

    public event Action? PathSplit;

    public BasePath(T source, T dest)
    {
        Source = source;
        Dest = dest;
        Length = Source.Pos.Distance(Dest.Pos);
        Direction = Source.Pos.Direction(Dest.Pos);
    }

    public bool OnPath(IntVector pos)
    {
        if (pos == Source.Pos || pos == Dest.Pos) return true;
        if (Source.Pos.FloatDirection(pos) == Dest.Pos.FloatDirection(pos).Negate()) return true;
        return false;
    }

    public void OnPathSplit() => PathSplit?.Invoke();
}

public abstract class Path : BasePath<PathNode>
{
    public IReadOnlyCollection<PathLane> Lanes => _lanes;
    protected List<PathLane> _lanes;

    public Path(PathNode source, PathNode dest) : base(source, dest)
    {
        _lanes = new List<PathLane>();
    }

    public void Connect()
    {
        _lanes.Clear();
        AddLanes();
        Source.Connect(Dest, this);
        Dest.Connect(Source, this);
    }

    public void Disconnect()
    {
        Source.Disconnect(Dest);
        Dest.Disconnect(Source);
        _lanes.Clear();
    }

    protected virtual void AddLanes()
    {
        _lanes.Add(new PathLane(this, LaneDir.Source));
        _lanes.Add(new PathLane(this, LaneDir.Dest));
    }

    public bool HasLaneTo(PathNode node)
    {
        try
        {
            GetLaneTo(node);
        }
        catch (ArgumentException)
        {
            return false;
        }
        return true;
    }

    public PathLane GetLaneTo(PathNode node)
    {
        foreach (var lane in _lanes)
        {
            if (lane.Dest == node) return lane;
        }

        throw new ArgumentException("Path contains no lane to that node");
    }

    public PathLane GetLaneFrom(PathNode node)
    {
        foreach (var lane in _lanes)
        {
            if (lane.Source == node) return lane;
        }

        throw new ArgumentException("Path contains no lane to that node");
    }
}

public class Road : Path
{
    public Road(PathNode source, PathNode dest) : base(source, dest) { }

    public override PathCategory Category => PathCategory.Road;

    public override PathType PathType => PathType.Road;
}

public class OneWayRoad : Road
{
    public OneWayRoad(PathNode source, PathNode dest) : base(source, dest) { }

    public override PathType PathType => PathType.OneWayRoad;

    protected override void AddLanes()
    {
        _lanes.Add(new PathLane(this, LaneDir.Dest));
        _lanes.Add(new PathLane(this, LaneDir.Dest));
    }
}

public class Rail : Path
{
    public Rail(PathNode source, PathNode dest) : base(source, dest) { }

    public override PathCategory Category => PathCategory.Rail;

    public override PathType PathType => PathType.Rail;
}


public class PathLane
{
    private List<Vehicle> _queue = new List<Vehicle>();

    public Path Parent { get; private set; }
    public LaneDir Direction { get; private set; }
    public PathNode Source => Direction == LaneDir.Dest ? Parent.Source : Parent.Dest;
    public PathNode Dest => Direction == LaneDir.Dest ? Parent.Dest : Parent.Source;

    public PathLane(Path parent, LaneDir direction)
    {
        Parent = parent;
        Direction = direction;
    }

    public bool AtCapacity => _queue.Count >= Parent.Length;

    public void Enter(Vehicle vehicle)
    {
        _queue.Add(vehicle);
    }

    public void Depart(Vehicle vehicle)
    {
        _queue.Remove(vehicle);
    }

    public Vehicle? GetVehicleAhead(Vehicle vehicle)
    {
        for (int i = 1; i < _queue.Count; i++)
        {
            if (_queue[i] == vehicle) return _queue[i - 1];
        }
        return null;
    }
}

public enum LaneDir
{
    Source,
    Dest
}
