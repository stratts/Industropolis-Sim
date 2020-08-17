using System;

namespace Industropolis.Sim.Buildings
{
    public class Smelter : ProductionBuilding
    {
        private class ItemDecider : IDirectInput
        {
            private Action<Item> _onInsert;

            public bool Accepts(Item item) => item == Item.IronOre || item == Item.CopperOre;

            public bool CanInsert(Item item) => item == Item.IronOre || item == Item.CopperOre;

            public bool Insert(Item item)
            {
                _onInsert.Invoke(item);
                return true;
            }

            public ItemDecider(Action<Item> onInsert) => _onInsert = onInsert;
        }

        public Smelter()
        {
            Type = BuildingType.Smelter;
            Input = new ItemDecider(SetItem);
            ProcessingTime = 2;
            Width = 2;
            Height = 2;
            Entrance = new BuildingEntrance(this, (1, 1), (1, 2), PathType.Driveway);
        }

        private void SetItem(Item item)
        {
            Item outputItem = item switch
            {
                Item.IronOre => Item.Iron,
                Item.CopperOre => Item.Copper,
                _ => throw new ArgumentException("Invalid smelter input")
            };

            var input = new DirectConsumer(10, 5, item);
            var output = new DirectProducer(10, 1, outputItem);

            Input = input;
            Consumer = input;
            Output = output;
            Producer = output;
        }
    }
}