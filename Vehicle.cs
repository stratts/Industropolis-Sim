using System;

public class Vehicle
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

    private Action _action;
    private float _elapsedTime;

    public Vehicle(Route route)
    {
        Route = route;
        _direction = Route.Direction.Forwards;
        Destination = Route.Dest;
        PrevNode = Route.Source;
        NextNode = Route.Source;

        GoNext();
    }

    public void Update(float elapsedTime)
    {
        _elapsedTime = elapsedTime;
        _action();
    }

    private void FollowPath()
    {
        if (PathPos < CurrentPath.Length - 1) Move();
        else if (CanGoNext())
        {
            _action = CrossIntersection;
            NextNode.Occupied = true;
        }
    }

    private void CrossIntersection()
    {
        if (PathPos < CurrentPath.Length) Move();
        else if (NextNode != Destination)
        {
            NextNode.Occupied = false;
            GoNext();
        }
    }

    private void Move()
    {
        if (CurrentLane.GetVehicleAhead(this) is Vehicle v && v.PathPos - PathPos < 1) return;
        PathPos += _speed * _elapsedTime;
    }

    private bool CanGoNext() => NextNode.CanProceed(PrevNode, Route.Next(NextNode, _direction));

    private void GoNext()
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
}
