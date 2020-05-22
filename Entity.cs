using System;
using Godot;

public abstract class Entity : MapObject {
    public ref IntVector Pos => ref _pos;
    private IntVector _pos;

    public ref IntVector NextPos => ref _nextPos;
    private IntVector _nextPos;

    public float LastMoveTime { get; set; } = 0;
    public float NextMoveTime { get; set; } = 0;
    
    private static int id = 0;
    public int Id { get; set; }

    public Entity(MapInfo info, int x, int y) : base(info) {
        this._pos = new IntVector(x, y);
        id++;
        Id = id;
    }

    public virtual void Update(float elapsedTime) {

    }
}