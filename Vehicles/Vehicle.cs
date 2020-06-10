using System;

public abstract class Vehicle
{
    private float _speed = 1; // Tiles per second
    private Route.Direction _direction;

    public Route Route { get; }
    public Path CurrentPath { get; private set; }
    public PathLane CurrentLane { get; private set; }
    public float PathPos { get; private set; }
    public PathNode PrevNode { get; private set; }
    public PathNode NextNode { get; private set; }
    public PathNode Destination { get; private set; }

    protected Action _action;
    protected float _elapsedTime;

    public Vehicle(Route route)
    {
        Route = route;
        SetDirection(Route.Direction.Forwards);
        GoNext();
    }

    public void Update(float elapsedTime)
    {
        _elapsedTime = elapsedTime;
        _action();
    }

    protected void FollowPath()
    {
        if (PathPos < CurrentPath.Length - 1) Move();
        else if (CanGoNext())
        {
            _action = CrossIntersection;
            NextNode.Occupied = true;
        }
    }

    protected void CrossIntersection()
    {
        if (PathPos < CurrentPath.Length) Move();
        else if (NextNode != Destination)
        {
            NextNode.Occupied = false;
            GoNext();
        }
        else
        {
            NextNode.Occupied = false;
            DestinationReached();
        }
    }

    protected void Move()
    {
        if (CurrentLane.GetVehicleAhead(this) is Vehicle v && v.PathPos - PathPos < 1) return;
        PathPos += _speed * _elapsedTime;
    }

    protected void SetDirection(Route.Direction direction)
    {
        _direction = direction;

        switch (direction)
        {
            case Route.Direction.Forwards:
                Destination = Route.Dest;
                PrevNode = Route.Source;
                NextNode = Route.Source;
                break;
            case Route.Direction.Backwards:
                Destination = Route.Source;
                PrevNode = Route.Dest;
                NextNode = Route.Dest;
                break;
        }
    }

    protected bool CanGoNext() => NextNode.CanProceed(PrevNode, Route.Next(NextNode, _direction));

    protected void GoNext()
    {
        if (CurrentLane != null) CurrentLane.Depart(this);
        var current = NextNode;
        PrevNode = current;
        NextNode = Route.Next(current, _direction);
        CurrentPath = current.Connections[NextNode];
        CurrentLane = CurrentPath.GetLaneTo(NextNode);
        CurrentLane.Enter(this);

        PathPos = 0;
        Godot.GD.Print($"Start moving to {NextNode.Pos}");

        _action = FollowPath;
    }

    protected abstract void DestinationReached();
}
