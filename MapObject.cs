using System;

// Represents an entity that exists on the map
public class MapObject
{
    public event Action? Removed;

    public void Remove()
    {
        Removed?.Invoke();
    }
}
