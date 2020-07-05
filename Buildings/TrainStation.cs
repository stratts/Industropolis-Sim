

namespace Industropolis.Sim
{
    public class TrainStation : Building
    {
        private Map _map;
        private TrainStop? _stop;
        private RailNode? _entrance;

        public TrainStation(Map map)
        {
            _map = map;
            Width = 2;
            Height = 4;
        }

        public void Setup()
        {
            _stop = new TrainStop(Pos + new IntVector(Width, 0), this);
            _entrance = new RailNode(Pos + new IntVector(Width, Height - 1));
            _stop.Fixed = true;
            _entrance.Fixed = true;

            var path = new Rail(_entrance, _stop);
            path.Fixed = true;

            _stop.Connect(_entrance, path);
            _entrance.Connect(_stop, path);

            _map.AddNode(_stop);
            _map.AddNode(_entrance);
            _map.AddPath(path);
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