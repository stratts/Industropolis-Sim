using System;

public class GameObject {
    public event Action Removed;

    public void Remove() {
        Removed?.Invoke();
    }
}