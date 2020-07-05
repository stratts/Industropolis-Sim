using System;


namespace Industropolis.Sim
{
    public class Rail : VehiclePath
    {
        public Rail(VehicleNode source, VehicleNode dest) : base(source, dest)
        {
        }

        public override PathCategory Category => PathCategory.Rail;

        public override PathType PathType => PathType.Rail;

        protected override void AddLanes()
        {
            _lanes.Add(new BidirectionalLane(this));
        }
    }
}
