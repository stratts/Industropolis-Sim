public class Building { 
    public float ProcessingTime { get; set; } = 0;
    public bool Processing { get; private set; } = false;

    public IConsumer Input { get; set; }
    public IProducer Output { get; set; }

    private float lastProcess = 0;

    public virtual void Update(float delta) {
        if (Input.CanRetrieve && Output.CanPush) {
            lastProcess += delta;
            if (lastProcess >= ProcessingTime) {
                lastProcess = 0;
                Input.Retrieve();
                Output.Push();
            }
        }
    }
}

public class Storage : Building {
    public Storage() {
        var buffer = new DirectInOut(200);
        Input = buffer;
        Output = buffer;
        ProcessingTime = 0;
    }

    public override void Update(float delta) {

    }
}

public class InfiniteStorage : Building {
    public InfiniteStorage() {
        Input = new InfiniteInput();
        Output = new DirectInOut();
        ProcessingTime = 0;
    }
}