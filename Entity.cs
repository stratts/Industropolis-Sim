using System;
using Godot;

public abstract class Entity : MapObject {
    public ref TilePos Pos => ref _pos;
    private TilePos _pos;

    public Entity(MapInfo info, int x, int y) : base(info) {
        this._pos = new TilePos(x, y);
    }

    public virtual void Update(float elapsedTime) {

    }
}