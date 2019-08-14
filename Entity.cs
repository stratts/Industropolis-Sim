using System;
using Godot;

public abstract class Entity {
    public ref TilePos Pos => ref _pos;
    private TilePos _pos;
    protected Map map;

    public Entity(Map map, int x, int y) {
        this._pos = new TilePos(x, y);
        this.map = map;
    }

    public virtual void Update(float elapsedTime) {

    }
}

public class Hauler : Entity {
    private TilePos source;
    private TilePos dest;
    private float timeSinceMove = 0;
    private float moveInterval = 1;
    private TilePos target;
 
    public int Inventory { get; set; } = 0;
    public int MaxInventory { get; set; } = 5;
    public Boolean Hauling { get; set; } = false;

    public Hauler(Map map, int x, int y) : base(map, x, y) {

    }

    public void Haul(TilePos source, TilePos dest) {
        if (map.GetBuilding(source.X, source.Y) == null ||
            map.GetBuilding(dest.X, dest.Y) == null) {
                GD.Print("Not a building!");
                return;
            }

        GD.Print("Hauling!");

        this.source = source;
        this.dest = dest;
        Hauling = true;
        timeSinceMove = 0;
    }

    private bool Pickup(Building building) {
        if (building.Storage <= 0) return false;
        
        Inventory++;
        building.Storage--;
        return true;
    }

    private bool Dropoff(Building building) {
        if (Inventory <= 0) return false;

        Inventory--;
        building.Storage++;
        return true;
    }

    public override void Update(float elapsedTime) {
        if (!Hauling) return;

        timeSinceMove += elapsedTime;

         if (Pos == dest || Pos == source) {
                var building = map.GetBuilding(Pos.X, Pos.Y);
                if (Pos == dest) {  
                    if (Dropoff(building)) return;
                }
                else if (Pos == source) {
                    if (Pickup(building)) return;
                }   
        }

        if (timeSinceMove >= moveInterval) {
            timeSinceMove = 0;

            if (Inventory > 0) target = dest;
            else target = source;
           
            if (Pos.X < target.X) Pos.X += 1;
            else if (Pos.X > target.X) Pos.X -= 1;
            if (Pos.Y < target.Y) Pos.Y += 1;
            else if (Pos.Y > target.Y) Pos.Y -= 1;
        }
    }
}