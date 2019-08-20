using System;
using Godot;


public class Hauler : Entity {
    public Route Route { get; set; } = null;
    private float timeSinceMove = 0;
    private float moveInterval = 1;
    private bool followingRoute = false;
 
    public int Inventory { get; set; } = 0;
    public int MaxInventory { get; set; } = 5;
    public Item Item { get; set; }
    public bool Hauling { get; set; } = false;

    public Hauler(MapInfo map, int x, int y) : base(map, x, y) {

    }

    public void Haul() {
        Hauling = true;
        timeSinceMove = 0;
    }

    private bool Pickup(DirectOutput output) {
        if (Inventory >= MaxInventory || !output.CanRemove) return false;
        output.Remove();
        Inventory++;
        return true;
    }

    private bool Dropoff(DirectInput input) {
        if (Inventory <= 0 || !input.CanInsert(Item)) return false;
        input.Insert(Item);
        Inventory--;
        return true;
    }

    public override void Update(float elapsedTime) {
        if (!Hauling || Route == null) return;

        timeSinceMove += elapsedTime;

         if (Pos == Route.Dest || Pos == Route.Source) {
                var building = MapInfo.GetBuilding(Pos.X, Pos.Y);
                if (Pos == Route.Dest) {  
                    if (Dropoff((DirectInput)building.Input)) return;
                }
                else if (Pos == Route.Source) {
                    if (Pickup((DirectOutput)building.Output)) return;
                }   
        }

        if (timeSinceMove >= moveInterval) {
            timeSinceMove = 0;
            TilePos target;

            if (followingRoute) {
                if (Inventory > 0) {
                    target = Route.Next(this.Pos, Route.Direction.Forwards);
                }
                else target = Route.Next(this.Pos, Route.Direction.Backwards);
                GD.Print(target.X + " " + target.Y);
            }
            else {
                target = Route.Source;
                if (this.Pos == Route.Source) followingRoute = true;
            }
           
            if (Pos.X < target.X) Pos.X += 1;
            else if (Pos.X > target.X) Pos.X -= 1;
            if (Pos.Y < target.Y) Pos.Y += 1;
            else if (Pos.Y > target.Y) Pos.Y -= 1;
        }
    }
}