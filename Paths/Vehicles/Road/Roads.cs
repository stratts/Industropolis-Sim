
namespace Industropolis.Sim
{
    public abstract class Road : VehiclePath
    {
        public Road(VehicleNode source, VehicleNode dest) : base(source, dest) { }

        public override PathCategory Category => PathCategory.Road;
    }

    public class SimpleRoad : Road
    {
        public SimpleRoad(VehicleNode source, VehicleNode dest) : base(source, dest) { }

        public override PathType PathType => PathType.SimpleRoad;

        protected override void AddLanes()
        {
            _lanes.Add(new UnidirectionalLane(this, LaneDir.Source));
            _lanes.Add(new UnidirectionalLane(this, LaneDir.Dest));
        }
    }

    public class OneWayRoad : Road
    {
        public OneWayRoad(VehicleNode source, VehicleNode dest) : base(source, dest) { }

        public override PathType PathType => PathType.OneWayRoad;

        protected override void AddLanes()
        {
            _lanes.Add(new UnidirectionalLane(this, LaneDir.Dest));
            _lanes.Add(new UnidirectionalLane(this, LaneDir.Dest));
        }
    }
}