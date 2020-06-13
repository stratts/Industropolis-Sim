using System;
using System.Collections.Generic;


public abstract class Path : MapObject
{
    public abstract PathCategory Category { get; }

    public PathNode Source { get; private set; }
    public PathNode Dest { get; private set; }
    public float Length { get; private set; }
    public IntVector Direction { get; private set; }

    public IReadOnlyCollection<PathLane> Lanes => _lanes;
    private List<PathLane> _lanes;

    public event Action? PathSplit;

    public Path(PathNode source, PathNode dest)
    {
        _lanes = new List<PathLane>();
        Source = source;
        Dest = dest;
        SetNodes(source, dest);
    }

    public void SetNodes(PathNode source, PathNode dest)
    {
        Source = source;
        Dest = dest;
        Length = Source.Pos.Distance(Dest.Pos);
        Direction = Source.Pos.Direction(Dest.Pos);
    }

    public void Connect()
    {
        _lanes.Clear();
        _lanes.Add(new PathLane(this, LaneDir.Source));
        _lanes.Add(new PathLane(this, LaneDir.Dest));
        Source.Connect(Dest, this);
        Dest.Connect(Source, this);
    }

    public void Disconnect()
    {
        Source.Disconnect(Dest);
        Dest.Disconnect(Source);
        _lanes.Clear();
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

    // Split path at given node, and return new paths 
    public static (Path, Path) Split(Path path, PathNode node)
    {
        var path1 = (Path)Activator.CreateInstance(path.GetType(), path.Source, node);
        var path2 = (Path)Activator.CreateInstance(path.GetType(), node, path.Dest);
        path.Disconnect();
        path.PathSplit?.Invoke();
        return (path1, path2);
    }

    // Merge two paths into one, return new path
    public static Path Merge(Path path1, Path path2)
    {
        PathNode start;
        PathNode end;

        // Todo: handle different path types
        if (path1.Source == path2.Source)
        {
            start = path1.Dest;
            end = path2.Dest;
        }
        else if (path1.Dest == path2.Dest)
        {
            start = path1.Source;
            end = path2.Source;
        }
        else if (path1.Dest == path2.Source)
        {
            start = path1.Source;
            end = path2.Dest;
        }
        else
        {
            start = path2.Source;
            end = path1.Dest;
        }
        var path = (Path)Activator.CreateInstance(path1.GetType(), start, end);
        return path;
    }

    public bool OnPath(IntVector pos)
    {
        if (pos == Source.Pos || pos == Dest.Pos) return true;
        if (Source.Pos.FloatDirection(pos) == Dest.Pos.FloatDirection(pos).Negate()) return true;
        return false;
    }
}

public class Road : Path
{
    public Road(PathNode source, PathNode dest) : base(source, dest) { }

    public override PathCategory Category => PathCategory.Road;
}

public class Rail : Path
{
    public Rail(PathNode source, PathNode dest) : base(source, dest) { }

    public override PathCategory Category => PathCategory.Rail;
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
