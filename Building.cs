using System.Collections.Generic;

public class Building : GameObject { 
    public TilePos Pos { get; set; }

    public IDirectInput Input { get; set; } = null;
    public IDirectOutput Output { get; set; } = null;

    public IReadOnlyDictionary<Item, int> RequiredResources => _requiredResources;

    protected Dictionary<Item, int> _requiredResources = null;

    public virtual void Update(float delta) {
        
    }
}

public class ProductionBuilding : Building {
    public float ProcessingTime { get; set; } = 0;
    public bool Processing { get; private set; } = false;

    public IConsumer Consumer { get; set; } = null;
    public IProducer Producer { get; set; } = null;

    private float lastProcess = 0;

    public override void Update(float delta) {
        if (Consumer == null || Producer == null) return;
        if (Producer.CanProduce) {
            if (Consumer.CanConsume && !Processing) {
                Consumer.Consume();
                Processing = true;
                lastProcess = 0;
            }
            else if (Processing) {
                lastProcess += delta;
                if (lastProcess >= ProcessingTime) {
                    Processing = false;
                    Producer.Produce();
                }
            }
        }
    }
}

public class Workshop : ProductionBuilding {
    public Recipe Recipe {
        get {
            return _recipe;
        }
        set {
            _recipe = value;
            LoadRecipe(value);
        }
    }

    private Recipe _recipe;

    public Workshop() {
        _requiredResources = new Dictionary<Item, int>() {
            {Item.Wood, 50}
        };
    }

    public void LoadRecipe(Recipe recipe) {
        var input = new DirectConsumer();
        foreach (RecipeInput i in recipe.Input) {
            input.AddItem(i.Count * 2, i.Count, i.Item);
        }
        Input = input;
        Consumer = input;
        var producer = new DirectProducer(recipe.OutputCount * 5, recipe.OutputCount, recipe.OutputItem);
        Producer = producer;
        Output = producer;
        ProcessingTime = recipe.ProcessingTime;
    }
}

public class House : ProductionBuilding {
    public House(PopulationInfo population) {
        var input = new DirectConsumer(5, 1, Item.Food);
        Input = input;
        Consumer = input;
        Producer = new PopulationOutput(population);
        ProcessingTime = 5;
    }
}

public class Stockpile : Building {
    private Storage _storage = new Storage();

    public Stockpile() {
        Input = _storage;
        Output = _storage;
    }

    public void AddItem(Item item, int amount) {
        for (int i = 0; i < amount; i++) {
            _storage.Insert(item);
        }
    }

    public bool HasItem(Item item, int amount) {
        if (!_storage.Has(item)) return false;
        var buffer = _storage.GetBuffer(item);
        if (buffer.Buffer < amount) return false;
        return true;
    }

    public void RemoveItem(Item item, int amount) {
        for (int i = 0; i < amount; i++) {
            _storage.Remove(item);
        }
    }
}