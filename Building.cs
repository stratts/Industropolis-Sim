using System.Collections.Generic;


namespace Industropolis.Sim
{
    public enum BuildingType
    {
        None,
        Workshop,
        //House,
        Mine,
        Farm,
        TestProducer,
        TestConsumer,
        TrainStation,
        Core
    }

    public class BuildingEntrance
    {
        private Building _parent;

        public IntVector Location { get; }
        public VehicleNode? Node { get; private set; }
        public IntVector Pos => _parent.Pos + Location;
        public IntVector ConnectionPos => Pos + new IntVector(0, 1);
        public bool Connected => Node != null;

        public PathCategory Category { get; }

        public BuildingEntrance(Building parent, IntVector location, PathCategory category)
        {
            Category = category;
            Location = location;
            _parent = parent;
        }

        public bool CanConnect(IntVector pos, PathCategory category) =>
            pos == ConnectionPos &&
            category == Category &&
            !Connected;

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
        public IntVector Pos { get; set; }
        public BuildingType Type { get; protected set; }

        public IDirectInput? Input { get; protected set; }
        public IDirectOutput? Output { get; protected set; }

        public int Width { get; protected set; } = 1;
        public int Height { get; protected set; } = 1;

        public int Cost { get; set; } = 0;

        public bool HasEntrance => Entrance != null;
        public BuildingEntrance? Entrance { get; protected set; }

        //public IReadOnlyDictionary<Item, int> RequiredResources => _requiredResources;

        //protected Dictionary<Item, int> _requiredResources = null;

        public virtual void Update(float delta)
        {

        }
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
