using System;
using System.Collections.Generic;

public abstract class Road : Path<RoadNode>
{
    public IReadOnlyCollection<RoadLane> Lanes => _lanes;
    protected List<RoadLane> _lanes;

    public override PathCategory Category => PathCategory.Road;

    public Road(RoadNode source, RoadNode dest) : base(source, dest)
    {
        _lanes = new List<RoadLane>();
        AddLanes();
    }

    protected abstract void AddLanes();

    public bool HasLaneTo(RoadNode node)
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

    public RoadLane GetLaneTo(RoadNode node)
    {
        foreach (var lane in _lanes)
        {
            if (lane.Dest == node) return lane;
        }

        throw new ArgumentException("Path contains no lane to that node");
    }

    public RoadLane GetLaneFrom(RoadNode node)
    {
        foreach (var lane in _lanes)
        {
            if (lane.Source == node) return lane;
        }

        throw new ArgumentException("Path contains no lane to that node");
    }
}

public class SimpleRoad : Road
{
    public SimpleRoad(RoadNode source, RoadNode dest) : base(source, dest) { }

    public override PathType PathType => PathType.OneWayRoad;

    protected override void AddLanes()
    {
        _lanes.Add(new RoadLane(this, LaneDir.Source));
        _lanes.Add(new RoadLane(this, LaneDir.Dest));
    }
}

public class OneWayRoad : Road
{
    public OneWayRoad(RoadNode source, RoadNode dest) : base(source, dest) { }

    public override PathType PathType => PathType.OneWayRoad;

    protected override void AddLanes()
    {
        _lanes.Add(new RoadLane(this, LaneDir.Dest));
        _lanes.Add(new RoadLane(this, LaneDir.Dest));
    }
}

public class RoadLane
{
    private List<Vehicle> _queue = new List<Vehicle>();

    public Road Parent { get; private set; }
    public LaneDir Direction { get; private set; }
    public RoadNode Source => Direction == LaneDir.Dest ? Parent.Source : Parent.Dest;
    public RoadNode Dest => Direction == LaneDir.Dest ? Parent.Dest : Parent.Source;

    public RoadLane(Road parent, LaneDir direction)
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