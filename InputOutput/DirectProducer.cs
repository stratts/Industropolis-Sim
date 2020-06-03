using System.Collections.Generic;

public class DirectProducer : IProducer, IDirectOutput
{
    private ItemBuffer _buffer;
    private Item[] _items;
    private Item _item;

    public int Buffer => _buffer.Buffer;
    public int BufferSize => _buffer.BufferSize;

    public Item Item => _item;
    public int ProduceAmount { get; set; }
    public IReadOnlyList<Item> Items => _items;
    public bool CanProduce => _buffer.Buffer <= _buffer.BufferSize - ProduceAmount;

    public DirectProducer(int bufferSize, int produceAmount, Item item)
    {
        _buffer = new ItemBuffer(bufferSize, item);
        _items = new[] { item };
        _item = item;
        this.ProduceAmount = produceAmount;
    }

    public bool Produce()
    {
        if (CanProduce)
        {
            _buffer.Buffer += ProduceAmount;
            return true;

        }
        return false;
    }

    public bool Has(Item item)
    {
        if (item != _item) return false;
        return true;
    }

    public bool CanRemove(Item item)
    {
        if (item != _item || _buffer.Buffer <= 0) return false;
        return true;
    }

    public bool Remove(Item item)
    {
        if (!CanRemove(item)) return false;
        _buffer.Buffer -= 1;
        return true;
    }

    public int AmountOf(Item item)
    {
        if (!Has(item)) return 0;
        return Buffer;
    }
}