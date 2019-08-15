
public interface IConsumer {
    bool CanRetrieve { get; }
    bool Retrieve();
}

public class InfiniteInput : IConsumer {
    public bool CanRetrieve => true;
    public bool Retrieve() => true;
}