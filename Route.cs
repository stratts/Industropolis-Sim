using System;
using System.Collections.Generic;

public class Route : MapObject {
    private List<TilePos> _path;
    
    public IReadOnlyList<TilePos> Path => _path;
    public TilePos Source { get; set; }
    public TilePos Dest { get; set; }

    public Tile.SurfaceType SurfaceRestriction { get; set; } = Tile.SurfaceType.Base;

    public IDirectOutput SourceOutput { get; set; }
    public IDirectInput DestInput { get; set; }

    private Item _item = Item.None;
    public Item Item { 
        get => _item;
        set {
            _item = value;
            foreach (var h in Haulers) h.Item = value;
        }
    }
    public List<Hauler> Haulers { get; } = new List<Hauler>();

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

        var visited = new HashSet<TilePos>();
        var dist = new Dictionary<TilePos, float>();
        var destDist = new Dictionary<TilePos, float>();
        var queue = new List<TilePos>();

        queue.Add(Source);
        visited.Add(Source);
        dist[Source] = 0;

        while (!visited.Contains(Dest) && queue.Count > 0) {
            TilePos tile = queue[0];
            queue.RemoveAt(0);
            float tileDist = dist[tile];
            // Visit all neighbours
            foreach (TilePos neighbour in tile.Neighbours) {         
                if (!neighbour.WithinBounds(MapInfo.Width, MapInfo.Height)) continue;
                Tile t = MapInfo.GetTile(neighbour);
                var neighbourDist = tileDist + (tile.Distance(neighbour) / t.SpeedMultiplier);
                destDist[neighbour] = neighbour.Distance(Dest) / t.SpeedMultiplier;
                if (visited.Contains(neighbour) && dist.ContainsKey(neighbour) &&
                    neighbourDist >= dist[neighbour]) continue;
                visited.Add(neighbour);
                if (MapInfo.GetBuilding(neighbour) != null) continue;
                if (SurfaceRestriction != Tile.SurfaceType.Base &&
                    t.Surface != SurfaceRestriction) 
                    continue;
                dist[neighbour] = neighbourDist;
                queue.Add(neighbour);
            }
            // Sort queue using distance heuristic
            queue.Sort((a, b) => {
                float distA = destDist[a] + dist[a];
                float distB = destDist[b] + dist[b];
                if (distA > distB) return 1;
                else if (distA < distB) return -1;
                return 0;
            });
        }

        // Trace path back to source
        if (visited.Contains(Dest)) {
            TilePos tile = Dest;
            _path.Add(Dest);
            while (tile != Source) {
                float min = float.MaxValue;
                foreach (TilePos neighbour in tile.Neighbours) {
                    if (!dist.ContainsKey(neighbour)) continue;
                    if (dist[neighbour] < min) {
                        min = dist[neighbour];
                        tile = neighbour;
                    }
                }
                _path.Add(tile);
            }

            _path.Reverse();
        }
    }

    public void AddHauler() {
        //if (!MapInfo.Population.Use()) return;
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
        //MapInfo.Population.Free();
    }
}