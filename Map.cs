using System;
using System.Collections.Generic;

public interface MapInfo {
    int Width { get; }
    int Height { get; }
    Building GetBuilding(int x, int y);
    IEnumerable<T> GetEntities<T>() where T: Entity;
    void AddEntity(Entity entity);
    void RemoveEntity(Entity entity);
    PopulationInfo Population { get; }
    bool HasResource(Item item, int amount);
    void GetResource(Item item, int amount);
    Tile GetTile(TilePos pos);
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

    public List<Entity> Entities => entities;
    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);

    public PopulationInfo Population { get; } = new PopulationInfo();

    public event MapChangedEvent MapChanged;

    public Map() {
        entities = new List<Entity>();
        routes = new List<Route>();
        buildings = new List<Building>();

        int size = 50;
        tiles = new Tile[size,size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                tiles[i,j] = new Tile();
            }
        }     
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

    public void AddBuilding(Building building, TilePos pos) {
        if (GetBuilding(pos) != null) return;
        if (building.RequiredResources != null) {
            foreach (var resource in building.RequiredResources) {
                if (!HasResource(resource.Key, resource.Value)) {
                    Godot.GD.Print($"Not enough resources to build {building.GetType().Name}");
                    return;
                }
            }
            foreach (var resource in building.RequiredResources) {
                GetResource(resource.Key, resource.Value);
            }
        }
        tiles[pos.X, pos.Y].Building = building;
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

    public void RemoveBuilding(Building building) {
        building.Remove();
        tiles[building.Pos.X, building.Pos.Y].Building = null;
    }

    public void Update(float delta) {
        foreach (Entity e in entities) e.Update(delta);
        foreach (Building b in buildings) b.Update(delta);
    }

    public Route AddRoute(TilePos start, TilePos dest, Item item) {
        var route = new Route(this, start, dest);
        route.Item = item;
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
}
