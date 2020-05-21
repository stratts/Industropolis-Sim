using System;
using System.Collections.Generic;

public interface MapInfo {
    int Width { get; }
    int Height { get; }
    Building GetBuilding(int x, int y);
    Building GetBuilding(TilePos pos);
    IEnumerable<T> GetEntities<T>() where T: Entity;
    void AddEntity(Entity entity);
    void RemoveEntity(Entity entity);
    //PopulationInfo Population { get; }
    bool HasResource(Item item, int amount);
    void GetResource(Item item, int amount);
    Tile GetTile(TilePos pos);
    int CurrentMoney { get; set; }
}

public delegate void MapChangedEvent(Map map, MapChangedEventArgs e);

public class MapChangedEventArgs : EventArgs {
    public Building Building { get; set; } = null;
    public Entity Entity { get; set; } = null;
    public Route Route { get; set; } = null;
}

public class Map : MapInfo {
    private Tile[,] tiles;
    private List<Entity> entities;
    private List<Building> buildings;
    private List<Route> routes;
    private List<Path> paths;
    private int _currentMoney = 0;

    public List<Entity> Entities => entities;
    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);

    //public PopulationInfo Population { get; } = new PopulationInfo();
    public int CurrentMoney { 
        get {
            return _currentMoney;
        }
        set {
            _currentMoney = value;
            if (_currentMoney < 0) _currentMoney = 0;
        }
    }

    public event MapChangedEvent MapChanged;

    public event Action<Path> PathAdded;
    public event Action<Path> PathRemoved;
    public event Action<PathNode> PathNodeAdded;

    public Map() {
        entities = new List<Entity>();
        routes = new List<Route>();
        buildings = new List<Building>();
        paths = new List<Path>();

        int size = 50;
        tiles = MapGenerator.GenerateTiles(size, size, 1);
    }

    public void AddEntity(Entity entity) {
        entities.Add(entity);
        if (MapChanged != null) {
            MapChanged(this, new MapChangedEventArgs { Entity = entity });
        }
    }

    public void RemoveEntity(Entity entity) {
        entity.Remove();
        entities.Remove(entity);
    }

    public Entity GetEntity(TilePos pos) {
        foreach (Entity e in entities) {
            if (e.Pos == pos) return e;
        }

        return null;
    }

    public IEnumerable<T> GetEntities<T>() where T : Entity {
        foreach (Entity e in entities) {
            if (e is T t) yield return t;
        }
    }

    public void CreateBuilding(BuildingType type, TilePos pos) {
        Building building = null;
        switch (type) {
            case BuildingType.Workshop: building = new Workshop(); break;
            //case BuildingType.House: building = new House(this.Population); break;
            case BuildingType.Mine: building = new Mine(this, pos); break;
            case BuildingType.Farm: building = new Farm(this, pos); break;
        }
        AddBuilding(building, pos);
    }

    public void AddBuilding(Building building, TilePos pos) {
        /*if (building.RequiredResources != null) {
            foreach (var resource in building.RequiredResources) {
                if (!HasResource(resource.Key, resource.Value)) {
                    Godot.GD.Print($"Not enough resources to build {building.GetType().Name}");
                    return;
                }
            }
            foreach (var resource in building.RequiredResources) {
                GetResource(resource.Key, resource.Value);
            }
        }*/
        if (CurrentMoney < building.Cost) {
             Godot.GD.Print($"Not enough money to build {building.GetType().Name}");
             return;
        }
        else {
            CurrentMoney -= building.Cost;
        }
        building.Pos = pos;
        foreach (var tile in GetBuildingTiles(building)) {
            if (GetBuilding(tile) != null) return;
        }
        foreach (var tile in GetBuildingTiles(building)) {
            tiles[tile.X, tile.Y].Building = building;
        }
        building.Pos = pos;
        buildings.Add(building);
        if (MapChanged != null) {
            MapChanged(this, new MapChangedEventArgs { Building = building });
        }
    }

    public void AddBuilding(Building building, int x, int y) {
        AddBuilding(building, new TilePos(x, y));
    }

    public Building GetBuilding(int x, int y) {
        return tiles[x, y].Building;
    }

    public Building GetBuilding(TilePos pos) {
        return tiles[pos.X, pos.Y].Building;
    }

    public IEnumerable<TilePos> GetBuildingTiles(Building building) {
        for (int x = 0; x < building.Width; x++) {
            for (int y = 0; y < building.Height; y++) {
                yield return building.Pos + new TilePos(x, y);
            }
        }
    }

    public void RemoveBuilding(Building building) {
        building.Remove();
        buildings.Remove(building);
        foreach (var pos in GetBuildingTiles(building)) {
            tiles[pos.X, pos.Y].Building = null;
        }
    }

    public void Update(float delta) {
        foreach (Entity e in entities) e.Update(delta);
        foreach (Building b in buildings) b.Update(delta);
    }

    public Route AddRoute(TilePos start, TilePos dest, Item item) {
        var route = new Route(this, start, dest);
        route.Item = item;
        route.SourceOutput = GetBuilding(start).Output;
        route.DestInput = GetBuilding(dest).Input;
        route.Pathfind();
        routes.Add(route);

        if (MapChanged != null) {
            MapChanged(this, new MapChangedEventArgs { Route = route });
        }
        
        return route;
    }

    public Route GetRoute(TilePos pos) {
        foreach (Route r in routes) {
            foreach (TilePos t in r.Path) {
                if (t == pos) return r;
            }
        }

        return null;
    }

    public void RemoveRoute(Route route) {
        routes.Remove(route);
        route.Remove();
        foreach (Entity e in entities) {
            if (e is Hauler h && h.Route == route) {
                h.Route = null;
            }
        }
    }

    public PathNode GetNode(TilePos pos) {
        foreach (var path in paths) {
            if (path.Source.Pos == pos) return path.Source;
            if (path.Dest.Pos == pos) return path.Dest;
        }

        return null;
    }

    private PathNode AddPathNode(TilePos pos) {
        PathNode n = GetNode(pos);
        if (n == null) {
            n = new PathNode(pos);
            Path p = GetPath(pos);
            if (p != null) {
                var (path1, path2) = Path.Split(p, n);
                path1.Connect();
                path2.Connect();
                AddPath(path1);
                AddPath(path2);
                RemovePath(p);
            }
            PathNodeAdded?.Invoke(n);
        }
        return n;
    }

    public void BuildPath<T>(TilePos source, TilePos dest) where T : Path, new() {
        var dir = source.Direction(dest);
        TilePos inc = new TilePos(dir.x, dir.y);
        TilePos prev = source;
        TilePos cur = source + inc;
     
        for (int i = 0; i < source.Distance(dest); i++) {
            if (cur == dest || GetPath(cur) != null || GetNode(cur) != null) {
                BuildPathSegment<T>(prev, cur);
                prev = cur;
            }
            cur += inc;
        }
    }

    public void BuildPathSegment<T>(TilePos source, TilePos dest) where T : Path, new() {
        PathNode s = AddPathNode(source);
        PathNode d = AddPathNode(dest);
        Path path = new T();
        path.SetNodes(s, d);
        path.Connect();
        AddPath(path);
        if (s.Connections.Count == 2) TryMergeNode(s);
        if (d.Connections.Count == 2) TryMergeNode(d);
    }

    private void TryMergeNode(PathNode node) {
        Path path1 = null;
        Path path2 = null;
        foreach (Path p in node.Connections.Values) {
            if (path1 == null) {
                path1 = p; continue;
            }
            else if (path2 == null) {
                path2 = p; continue;
            }
            else break;
        }

        if (!(path1.Direction == path2.Direction || 
            path1.Direction == -path2.Direction)) return;

        Path newPath = Path.Merge(path1, path2);
        path1.Disconnect();
        path2.Disconnect();
        RemovePath(path1);
        RemovePath(path2);
        newPath.Connect();
        AddPath(newPath);
        // Remove node
    }

    public void AddPath(Path path) {
        this.paths.Add(path);
        path.PathSplit += () => {
            RemovePath(path);
        };
        PathAdded?.Invoke(path);
    }

    public Path GetPath(TilePos pos) {
        foreach (Path path in paths) {
            if (path.OnPath(pos)) return path;
        }
        return null;
    }

    public void RemovePath(Path path) {
        this.paths.Remove(path);
        PathRemoved?.Invoke(path);
    }

    public int GetResourceAmount(Item item) {
        foreach (Building b in buildings) {
            if (b is Stockpile s) {
                if (s.Output.Has(item)) {
                    return s.Output.AmountOf(item);
                }
            }
        }

        return 0;
    }

    public bool HasResource(Item item, int amount) {
        foreach (Building b in buildings) {
            if (b is Stockpile s) {
                if (s.HasItem(item, amount)) return true;
            }
        }
        return false;
    }

    public void GetResource(Item item, int amount) {
        foreach (Building b in buildings) {
            if (b is Stockpile s) {
                if (s.HasItem(item, amount)) {
                    s.RemoveItem(item, amount);
                    return;
                }
            }
        }
    }

    public Tile GetTile(TilePos pos) {
        int x = pos.X;
        int y = pos.Y;

        if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
        return tiles[pos.X, pos.Y];
    }

    public void CreateResourcePatch(int x, int y, int size, Item resource, int amount) {
        for (int i = x; i < x + size; i++) {
            for (int j = y; j < y + size; j++) {
                Tile t = GetTile(new TilePos(i, j));
                if (t != null) {
                    t.Resource = resource;
                    t.ResourceCount = amount;
                }
            }
        }
    }
}
