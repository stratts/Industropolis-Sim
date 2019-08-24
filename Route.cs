using System;
using System.Collections.Generic;

public class Route : MapObject {
    private List<TilePos> _path;
    
    public IReadOnlyList<TilePos> Path => _path;
    public TilePos Source { get; set; }
    public TilePos Dest { get; set; }

    public IDirectOutput SourceOutput { get; set; }
    public IDirectInput DestInput { get; set; }

    public Item Item { get; set; } = Item.None;
    public List<Hauler> Haulers { get; }= new List<Hauler>();

    public int NumHaulers => Haulers.Count;

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

    public void AddHauler() {
        if (!MapInfo.Population.Use()) return;
        var hauler = new Hauler(MapInfo, Source.X, Source.Y);
        hauler.Route = this;
        hauler.Item = Item;
        Haulers.Add(hauler);
        hauler.Haul();
        MapInfo.AddEntity(hauler);
    }

    public void RemoveHauler() {
        if (Haulers.Count == 0) return;
        Hauler h = Haulers[0];
        Haulers.RemoveAt(0);
        h.Route = null;
        MapInfo.RemoveEntity(h);
        MapInfo.Population.Free();
    }
}