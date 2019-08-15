
public interface IProducer {
    Item Item { get; }
    bool CanPush { get; }
    bool Push();
}

public class DirectInOut : IConsumer, IProducer {
    private int buffer = 0;
    private int bufferSize = 200;

    public Item Item { get; set; }

    public int PushAmount { get; set; } = 1;
    public int RetrieveAmount { get; set; }= 1;

    public int Buffer => buffer;
    public int BufferSize => bufferSize;
    public bool CanRetrieve => buffer >= RetrieveAmount;
    public bool CanPush => buffer <= bufferSize - RetrieveAmount;

    public DirectInOut() {

    }

    public DirectInOut(int bufferSize, Item item) {
        this.bufferSize = bufferSize;
        this.Item = item;
    }

    public bool Retrieve() {
        if (!CanRetrieve) return false;
        buffer -= RetrieveAmount;
        return true;
    }

    public bool Push() {
        if (CanPush) {
            buffer += PushAmount;
            return true;
        }
        return false;
    }
}