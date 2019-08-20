
using System.Collections.Generic;

public interface IConsumer {
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

public class ItemBuffer {
    public int Buffer { get; set; }
    public int BufferSize { get; private set; }
    public Item Item { get; private set; }

    public bool CanInsert => Buffer < BufferSize;
    public bool CanRemove => Buffer > 0;

    public ItemBuffer(int bufferSize, Item item) {
        BufferSize = bufferSize;
        Item = item;
    }

    public void Insert() {
        if (CanInsert) Buffer++;
    }

    public void Remove() {
        if (CanRemove) Buffer--;
    }
}

public class DirectInput : IConsumer {
    private List<ItemBuffer> _buffers = new List<ItemBuffer>();
    private Dictionary<Item, int> _consumeAmount = new Dictionary<Item, int>();

    public bool CanConsume => _canConsume();
    public IEnumerable<Item> Items => _consumeAmount.Keys;

    public DirectInput() {

    }

    public DirectInput(int bufferSize, int consumeAmount, Item item) {
        AddItem(bufferSize, consumeAmount, item);
    }

    public bool Accepts(Item item) {
        return _consumeAmount.ContainsKey(item);
    }

    public bool CanInsert(Item item) {
        return GetBuffer(item).CanInsert;
    }

    public bool Insert(Item item) {
        if (!CanInsert(item)) return false;
        GetBuffer(item).Insert();
        return true;
    }

    public int GetConsumeAmount(Item item) {
        return _consumeAmount[item];
    }

    public ItemBuffer GetBuffer(Item item) {
        foreach (ItemBuffer b in _buffers) {
            if (b.Item == item) return b;
        }
        return null;
    }

    public void AddItem(int bufferSize, int consumeAmount, Item item) {
        _consumeAmount[item] = consumeAmount;
        _buffers.Add(new ItemBuffer(bufferSize, item));
    }

    private bool _canConsume() {
        foreach (ItemBuffer b in _buffers) {
            if (_consumeAmount[b.Item] > b.Buffer) return false;
        }
        return true;
    }

    public bool Consume() {
        if (!CanConsume) return false;
        foreach (ItemBuffer b in _buffers) {
            b.Buffer -= _consumeAmount[b.Item];
        }
        return true;
    }
}