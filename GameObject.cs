using System;

public class GameObject {
    public event EventHandler Removed;

    public void Remove() {
        if (Removed != null) Removed(this, null);
    }
}