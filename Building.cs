using System.Collections.Generic;

public class Building : GameObject { 
    public TilePos Pos { get; set; }

    public float ProcessingTime { get; set; } = 0;
    public bool Processing { get; private set; } = false;

    public IConsumer Input { get; set; } = null;
    public IProducer Output { get; set; } = null;

    public IReadOnlyDictionary<Item, int> RequiredResources => _requiredResources;

    protected Dictionary<Item, int> _requiredResources = null;
    private float lastProcess = 0;

    public virtual void Update(float delta) {
        if (Input == null || Output == null) return;
        if (Input.CanConsume && Output.CanProduce) {
            lastProcess += delta;
            if (lastProcess >= ProcessingTime) {
                lastProcess = 0;
                Input.Consume();
                Output.Produce();
            }
        }
    }
}

public class Workshop : Building {
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
        Input = new DirectInput(recipe.InputCount * 2, recipe.InputCount, recipe.InputItem);
        Output = new DirectOutput(recipe.OutputCount * 5, recipe.OutputCount, recipe.OutputItem);
        ProcessingTime = recipe.ProcessingTime;
    }
}

public class House : Building {
    public House(PopulationInfo population) {
        Input = new DirectInput(5, 1, Item.Food);
        Output = new PopulationOutput(population);
        ProcessingTime = 5;
    }
}

public class Stockpile : Building {
    private DirectOutput output = new DirectOutput(1000, 1, Item.Wood);
    private DirectInput input = new DirectInput(1000, 1, Item.Wood);

    public Stockpile() {
        Input = input;
        Output = output;
        ProcessingTime = 0;
    }

    public void AddItem(Item item, int amount) {
        for (int i = 0; i < amount; i++) {
            input.Insert();
        }
    }

    public bool HasItem(Item item, int amount) {
        return output.Buffer >= amount;
    }

    public void RemoveItem(Item item, int amount) {
        for (int i = 0; i < amount; i++) {
            output.Remove();
        }
    }
}

public class InfiniteStorage : Building {
    public InfiniteStorage(Item item) {
        Input = new InfiniteInput(item);
        Output = new DirectOutput(200, 1, item);
        ProcessingTime = 0;
    }
}