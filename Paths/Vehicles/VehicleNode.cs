using System;


namespace Industropolis.Sim
{
    public abstract class VehicleNode : PathNode<VehicleNode, VehiclePath>
    {
        public bool Occupied { get; set; } = false;

        public VehicleNode(IntVector pos) : base(pos)
        {
        }

        public override event Action<VehicleNode>? Changed;

        public override void OnChange() => Changed?.Invoke(this);

        public override bool HasPathTo(VehicleNode node) => IsConnected(node) && Connections[node].HasLaneTo(node);

        public bool CanProceed(VehicleNode source, VehicleNode dest) =>
            !Occupied &&
            (dest == this ||
            !Connections[dest].GetLaneFrom(this).AtCapacity);
    }
}
