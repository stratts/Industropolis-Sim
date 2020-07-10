using System;
using System.Numerics;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public abstract class Vehicle : MapObject
    {
        private float _speed = 1.5f; // Tiles per second
        private RouteDirection _direction;
        private List<VehicleLane> _lanes = new List<VehicleLane>();
        private List<VehicleNode> _nodes = new List<VehicleNode>();

        // Set to null! as they are initialized when GoNext() is called in constructor
        public Route<VehicleNode> Route { get; }
        public VehiclePath CurrentPath { get; private set; } = null!;
        public VehicleLane CurrentLane { get; private set; } = null!;
        public float FrontPos { get; private set; }
        public float RearPos { get; private set; }
        public VehicleNode RearNode { get; private set; } = null!;
        public VehicleNode PrevNode { get; private set; } = null!;
        public VehicleNode NextNode { get; private set; } = null!;
        public VehicleNode Destination { get; private set; } = null!;
        public float Length { get; set; } = 0.5f;
        public IReadOnlyList<VehicleLane> Lanes => _lanes;

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
            if (FrontPos < CurrentPath.Length - 1) Move();
            else if (CanGoNext())
            {
                _action = CrossIntersection;
            }
        }

        protected void CrossIntersection()
        {
            if (FrontPos < CurrentPath.Length) Move();
            else if (NextNode != Destination)
            {
                GoNext();
            }
            else
            {
                DestinationReached();
            }
        }

        protected void Move()
        {
            if (CurrentLane.GetVehicleAhead(this) is Vehicle v && v.RearPos - FrontPos < 0.5f) return;
            FrontPos += _speed * _elapsedTime;
            RearPos += _speed * _elapsedTime;
            if (_lanes.Count > 0)
            {
                var rearLane = _lanes[0];
                if (RearPos > rearLane.Parent.Length)
                {
                    RearPos = 0;
                    rearLane.Depart(this);
                    _lanes.Remove(rearLane);

                    if (_nodes.Count > 0)
                    {
                        var node = _nodes[0];
                        node.Occupied = false;
                        RearNode = node;
                        _nodes.Remove(node);
                    }
                }
            }
        }

        protected void FlipDirection()
        {
            if (_direction == RouteDirection.Forwards) SetDirection(RouteDirection.Backwards);
            else SetDirection(RouteDirection.Forwards);
        }

        protected void SetDirection(RouteDirection direction)
        {
            FrontPos = 0;
            RearPos = -Length;
            foreach (var lane in _lanes) lane.Depart(this);
            foreach (var node in _nodes) node.Occupied = false;
            _nodes.Clear();
            _lanes.Clear();
            _direction = direction;

            switch (direction)
            {
                case RouteDirection.Forwards:
                    Destination = Route.Dest;
                    PrevNode = Route.Source;
                    RearNode = Route.Source;
                    NextNode = Route.Source;
                    break;
                case RouteDirection.Backwards:
                    Destination = Route.Source;
                    PrevNode = Route.Dest;
                    RearNode = Route.Dest;
                    NextNode = Route.Dest;
                    break;
            }
        }

        protected bool CanGoNext() => NextNode.CanProceed(PrevNode, Route.Next(NextNode, _direction));

        protected void GoNext()
        {
            if (PrevNode != NextNode)
            {
                _nodes.Add(NextNode);
                NextNode.Occupied = true;
            }

            var current = NextNode;
            PrevNode = current;
            NextNode = Route.Next(current, _direction);
            CurrentPath = current.Connections[NextNode];
            CurrentLane = CurrentPath.GetLaneTo(NextNode);
            CurrentLane.Enter(this);
            _lanes.Add(CurrentLane);

            FrontPos = 0;

            _action = FollowPath;
        }

        public IEnumerable<Vector2> GetPoints()
        {
            // Position of front of vehicle
            yield return PrevNode.Pos.ToVector2() + PrevNode.Pos.Direction(NextNode.Pos) * FrontPos;
            // Intermediate occupied nodes, if any
            for (int i = _nodes.Count - 1; i >= 0; i--) yield return _nodes[i].Pos.ToVector2();
            // Position of rear of vehicle
            var rearPos = Math.Max(RearPos, 0);
            if (_nodes.Count == 0) yield return PrevNode.Pos.ToVector2() + PrevNode.Pos.Direction(NextNode.Pos) * rearPos;
            else yield return RearNode.Pos.ToVector2() + RearNode.Pos.Direction(_nodes[0].Pos) * rearPos;
        }

        protected abstract void DestinationReached();
    }
}
