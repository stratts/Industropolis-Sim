public class Building { 
    public float ProcessingTime { get; set; } = 0;
    public bool Processing { get; private set; } = false;

    public IConsumer Input { get; set; }
    public IProducer Output { get; set; }

    private float lastProcess = 0;

    public void Update(float delta) {
        if (Input.CanRetrieve) {
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
        Input = new DirectInOut();
        Output = new DirectInOut(100);
        ProcessingTime = 0;
    }
}

public class InfiniteStorage : Building {
    public InfiniteStorage() {
        Input = new InfiniteInput();
        Output = new DirectInOut();
        ProcessingTime = 0;
    }
}