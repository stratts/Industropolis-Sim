public class Building { 
    public float ProcessingTime { get; set; } = 0;
    public bool Processing { get; private set; } = false;

    public IConsumer Input { get; set; }
    public IProducer Output { get; set; }

    private float lastProcess = 0;

    public virtual void Update(float delta) {
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
    public Workshop(Item input, int inputReq, Item output, int outputAmount, int time) { 
        Input = new DirectInput(inputReq * 2, inputReq, input);
        Output = new DirectOutput(5, outputAmount, output);

        ProcessingTime = time;
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