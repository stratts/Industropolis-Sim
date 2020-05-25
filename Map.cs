using System;
using System.Collections.Generic;

public interface MapInfo {
    int Width { get; }
    int Height { get; }
    Building GetBuilding(int x, int y);
    Building GetBuilding(IntVector pos);
    IEnumerable<T> GetEntities<T>() where T: Entity;
    void AddEntity(Entity entity);
    void RemoveEntity(Entity entity);
    //PopulationInfo Population { get; }
    bool HasResource(Item item, int amount);
    void GetResource(Item item, int amount);
    Tile GetTile(IntVector pos);
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
    private List<PathNode> nodes;
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
    public event Action<PathNode> PathNodeAdded;

    public Map() {
        entities = new List<Entity>();
        routes = new List<Route>();
        buildings = new List<Building>();
        paths = new List<Path>();
        nodes = new List<PathNode>();

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

    public Entity GetEntity(IntVector pos) {
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

    public void CreateBuilding(BuildingType type, IntVector pos) {
        Building building = null;
        switch (type) {
            case BuildingType.Workshop: building = new Workshop(); break;
            //case BuildingType.House: building = new House(this.Population); break;
            case BuildingType.Mine: building = new Mine(this, pos); break;
            case BuildingType.Farm: building = new Farm(this, pos); break;
        }
        AddBuilding(building, pos);
    }

    public bool CanBuild(Building building, IntVector pos) {
        for (int x = 0; x < building.Width; x++) {
            for (int y = 0; y < building.Height; y++) {
                var p = new IntVector(pos.X + x, pos.Y + y);
                if (GetPath(p) != null) return false;
                if (GetNode(p) != null) return false;
                if (GetBuilding(p) != null) return false;
            }
        }

        return true;
    }

    public void AddBuilding(Building building, IntVector pos) {
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
        if (!CanBuild(building, pos)) {
            Godot.GD.Print("Cannot build here");
            return;
        }
        foreach (var tile in GetBuildingTiles(building)) {
            tiles[tile.X, tile.Y].Building = building;
        }
        building.Pos = pos;
        buildings.Add(building);
        if (building.HasEntrance) ConnectBuilding(building);
        if (MapChanged != null) {
            MapChanged(this, new MapChangedEventArgs { Building = building });
        }
    }

    public void AddBuilding(Building building, int x, int y) {
        AddBuilding(building, new IntVector(x, y));
    }

    public Building GetBuilding(int x, int y) {
        return tiles[x, y].Building;
    }

    public Building GetBuilding(IntVector pos) {
        return tiles[pos.X, pos.Y].Building;
    }

    public IEnumerable<IntVector> GetBuildingTiles(Building building) {
        for (int x = 0; x < building.Width; x++) {
            for (int y = 0; y < building.Height; y++) {
                yield return building.Pos + new IntVector(x, y);
            }
        }
    }

    public void ConnectBuilding(Building building) {
        var entrance = building.Entrance;
        if (GetPath(entrance.ConnectionPos) != null || GetNode(entrance.ConnectionPos) != null) {
            var node = new BuildingNode(entrance.Pos, building);
            entrance.Connect(node);
            AddNode(entrance.Node);
            BuildPath<Path>(entrance.Pos, entrance.ConnectionPos);
        }
    }

    public void DisconnectBuilding(Building building) {
        PathNode n = building.Entrance.Node;
        PathNode pathCon = GetNode(n.Pos + new IntVector(0, 1));
        RemoveNode(n);
        if (pathCon != null && pathCon.Connections.Count == 2) TryMergeNode(pathCon);
        building.Entrance.Disconnect();
    }

    public void RemoveBuilding(Building building) {
        building.Remove();
        buildings.Remove(building);
        foreach (var pos in GetBuildingTiles(building)) {
            tiles[pos.X, pos.Y].Building = null;
        }
        if (building.HasEntrance) DisconnectBuilding(building);
    }

    public void Update(float delta) {
        foreach (Entity e in entities) e.Update(delta);
        foreach (Building b in buildings) b.Update(delta);
    }

    public Route AddRoute(IntVector start, IntVector dest, Item item) {
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

    public Route GetRoute(IntVector pos) {
        foreach (Route r in routes) {
            foreach (IntVector t in r.Path) {
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

    public PathNode GetNode(IntVector pos) {
        foreach (PathNode node in nodes) {
            if (node.Pos == pos) return node;
        }

        return null;
    }

    public void AddNode(PathNode node) {
        nodes.Add(node);
        PathNodeAdded?.Invoke(node);
    }

    private PathNode AddPathNode(IntVector pos) {
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
            AddNode(n);
        }
        return n;
    }

    public void RemoveNode(PathNode node) {
        var paths = new List<Path>(node.Connections.Values);
        foreach (Path path in paths) {
            path.Disconnect();
            RemovePath(path);
        } 
        nodes.Remove(node);
        node.Remove();
    }

    public void BuildPath<T>(IntVector source, IntVector dest) where T : Path, new() {
        IntVector inc = source.Direction(dest);
        IntVector prev = source;
        IntVector cur = source;
        bool onPath = false;
        var toConnect = new List<Building>();
     
        while (cur != dest) {
            cur += inc;
            Path p = GetPath(cur);
            PathNode n = GetNode(cur);
            if (p != null && p.Direction.IsParallelTo(inc)) {
                onPath = true;
            }
            else if (onPath && n != null) {
                onPath = false;
                prev = cur;
            }
            else if (!onPath && (cur == dest || p != null || n != null)) {
                BuildPathSegment<T>(prev, cur);
                prev = cur;
            }     
            foreach (var building in buildings) {
                if (building.HasEntrance && building.Entrance.ConnectionPos == cur
                    && !building.Entrance.Connected) {
                    toConnect.Add(building);
                }
            }
        }

        foreach (var building in toConnect) ConnectBuilding(building);
    }

    public void BuildPathSegment<T>(IntVector source, IntVector dest) where T : Path, new() {
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

        if (!path1.Direction.IsParallelTo(path2.Direction)) return;

        Path newPath = Path.Merge(path1, path2);
        path1.Disconnect();
        path2.Disconnect();
        RemovePath(path1);
        RemovePath(path2);
        newPath.Connect();
        AddPath(newPath);
        RemoveNode(node);
    }

    public void AddPath(Path path) {
        this.paths.Add(path);
        path.PathSplit += () => {
            RemovePath(path);
        };
        PathAdded?.Invoke(path);
    }

    public Path GetPath(IntVector pos) {
        foreach (Path path in paths) {
            if (path.OnPath(pos) && GetNode(pos) == null) {
                return path;
            }        
        }
        return null;
    }

    public void DeletePathSegment(IntVector pos) {
        Path p = GetPath(pos);
        PathNode n = GetNode(pos);
        if (p == null && n == null) {
            throw new ArgumentException($"Position {pos} does not contain a path to delete");
        }
        if (p != null) {
            AddPathNode(pos + p.Direction);
            AddPathNode(pos - p.Direction);
            var newPath = GetPath(pos);
            newPath.Disconnect();
            RemovePath(newPath);
        }
        else if (n != null) {
            var connections = new List<PathNode>(n.Connections.Keys);
            foreach (PathNode connection in connections) {
                AddPathNode(n.Pos + n.Pos.Direction(connection.Pos));
            }
            RemoveNode(n);
        }
    }

    public void RemovePath(Path path) {
        this.paths.Remove(path);
        path.Remove();
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

    public Tile GetTile(IntVector pos) {
        int x = pos.X;
        int y = pos.Y;

        if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
        return tiles[pos.X, pos.Y];
    }

    public void CreateResourcePatch(int x, int y, int size, Item resource, int amount) {
        for (int i = x; i < x + size; i++) {
            for (int j = y; j < y + size; j++) {
                Tile t = GetTile(new IntVector(i, j));
                if (t != null) {
                    t.Resource = resource;
                    t.ResourceCount = amount;
                }
            }
        }
    }
}
