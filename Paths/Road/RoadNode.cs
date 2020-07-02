using System;


namespace Industropolis.Sim
{
    public class RoadNode : PathNode<RoadNode, Road>
    {
        public bool Occupied { get; set; } = false;

        public RoadNode(IntVector pos, PathCategory category) : base(pos, category)
        {
        }

        public override event Action<RoadNode>? Changed;

        public override void OnChange() => Changed?.Invoke(this);

        public bool HasPathTo(RoadNode node) => IsConnected(node) && Connections[node].HasLaneTo(node);

        public bool CanProceed(RoadNode source, RoadNode dest) =>
            !Occupied &&
            (dest == this ||
            !Connections[dest].GetLaneFrom(this).AtCapacity);
    }
}
