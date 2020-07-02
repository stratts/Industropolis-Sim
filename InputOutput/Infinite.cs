

using System.Collections.Generic;

namespace Industropolis.Sim
{
    public class InfiniteDirectOutput : IDirectOutput
    {
        public IReadOnlyList<Item> Items => _items;
        private List<Item> _items;
        private Item _item;

        public InfiniteDirectOutput(Item item)
        {
            _item = item;
            _items = new List<Item>();
            _items.Add(item);
        }

        public int AmountOf(Item item) => item == _item ? 999 : 0;
        public bool CanRemove(Item item) => item == _item;
        public bool Has(Item item) => item == _item;
        public bool Remove(Item item) => item == _item;
    }

    public class InfiniteDirectInput : IDirectInput
    {
        public bool Accepts(Item item) => true;
        public bool CanInsert(Item item) => true;
        public bool Insert(Item item) => true;
    }
}
