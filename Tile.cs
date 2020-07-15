using System;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public struct Tile
    {
        private short _resourceCount;

        public event Action? ResourceExhausted;

        public short Nutrients;
        public Item Resource;
        public short ResourceCount
        {
            get => _resourceCount;
            set
            {
                if (value <= 0) ResourceExhausted?.Invoke();
                _resourceCount = value;
            }
        }
        //public List<Entity> Entities { get; set; } = new List<Entity>();
    }
}
