
public interface IProducer {
    bool CanPush { get; }
    bool Push();
}

public class DirectInOut : IConsumer, IProducer {
    private int buffer = 0;
    private int bufferSize = 200;

    private int pushAmount = 1;
    private int retrieveAmount = 1;

    public int Buffer => buffer;
    public int BufferSize => bufferSize;
    public bool CanRetrieve => buffer >= retrieveAmount;
    public bool CanPush => buffer <= bufferSize - retrieveAmount;

    public DirectInOut() {

    }

    public DirectInOut(int bufferSize) {
        this.bufferSize = bufferSize;
    }

    public bool Retrieve() {
        if (!CanRetrieve) return false;
        buffer -= retrieveAmount;
        return true;
    }

    public bool Push() {
        if (CanPush) {
            buffer += pushAmount;
            return true;
        }
        return false;
    }
}