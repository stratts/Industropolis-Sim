

namespace Industropolis.Sim
{
    public class TrainStation : Building
    {
        private Map _map;
        private VehicleNode? _stop;
        private VehicleNode? _entrance;

        public TrainStation(Map map)
        {
            _map = map;
            Width = 2;
            Height = 4;
            Input = new InfiniteDirectInput();
            Output = new InfiniteDirectOutput(Item.Wood);
        }

        public void Setup()
        {
            var stopPos = Pos + new IntVector(Width, 0);
            var entrancePos = Pos + new IntVector(Width, Height - 1);
            _map.BuildPath(PathType.Rail, stopPos, entrancePos, true);

            _stop = _map.GetNode(stopPos);
            _entrance = _map.GetNode(entrancePos);

            if (_stop == null) throw new System.Exception("Could not build train station rails");

            Entrance = new BuildingEntrance(this, _stop.Pos, PathCategory.Rail);
            Entrance.Connect(_stop);
        }

        public override void Remove()
        {
            if (_stop != null) _map.RemoveNode(_stop);
            if (_entrance != null)
            {
                _entrance.Fixed = false;
                if (_entrance.Connections.Count == 0) _map.RemoveNode(_entrance);
            }
            base.Remove();
        }
    }
}