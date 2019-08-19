public class Building : GameObject { 
    public TilePos Pos { get; set; }

    public float ProcessingTime { get; set; } = 0;
    public bool Processing { get; private set; } = false;

    public IConsumer Input { get; set; } = null;
    public IProducer Output { get; set; } = null;

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

    }

    public void LoadRecipe(Recipe recipe) {
        Input = new DirectInput(recipe.InputCount * 2, recipe.InputCount, recipe.InputItem);
        Output = new DirectOutput(recipe.OutputCount * 5, recipe.OutputCount, recipe.OutputItem);
        ProcessingTime = recipe.ProcessingTime;
    }
}

/*public class Storage : Building {
    public Storage() {
        var buffer = new DirectInOut(200, Item.Wood);
        Input = buffer;
        Output = buffer;
        ProcessingTime = 0;
    }

    public override void Update(float delta) {

    }
}*/

public class InfiniteStorage : Building {
    public InfiniteStorage(Item item) {
        Input = new InfiniteInput(item);
        Output = new DirectOutput(200, 1, item);
        ProcessingTime = 0;
    }
}