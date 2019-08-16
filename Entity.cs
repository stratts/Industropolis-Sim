using System;
using Godot;

public abstract class Entity : MapObject {
    public ref TilePos Pos => ref _pos;
    private TilePos _pos;
    
    private static int id = 0;
    public int Id { get; set; }

    public Entity(MapInfo info, int x, int y) : base(info) {
        this._pos = new TilePos(x, y);
        id++;
        Id = id;
    }

    public virtual void Update(float elapsedTime) {

    }
}