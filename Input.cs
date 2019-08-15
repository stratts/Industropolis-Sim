
public interface IConsumer {
    Item Item { get; }
    bool CanRetrieve { get; }
    bool Retrieve();
}

public class InfiniteInput : IConsumer {
    public InfiniteInput(Item item) {
        Item = item;
    }
    
    public Item Item { get; set; }
    public bool CanRetrieve => true;
    public bool Retrieve() => true;
}