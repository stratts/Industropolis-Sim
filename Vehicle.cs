
public class Vehicle {
    
    private float _speed = 1; // Tiles per second
    private Route.Direction _direction;

    public Route Route { get; }
    public Path CurrentPath { get; private set; }
    public float PathPos { get; private set; }
    public PathNode NextNode { get; private set; }
    public PathNode Destination { get; private set; }

    public Vehicle(Route route) {
        Route = route;
        _direction = Route.Direction.Forwards;
        Destination = Route.Dest;

        GoNext(Route.Source);
    }

    public void Update(float elapsedTime) {
        if (PathPos < CurrentPath.Length) {
            PathPos += _speed * elapsedTime;
        }
        else {
            if (NextNode != Destination) GoNext(NextNode);
        }
    }

    private void GoNext(PathNode current) {
        NextNode = Route.Next(current, _direction);
        CurrentPath = current.Connections[NextNode];
        PathPos = 0;
        Godot.GD.Print($"Start moving to {NextNode.Pos}");
    }

}
