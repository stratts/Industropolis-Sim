using System.Collections.Generic;

public class Tile {
    public Building Building { get; set; } = null;
    public Item Resource { get; set; } = Item.None;
    public int ResourceCount { get; set; } = 0;
    //public List<Entity> Entities { get; set; } = new List<Entity>();
}