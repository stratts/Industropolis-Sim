

namespace Industropolis.Sim
{
    public class RailBuilder : PathBuilder<RailNode, Rail>
    {
        private Map _map;

        public RailBuilder(Map map) : base(map.Rails)
        {
            _map = map;
        }

        public override bool CanBuildAt(PathType type, IntVector pos)
        {
            if (_map.GetBuilding(pos) != null) return false;
            if (_map.GetPath(pos) != null || _map.GetNode(pos) != null) return false;
            return true;
        }

        public override RailNode MakeNode(IntVector pos, PathCategory category) => new RailNode(pos, category);

        public override Rail MakePath(PathType type, RailNode source, RailNode dest)
        {
            switch (type)
            {
                case PathType.Rail: return new Rail(source, dest);
                default: return new Rail(source, dest);
            }
        }
    }
}
