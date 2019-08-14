using System;
using System.Collections.Generic;

public interface MapInfo {
    int Width { get; }
    int Height { get; }
    Building GetBuilding(int x, int y);
}

public class Map : MapInfo {
    private Tile[,] tiles;
    private List<Entity> entities;

    public List<Entity> Entities => entities;
    public int Width => tiles.GetLength(0);
    public int Height => tiles.GetLength(1);

    public Map() {
        entities = new List<Entity>();

        int size = 10;
        tiles = new Tile[size,size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                tiles[i,j] = new Tile();
            }
        }     
    }

    public void AddEntity(Entity entity) {
        entities.Add(entity);
    }

    public void AddBuilding(Building building, int x, int y) {
        tiles[x, y].Building = building;
    }

    public Building GetBuilding(int x, int y) {
        return tiles[x, y].Building;
    }

    public void Update(float delta) {
        foreach (Entity e in entities) e.Update(delta);
    }
}
