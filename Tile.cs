using System;
using System.Collections.Generic;

public class Tile
{
    private int _resourceCount = 0;

    public event Action? ResourceExhausted;

    public int Nutrients = 0;
    public Item Resource { get; set; } = Item.None;
    public int ResourceCount
    {
        get => _resourceCount;
        set
        {
            if (value <= 0) ResourceExhausted?.Invoke();
            _resourceCount = value;
        }
    }

    public float SpeedMultiplier => 1;
    //public List<Entity> Entities { get; set; } = new List<Entity>();
}