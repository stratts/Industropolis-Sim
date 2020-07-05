using System;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public abstract class VehiclePath : Path<VehicleNode>
    {
        public IReadOnlyCollection<VehicleLane> Lanes => _lanes;
        protected List<VehicleLane> _lanes;

        public VehiclePath(VehicleNode source, VehicleNode dest) : base(source, dest)
        {
            _lanes = new List<VehicleLane>();
            AddLanes();
        }

        protected abstract void AddLanes();

        public bool HasLaneTo(VehicleNode node)
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

        public VehicleLane GetLaneTo(VehicleNode node)
        {
            foreach (var lane in _lanes)
            {
                if (lane.Dest == node || lane is BidirectionalLane) return lane;
            }

            throw new ArgumentException("Path contains no lane to that node");
        }

        public VehicleLane GetLaneFrom(VehicleNode node)
        {
            foreach (var lane in _lanes)
            {
                if (lane.Source == node || lane is BidirectionalLane) return lane;
            }

            throw new ArgumentException("Path contains no lane to that node");
        }
    }

    public abstract class VehicleLane
    {
        public VehiclePath Parent { get; private set; }
        public abstract VehicleNode Source { get; }
        public abstract VehicleNode Dest { get; }

        public VehicleLane(VehiclePath parent)
        {
            Parent = parent;
        }

        public abstract bool AtCapacity { get; }

        public abstract void Enter(Vehicle vehicle);

        public abstract void Depart(Vehicle vehicle);

        public abstract Vehicle? GetVehicleAhead(Vehicle vehicle);
    }

    public class UnidirectionalLane : VehicleLane
    {
        private List<Vehicle> _queue = new List<Vehicle>();

        public LaneDir Direction { get; private set; }
        public override VehicleNode Source => Direction == LaneDir.Dest ? Parent.Source : Parent.Dest;
        public override VehicleNode Dest => Direction == LaneDir.Dest ? Parent.Dest : Parent.Source;

        public UnidirectionalLane(VehiclePath parent, LaneDir direction) : base(parent)
        {
            Direction = direction;
        }

        public override bool AtCapacity => _queue.Count >= Parent.Length;

        public override void Enter(Vehicle vehicle)
        {
            _queue.Add(vehicle);
        }

        public override void Depart(Vehicle vehicle)
        {
            _queue.Remove(vehicle);
        }

        public override Vehicle? GetVehicleAhead(Vehicle vehicle)
        {
            for (int i = 1; i < _queue.Count; i++)
            {
                if (_queue[i] == vehicle) return _queue[i - 1];
            }
            return null;
        }
    }

    public class BidirectionalLane : VehicleLane
    {
        private Vehicle? _occupier;

        public BidirectionalLane(VehiclePath parent) : base(parent)
        {
        }

        public override VehicleNode Source => Parent.Source;

        public override VehicleNode Dest => Parent.Dest;

        public override bool AtCapacity => _occupier != null;

        public override void Depart(Vehicle vehicle)
        {
            if (vehicle != _occupier) throw new ArgumentException("Vehicle is not occupying lane");
            _occupier = null;
        }

        public override void Enter(Vehicle vehicle)
        {
            if (AtCapacity) throw new ArgumentException("Lane is at capacity");
            _occupier = vehicle;
        }

        public override Vehicle? GetVehicleAhead(Vehicle vehicle) => null;
    }

    public enum LaneDir
    {
        Source,
        Dest
    }
}
