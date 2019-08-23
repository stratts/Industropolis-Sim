using System.Collections.Generic;

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