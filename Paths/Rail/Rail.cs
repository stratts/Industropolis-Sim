using System;


namespace Industropolis.Sim
{
    public class RailNode : PathNode<RailNode, Rail>
    {
        public RailNode(IntVector pos, PathCategory category) : base(pos, category)
        {
        }

        public override event Action<RailNode>? Changed;

        public override void OnChange() => Changed?.Invoke(this);
    }

    public class Rail : Path<RailNode>
    {
        public Rail(RailNode source, RailNode dest) : base(source, dest)
        {
        }

        public override PathCategory Category => PathCategory.Rail;

        public override PathType PathType => PathType.Rail;
    }
}
