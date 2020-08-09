using System.Collections.Generic;


namespace Industropolis.Sim
{
    public class BuildingEntrance
    {
        private Building _parent;
        private IntVector _start;
        private IntVector _end;

        public IntVector Start => _parent.Pos + _start;
        public IntVector End => _parent.Pos + _end;
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
        private Dictionary<Item, int>? _requiredResources = null;

        public IntVector Pos { get; set; }
        public BuildingType Type { get; protected set; }

        public IDirectInput? Input { get; protected set; }
        public IDirectOutput? Output { get; protected set; }

        public int Width { get; protected set; } = 1;
        public int Height { get; protected set; } = 1;

        public bool HasEntrance => Entrance != null;
        public BuildingEntrance? Entrance { get; protected set; }

        public IReadOnlyDictionary<Item, int>? RequiredResources => _requiredResources;

        protected void SetRequiredResources(params (Item item, int amount)[] resources)
        {
            if (_requiredResources == null) _requiredResources = new Dictionary<Item, int>();
            foreach (var resource in resources)
            {
                _requiredResources[resource.item] = resource.amount;
            }
        }

        public virtual void Update(float delta)
        {

        }

        public static Building Create(Map map, BuildingType type, IntVector pos) => BuildingFactory.Create(map, type, pos);
        public static IReadOnlyDictionary<Item, int>? GetRequiredResources(BuildingType type) => BuildingFactory.GetRequiredResources(type);
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
