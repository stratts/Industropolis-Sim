using System;
using System.Collections.Generic;

public class Route : MapObject
{
    private List<PathNode> _path = new List<PathNode>();

    public IReadOnlyList<PathNode> Path => _path;
    public PathNode Source { get; set; }
    public PathNode Dest { get; set; }
    public MapInfo MapInfo { get; private set; }

    public IDirectOutput? SourceOutput { get; set; }
    public IDirectInput? DestInput { get; set; }

    private IPathfinder<PathNode> _pathfinder;

    private Item _item = Item.None;
    public Item Item
    {
        get => _item;
        set
        {
            _item = value;
            //foreach (var h in Haulers) h.Item = value;
        }
    }
    //public List<Hauler> Haulers { get; } = new List<Hauler>();

    //public int NumHaulers => Haulers.Count;

    public enum Direction { Forwards, Backwards };

    public Route(MapInfo info, PathNode src, PathNode dest)
    {
        MapInfo = info;
        Source = src;
        Dest = dest;
        _pathfinder = new AStarPathfinder<PathNode>();
    }

    public PathNode Next(PathNode current, Direction direction)
    {
        int shift = 0;
        var curr = _path.IndexOf(current);

        if (direction == Direction.Forwards) shift = 1;
        else if (direction == Direction.Backwards) shift = -1;

        var newPos = curr + shift;
        if (newPos >= 0 && newPos < _path.Count) return _path[newPos];
        else return current;
    }

    public void Pathfind()
    {
        var path = _pathfinder.FindPath(new PathGraph(), Source, Dest);
        if (path != null) _path = path;
        else
        {
            Godot.GD.Print("No path found! :(");
            _path = new List<PathNode>();
            return;
        }
    }

    public void AddHauler()
    {
        //if (!MapInfo.Population.Use()) return;
        /*var hauler = new Hauler(MapInfo, Source.X, Source.Y);
        hauler.Route = this;
        hauler.Item = Item;
        Haulers.Add(hauler);
        hauler.Haul();
        MapInfo.AddEntity(hauler);*/
    }

    public void RemoveHauler()
    {
        /*if (Haulers.Count == 0) return;
         Hauler h = Haulers[0];
         Haulers.RemoveAt(0);
         h.Route = null;
         MapInfo.RemoveEntity(h);*/
        //MapInfo.Population.Free();
    }
}