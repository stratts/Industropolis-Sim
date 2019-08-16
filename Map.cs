using System;
using System.Collections.Generic;

public interface MapInfo {
    int Width { get; }
    int Height { get; }
    Building GetBuilding(int x, int y);
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

    public void AddBuilding(Building building, int x, int y) {
        tiles[x, y].Building = building;
        building.Pos = new TilePos(x, y);
        buildings.Add(building);
        if (MapChanged != null) {
            MapChanged(this, new MapChangedEventArgs { Building = building });
        }
    }

    public Building GetBuilding(int x, int y) {
        return tiles[x, y].Building;
    }

    public void RemoveBuilding(Building building) {
        building.Remove();
        tiles[building.Pos.X, building.Pos.X].Building = null;
    }

    public void Update(float delta) {
        foreach (Entity e in entities) e.Update(delta);
        foreach (Building b in buildings) b.Update(delta);
    }

    public Route AddRoute(TilePos start, TilePos dest) {
        var route = new Route(this, start, dest);
        route.Pathfind();
        routes.Add(route);

        foreach (Entity e in entities) {
            if (e is Hauler h && h.Route == null) {
                h.Route = route;
                h.Haul();
                break;
            }
        }

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
}
