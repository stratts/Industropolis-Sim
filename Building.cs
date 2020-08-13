using System.Collections.Generic;
using System.Linq;

namespace Industropolis.Sim
{
    using static Building.BuildingDirection;

    internal static class IntVectorExtensions
    {
        public static IntVector ApplyRotation(this IntVector pos, Building building)
        {
            IntVector FlipH(IntVector p) => (building.MapWidth - 1 - p.X, p.Y);
            IntVector FlipV(IntVector p) => (p.X, building.MapHeight - 1 - p.Y);
            IntVector Transpose(IntVector p) => (p.Y, p.X);

            return building.Direction switch
            {
                Down => pos,
                Up => FlipV(FlipH(pos)),
                Left => FlipV(Transpose(pos)),
                Right => FlipH(Transpose(pos)),
                _ => pos
            };
        }
    }

    public class BuildingEntrance
    {
        private Building _parent;
        private IntVector _start;
        private IntVector _end;

        public IntVector Start => _parent.Pos + _start.ApplyRotation(_parent);
        public IntVector End => _parent.Pos + _end.ApplyRotation(_parent);
        public VehicleNode? Node { get; private set; }
        public bool Connected => Node != null;

        public PathType Type { get; }

        public BuildingEntrance(Building parent, IntVector start, IntVector end, PathType type)
        {
            _parent = parent;
            _start = start;
            _end = end;
            Type = type;
        }

        public void Connect(VehicleNode node)
        {
            node.AddLink(_parent);
            _parent.AddLink(node);
            Node = node;
        }

        public void Disconnect()
        {
            if (Node != null)
            {
                Node.RemoveLink(_parent);
                _parent.RemoveLink(Node);
                Node = null;
            }
        }
    }

    public class Building : MapObject
    {
        public enum BuildingDirection { Down, Left, Up, Right };

        public IntVector Pos { get; set; }
        public BuildingType Type { get; protected set; }

        public IDirectInput? Input { get; protected set; }
        public IDirectOutput? Output { get; protected set; }

        public int Width { get; protected set; } = 1;
        public int Height { get; protected set; } = 1;

        public int MapWidth => Rotated ? Height : Width;
        public int MapHeight => Rotated ? Width : Height;

        public BuildingDirection Direction { get; set; } = BuildingDirection.Down;
        public bool Rotated => Direction switch { Up => false, Down => false, Left => true, Right => true, _ => false };

        public bool HasEntrance => Entrance != null;
        public BuildingEntrance? Entrance { get; protected set; }

        public virtual void Update(float delta)
        {

        }

        public static Building Create(Map map, BuildingType type, IntVector pos) => BuildingFactory.Create(map, type, pos);
        public static RequiredResources GetRequiredResources(BuildingType type) => BuildingFactory.GetRequiredResources(type);
    }

    public class ProductionBuilding : Building
    {
        public float ProcessingTime { get; set; } = 0;
        public bool Processing { get; private set; } = false;

        public IConsumer? Consumer { get; set; } = null;
        public IProducer? Producer { get; set; } = null;

        private float lastProcess = 0;

        public override void Update(float delta)
        {
            if (Consumer == null || Producer == null) return;
            if (Producer.CanProduce)
            {
                if (Consumer.CanConsume && !Processing)
                {
                    Consumer.Consume();
                    Processing = true;
                    lastProcess = 0;
                }
                else if (Processing)
                {
                    lastProcess += delta;
                    if (lastProcess >= ProcessingTime)
                    {
                        Processing = false;
                        Producer.Produce();
                    }
                }
            }
        }
    }
}
