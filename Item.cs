using System.Collections;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public enum Item : short
    {
        None,
        Stone,
        Wood,
        Log,
        Food,
        IronOre,
        CopperOre,
        Iron,
        Copper
    }

    public class RequiredResources : IEnumerable<(Item item, int amount)>
    {
        private (Item item, int amount)[] _requirements;

        public static RequiredResources None => new RequiredResources();

        public int Count => _requirements.Length;

        public RequiredResources(params (Item item, int amount)[] requirements)
        {
            _requirements = new (Item, int)[requirements.Length];
            requirements.CopyTo(_requirements, 0);
        }

        public IEnumerator<(Item item, int amount)> GetEnumerator() => ((IEnumerable<(Item, int)>)_requirements).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _requirements.GetEnumerator();

        public static implicit operator RequiredResources((Item, int)[] res) => new RequiredResources(res);
    }
}
