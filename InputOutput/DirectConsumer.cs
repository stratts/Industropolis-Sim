using System.Collections.Generic;

namespace Industropolis.Sim
{
    public class DirectConsumer : IConsumer, IDirectInput
    {
        private List<ItemBuffer> _buffers = new List<ItemBuffer>();
        private Dictionary<Item, int> _consumeAmount = new Dictionary<Item, int>();

        public bool CanConsume => _canConsume();
        public IEnumerable<Item> Items => _consumeAmount.Keys;

        public IReadOnlyList<ItemBuffer> Buffers => _buffers;

        public DirectConsumer()
        {

        }

        public DirectConsumer(int bufferSize, int consumeAmount, Item item)
        {
            AddItem(bufferSize, consumeAmount, item);
        }

        public bool Accepts(Item item)
        {
            return _consumeAmount.ContainsKey(item);
        }

        public bool CanInsert(Item item)
        {
            return GetBuffer(item).CanInsert;
        }

        public bool Insert(Item item)
        {
            if (!CanInsert(item)) return false;
            GetBuffer(item).Insert();
            return true;
        }

        public int GetConsumeAmount(Item item)
        {
            return _consumeAmount[item];
        }

        public ItemBuffer GetBuffer(Item item)
        {
            foreach (ItemBuffer b in _buffers)
            {
                if (b.Item == item) return b;
            }
            throw new System.ArgumentException($"Item {item} not contained in buffers");
        }

        public void AddItem(int bufferSize, int consumeAmount, Item item)
        {
            _consumeAmount[item] = consumeAmount;
            _buffers.Add(new ItemBuffer(bufferSize, item));
        }

        private bool _canConsume()
        {
            foreach (ItemBuffer b in _buffers)
            {
                if (_consumeAmount[b.Item] > b.Buffer) return false;
            }
            return true;
        }

        public bool Consume()
        {
            if (!CanConsume) return false;
            foreach (ItemBuffer b in _buffers)
            {
                b.Buffer -= _consumeAmount[b.Item];
            }
            return true;
        }
    }
}
