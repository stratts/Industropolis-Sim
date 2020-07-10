using System;
using System.Collections.Generic;

// Represents an entity that exists on the map
namespace Industropolis.Sim
{
    public class MapObject
    {
        private List<MapObject> _links = new List<MapObject>();

        public event Action? Removed;

        public virtual void Remove()
        {
            Removed?.Invoke();
        }

        public void AddLink(MapObject obj) => _links.Add(obj);

        public void RemoveLink(MapObject obj) => _links.Remove(obj);

        public T GetLink<T>() where T : MapObject
        {
            foreach (var link in _links)
            {
                if (link is T obj) return obj;
            }
            throw new ArgumentException($"{this} does not have a link to object of type {nameof(T)}");
        }
    }
}
