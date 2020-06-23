using System;
using System.Collections.Generic;

public interface MapInfo
{
    int Width { get; }
    int Height { get; }
    Building? GetBuilding(IntVector pos);
    //PopulationInfo Population { get; }
    bool HasResource(Item item, int amount);
    void GetResource(Item item, int amount);
    Tile GetTile(IntVector pos);
    int CurrentMoney { get; set; }
}

public class Map : MapInfo, IPathManager<PathNode, Path>
{
    private Tile[,] tiles;
    private List<Building> buildings;
    private List<Route> routes;
    private List<Path> paths;
    private List<PathNode> nodes;
    private int _currentMoney = 0;
    private PathBuilder _pathBuilder;

    public IReadOnlyList<Building> Buildings => buildings;
    public List<Vehicle> Vehicles = new List<Vehicle>();
    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);

    //public PopulationInfo Population { get; } = new PopulationInfo();
    public int CurrentMoney
    {
        get
        {
            return _currentMoney;
        }
        set
        {
            _currentMoney = value;
            if (_currentMoney < 0) _currentMoney = 0;
        }
    }

    public event Action<Path>? PathAdded;
    public event Action<PathNode>? PathNodeAdded;
    public event Action<Building>? BuildingAdded;
    public event Action<Route>? RouteAdded;
    public event Action<Vehicle>? VehicleAdded;

    public Map()
    {
        routes = new List<Route>();
        buildings = new List<Building>();
        paths = new List<Path>();
        nodes = new List<PathNode>();

        int size = 50;
        tiles = MapGenerator.GenerateTiles(size, size, 1);
        _pathBuilder = new PathBuilder(this);
    }

    public Building CreateBuilding(BuildingType type, IntVector pos)
    {
        switch (type)
        {
            case BuildingType.Workshop: return new Workshop();
            //case BuildingType.House: building = new House(this.Population); break;
            case BuildingType.Mine: return new Mine(this, pos);
            case BuildingType.Farm: return new Farm(this, pos);
            case BuildingType.TestConsumer: return new TestConsumer();
            case BuildingType.TestProducer: return new TestProducer();
            default: return new TestProducer();
        }
    }

    public bool CanBuild(Building building, IntVector pos)
    {
        for (int x = 0; x < building.Width; x++)
        {
            for (int y = 0; y < building.Height; y++)
            {
                var p = new IntVector(pos.X + x, pos.Y + y);
                if (GetPath(p) != null) return false;
                if (GetNode(p) != null) return false;
                if (GetBuilding(p) != null) return false;
            }
        }

        return true;
    }

    public void AddBuilding(Building building, IntVector pos)
    {
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
        if (CurrentMoney < building.Cost)
        {
            Godot.GD.Print($"Not enough money to build {building.GetType().Name}");
            return;
        }
        else
        {
            CurrentMoney -= building.Cost;
        }
        building.Pos = pos;
        if (!CanBuild(building, pos))
        {
            Godot.GD.Print("Cannot build here");
            return;
        }
        building.Pos = pos;
        buildings.Add(building);
        if (building.HasEntrance) _pathBuilder.ConnectBuilding(building);
        BuildingAdded?.Invoke(building);
    }

    public void AddVehicle(Vehicle vehicle)
    {
        Vehicles.Add(vehicle);
        vehicle.Removed += () => Vehicles.Remove(vehicle);
        VehicleAdded?.Invoke(vehicle);
    }

    public Building? GetBuilding(IntVector pos)
    {
        foreach (var b in buildings)
        {
            foreach (var t in GetBuildingTiles(b))
            {
                if (t == pos) return b;
            }
        }
        return null;
    }

    public IEnumerable<IntVector> GetBuildingTiles(Building building)
    {
        for (int x = 0; x < building.Width; x++)
        {
            for (int y = 0; y < building.Height; y++)
            {
                yield return building.Pos + new IntVector(x, y);
            }
        }
    }

    public void RemoveBuilding(Building building)
    {
        building.Remove();
        buildings.Remove(building);
        if (building.HasEntrance) _pathBuilder.DisconnectBuilding(building);
    }

    public void Update(float delta)
    {
        foreach (Route r in routes) r.Update();
        foreach (Building b in buildings) b.Update(delta);
        foreach (Vehicle v in Vehicles) v.Update(delta);
    }

    public Route AddRoute(BuildingNode start, BuildingNode dest, Item item)
    {
        var route = new Route(this, start, dest);
        route.Item = item;
        route.SourceOutput = start.Building.Output;
        route.DestInput = start.Building.Input;
        route.Pathfind();
        routes.Add(route);
        RouteAdded?.Invoke(route);

        return route;
    }

    public Route? GetRoute(IntVector pos)
    {
        /*foreach (Route r in routes) {
            foreach (PathNode t in r.Path) {
                if (t == pos) return r;
            }
        }*/

        return null;
    }

    public void RemoveRoute(Route route)
    {
        routes.Remove(route);
        route.Remove();
    }

    public PathNode? GetNode(IntVector pos)
    {
        foreach (PathNode node in nodes)
        {
            if (node.Pos == pos) return node;
        }

        return null;
    }

    public void AddNode(PathNode node)
    {
        nodes.Add(node);
        PathNodeAdded?.Invoke(node);
    }

    public void RemoveNode(PathNode node)
    {
        var paths = new List<Path>(node.Connections.Values);
        foreach (Path path in paths)
        {
            NodeUtils.Disconnect(path.Source, path.Dest);
            RemovePath(path);
        }
        nodes.Remove(node);
        node.Remove();
    }

    public void AddPath(Path path)
    {
        this.paths.Add(path);
        path.PathSplit += () =>
        {
            RemovePath(path);
        };
        PathAdded?.Invoke(path);
    }

    public Path? GetPath(IntVector pos)
    {
        if (GetNode(pos) == null)
        {
            foreach (Path path in GetPaths(pos)) return path;
        }
        return null;
    }

    public IEnumerable<Path> GetPaths(IntVector pos)
    {
        foreach (Path path in paths)
        {
            if (path.OnPath(pos)) yield return path;
        }
    }

    public void RemovePath(Path path)
    {
        this.paths.Remove(path);
        path.Remove();
    }

    public void DeletePathSegment(IntVector pos) => _pathBuilder.DeletePathSegment(pos);

    public int GetResourceAmount(Item item)
    {
        foreach (Building b in buildings)
        {
            if (b is Stockpile s)
            {
                if (s.Output == null) return 0;
                if (s.Output.Has(item))
                {
                    return s.Output.AmountOf(item);
                }
            }
        }

        return 0;
    }

    public bool HasResource(Item item, int amount)
    {
        foreach (Building b in buildings)
        {
            if (b is Stockpile s)
            {
                if (s.HasItem(item, amount)) return true;
            }
        }
        return false;
    }

    public void GetResource(Item item, int amount)
    {
        foreach (Building b in buildings)
        {
            if (b is Stockpile s)
            {
                if (s.HasItem(item, amount))
                {
                    s.RemoveItem(item, amount);
                    return;
                }
            }
        }
    }

    public Tile GetTile(IntVector pos)
    {
        int x = pos.X;
        int y = pos.Y;

        if (x < 0 || y < 0 || x >= Width || y >= Height) throw new ArgumentException("Invalid tile");
        return tiles[pos.X, pos.Y];
    }

    public void CreateResourcePatch(int x, int y, int size, Item resource, int amount)
    {
        for (int i = x; i < x + size; i++)
        {
            for (int j = y; j < y + size; j++)
            {
                Tile? t = GetTile(new IntVector(i, j));
                if (t != null)
                {
                    t.Resource = resource;
                    t.ResourceCount = amount;
                }
            }
        }
    }
}
