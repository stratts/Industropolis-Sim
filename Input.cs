
using System.Collections.Generic;

public interface IConsumer {
    bool CanConsume { get; }
    bool Consume();
}

public interface IDirectInput {
    bool Accepts(Item item);
    bool CanInsert(Item item);
    bool Insert(Item item);
}

public class InfiniteInput : IConsumer {
    public InfiniteInput(Item item) {
        Item = item;
    }

    public Item Item { get; set; }
    public bool CanConsume => true;
    public bool Consume() => true;
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

public class DirectConsumer : IConsumer, IDirectInput {
    private List<ItemBuffer> _buffers = new List<ItemBuffer>();
    private Dictionary<Item, int> _consumeAmount = new Dictionary<Item, int>();

    public bool CanConsume => _canConsume();
    public IEnumerable<Item> Items => _consumeAmount.Keys;

    public DirectConsumer() {

    }

    public DirectConsumer(int bufferSize, int consumeAmount, Item item) {
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

public class Storage : IDirectInput, IDirectOutput
{
    private Dictionary<Item, ItemBuffer> _buffers = new Dictionary<Item, ItemBuffer>();
    public IReadOnlyList<Item> Items => new List<Item>(_buffers.Keys);

    public bool Accepts(Item item)
    {
        return true;
    }

    public bool CanInsert(Item item)
    {
        return true;
    }

    public bool CanRemove(Item item)
    {
        if (!Has(item) || !GetBuffer(item).CanRemove) return false;
        return true;
    }

    public bool Has(Item item)
    {
        _buffers.TryGetValue(item, out ItemBuffer buffer);
        if (buffer == null) return false;
        return true;
    }

    public bool Insert(Item item)
    {
        GetBuffer(item).Insert();
        return true;
    }

    public bool Remove(Item item)
    {
        var buffer = GetBuffer(item);
        if (!buffer.CanRemove) return false;
        buffer.Remove();
        return true;
    }

    public ItemBuffer GetBuffer(Item item) {
        _buffers.TryGetValue(item, out ItemBuffer buffer);

        if (buffer == null) {
            buffer = new ItemBuffer(1000, item);
            _buffers[item] = buffer;
        }

        return buffer;
    }
}