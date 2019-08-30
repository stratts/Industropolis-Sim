using System;
using System.Collections.Generic;

public class Tile {
    private int _resourceCount = 0;

    public event EventHandler ResourceExhausted;
    public Building Building { get; set; } = null;
    public int Nutrients = 0;
    public Item Resource { get; set; } = Item.None;
    public int ResourceCount {
        get => _resourceCount;
        set {
            if (value <= 0) OnResourceExhausted();
            _resourceCount = value;
        }
    }

    private void OnResourceExhausted() {
        if (ResourceExhausted != null) ResourceExhausted(this, null);
    }
    //public List<Entity> Entities { get; set; } = new List<Entity>();
}