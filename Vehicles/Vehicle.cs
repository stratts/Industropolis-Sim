using System;
using System.Numerics;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public abstract class Vehicle : MapObject
    {
        public enum Direction { Forwards, Backwards };

        private float _speed = 1.5f; // Tiles per second
        private List<VehicleLane> _lanes = new List<VehicleLane>();
        private List<VehicleNode> _nodes = new List<VehicleNode>();
        private Vehicle? _following;
        private int _routeIndex = 0;

        // Set to null! as they are initialized when GoNext() is called in constructor
        public Route<VehicleNode> Route { get; set; }
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
        public int RouteIndex => _routeIndex;

        public event Action<Vehicle>? DepartedLane;

        public abstract VehicleType Type { get; }

        protected Action? _action;
        protected float _elapsedTime;

        public Vehicle(Route<VehicleNode> route)
        {
            Route = route;
            Reset(route.Source, route.Dest);
            GoNext();
        }

        public void Update(float elapsedTime)
        {
            _elapsedTime = elapsedTime;
            _action?.Invoke();
        }

        public void SetPosition(int routeIndex, float frontPos)
        {
            _routeIndex = routeIndex - 1;
            Reset(Route.GetNode(_routeIndex), Route.GetNextDestination(_routeIndex));
            GoNext();
            FrontPos = frontPos;
            RearPos = frontPos - Length;
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
            if (_following != null && _following.RearPos - FrontPos < 0.5f) return;
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
                    DepartedLane?.Invoke(this);

                    if (_nodes.Count > 0)
                    {
                        var node = _nodes[0];
                        RearNode = node;
                        _nodes.Remove(node);
                    }
                }
            }
        }

        protected void Reset(VehicleNode current, VehicleNode dest)
        {
            FrontPos = 0;
            RearPos = -Length;
            ClearOccupied();
            Destination = dest;
            PrevNode = current;
            RearNode = current;
            NextNode = current;
        }

        public override void Remove()
        {
            ClearOccupied();
            base.Remove();
        }

        private void ClearOccupied()
        {
            foreach (var lane in _lanes) lane.Depart(this);
            foreach (var node in _nodes) node.Occupied = false;
            _nodes.Clear();
            _lanes.Clear();
            DepartedLane?.Invoke(this);
        }

        protected bool CanGoNext() => NextNode.CanProceed(PrevNode, Route.Next(_routeIndex).node);

        protected void GoNext()
        {
            if (PrevNode != NextNode) _nodes.Add(NextNode);

            var current = NextNode;
            PrevNode = current;
            (_routeIndex, NextNode) = Route.Next(_routeIndex);
            CurrentPath = current.Connections[NextNode];
            CurrentLane = CurrentPath.GetLaneTo(NextNode);
            CurrentLane.Enter(this);
            _lanes.Add(CurrentLane);
            _following = CurrentLane.GetVehicleAhead(this);
            if (_following != null) _following.DepartedLane += Unfollow;

            FrontPos = 0;

            _action = FollowPath;
        }

        private void Unfollow(Vehicle vehicle)
        {
            _following = null;
            vehicle.DepartedLane -= Unfollow;
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

        public static Vehicle Create(VehicleType type, Route route) => VehicleFactory.Create(type, route);
    }
}
