using System;
using System.Collections.Generic;

public class Route : MapObject {
    public List<TilePos> _path;
    
    public IReadOnlyList<TilePos> Path => _path;
    public TilePos Source { get; set; }
    public TilePos Dest { get; set; }

    public enum Direction { Forwards, Backwards };

    public Route(MapInfo info, TilePos src, TilePos dest) : base(info) {
        _path = new List<TilePos>();
        Source = src;
        Dest = dest;
    }

    public TilePos Next(TilePos current, Direction direction) {
        int shift = 0;
        var curr = _path.IndexOf(current);

        if (direction == Direction.Forwards) shift = 1;
        else if (direction == Direction.Backwards) shift = -1;

        var newPos = curr + shift;
        if (newPos >= 0 && newPos < _path.Count) return _path[newPos];
        else return current;
    }

    public void Pathfind() {
        _path.Clear();

        TilePos curr = Source;

        while (curr != Dest) {
            _path.Add(curr);

            if (Dest.X > curr.X) curr.X += 1;
            else if (Dest.X < curr.X) curr.X -= 1;
            if (Dest.Y > curr.Y) curr.Y += 1;
            else if (Dest.Y < curr.Y) curr.Y -= 1;
        }

        _path.Add(curr);
    }
}