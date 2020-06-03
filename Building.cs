using System.Collections.Generic;


public enum BuildingType
{
    None,
    Workshop,
    //House,
    Mine,
    Farm,
    TestProducer,
    TestConsumer
}

public class BuildingEntrance
{
    private Building _parent;

    public IntVector Location { get; }
    public BuildingNode Node { get; private set; }
    public IntVector Pos => _parent.Pos + Location;
    public IntVector ConnectionPos => Pos + new IntVector(0, 1);
    public bool Connected => Node != null;

    public BuildingEntrance(Building parent, IntVector location)
    {
        Location = location;
        _parent = parent;
    }

    public void Connect(BuildingNode node) => Node = node;
    public void Disconnect() => Node = null;
}

public class Building : MapObject
{
    public IntVector Pos { get; set; }
    public BuildingType Type { get; protected set; }

    public IDirectInput Input { get; set; } = null;
    public IDirectOutput Output { get; set; } = null;

    public int Width { get; protected set; } = 1;
    public int Height { get; protected set; } = 1;

    public int Cost { get; set; } = 0;

    public bool HasEntrance { get; protected set; } = false;
    public BuildingEntrance Entrance { get; protected set; }

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

    public IConsumer Consumer { get; set; } = null;
    public IProducer Producer { get; set; } = null;

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
