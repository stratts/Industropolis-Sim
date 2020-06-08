using System;

public class Vehicle
{

    private float _speed = 1; // Tiles per second
    private Route.Direction _direction;

    public Route Route { get; }
    public Path CurrentPath { get; private set; }
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
        if (PathPos < CurrentPath.Length - 1) PathPos += _speed * _elapsedTime;
        else if (CanGoNext()) _action = CrossIntersection;
    }

    private void CrossIntersection()
    {
        if (PathPos < CurrentPath.Length) PathPos += _speed * _elapsedTime;
        else if (NextNode != Destination) GoNext();
    }

    private bool CanGoNext() => NextNode.CanProceed(PrevNode, Route.Next(NextNode, _direction));

    private void GoNext()
    {
        var current = NextNode;
        PrevNode = current;
        NextNode = Route.Next(current, _direction);
        CurrentPath = current.Connections[NextNode];
        PathPos = 0;
        Godot.GD.Print($"Start moving to {NextNode.Pos}");

        _action = FollowPath;
    }
}
