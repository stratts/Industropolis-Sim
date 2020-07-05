using System;

namespace Industropolis.Sim
{
    public abstract class Vehicle : MapObject
    {
        private float _speed = 1; // Tiles per second
        private RouteDirection _direction;

        // Set to null! as they are initialized when GoNext() is called in constructor
        public Route<VehicleNode> Route { get; }
        public VehiclePath CurrentPath { get; private set; } = null!;
        public VehicleLane CurrentLane { get; private set; } = null!;
        public float PathPos { get; private set; }
        public VehicleNode PrevNode { get; private set; } = null!;
        public VehicleNode NextNode { get; private set; } = null!;
        public VehicleNode Destination { get; private set; } = null!;

        protected Action? _action;
        protected float _elapsedTime;

        public Vehicle(Route<VehicleNode> route)
        {
            Route = route;
            SetDirection(RouteDirection.Forwards);
            GoNext();
        }

        public void Update(float elapsedTime)
        {
            _elapsedTime = elapsedTime;
            _action?.Invoke();
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

        protected void FlipDirection()
        {
            if (_direction == RouteDirection.Forwards) SetDirection(RouteDirection.Backwards);
            else SetDirection(RouteDirection.Forwards);
        }

        protected void SetDirection(RouteDirection direction)
        {
            _direction = direction;

            switch (direction)
            {
                case RouteDirection.Forwards:
                    Destination = Route.Dest;
                    PrevNode = Route.Source;
                    NextNode = Route.Source;
                    break;
                case RouteDirection.Backwards:
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
            Console.WriteLine($"Start moving to {NextNode.Pos}");

            _action = FollowPath;
        }

        protected abstract void DestinationReached();
    }
}
