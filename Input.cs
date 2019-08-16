
public interface IConsumer {
    Item Item { get; }
    bool CanConsume { get; }
    bool Consume();
}

public class InfiniteInput : IConsumer {
    public InfiniteInput(Item item) {
        Item = item;
    }

    public Item Item { get; set; }
    public bool CanConsume => true;
    public bool Consume() => true;
}

public abstract class DirectBase {
    protected int buffer = 0;
    protected int bufferSize = 200;

    public Item Item { get; set; }

    public int Buffer => buffer;
    public int BufferSize => bufferSize;
}

public class DirectInput : DirectBase, IConsumer {
    public int ConsumeAmount { get; private set; }

    public bool CanConsume => buffer >= ConsumeAmount;
    public bool CanInsert => buffer < bufferSize;

    public DirectInput(int bufferSize, int consumeAmount, Item item) {
        this.bufferSize = bufferSize;
        this.Item = item;
        this.ConsumeAmount = consumeAmount;
    }

    public bool Consume() {
        if (!CanConsume) return false;
        buffer -= ConsumeAmount;
        return true;
    }

    public bool Insert() {
        if (CanInsert) {
            buffer++;
            return true;
        }
        return false;
    }
}