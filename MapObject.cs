using System;

// Represents an entity that exists on the map
namespace Industropolis.Sim
{
    public class MapObject
    {
        public event Action? Removed;

        public virtual void Remove()
        {
            Removed?.Invoke();
        }
    }
}
