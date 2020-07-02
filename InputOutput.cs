
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public interface IConsumer
    {
        bool CanConsume { get; }
        bool Consume();
    }

    public interface IDirectInput
    {
        bool Accepts(Item item);
        bool CanInsert(Item item);
        bool Insert(Item item);
    }

    public interface IProducer
    {
        bool CanProduce { get; }
        bool Produce();
    }

    public interface IDirectOutput
    {
        IReadOnlyList<Item> Items { get; }
        bool Has(Item item);
        int AmountOf(Item item);
        bool CanRemove(Item item);
        bool Remove(Item item);
    }

    public class InfiniteInput : IConsumer
    {
        public InfiniteInput(Item item)
        {
            Item = item;
        }

        public Item Item { get; set; }
        public bool CanConsume => true;
        public bool Consume() => true;
    }
}
