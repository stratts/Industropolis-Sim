using System;
using System.Collections.Generic;


public class Map {
    private Tile[,] tiles;
    private List<Entity> entities;
    private IDisplay display;

    public List<Entity> Entities => entities;

    public Map(IDisplay display) {
        this.display = display;
        entities = new List<Entity>();

        int size = 10;
        tiles = new Tile[size,size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                tiles[i,j] = new Tile();
            }
        }

        var hauler = new Hauler(this, 0, 0);

        var building1 = new Building();
        var building2 = new Building();
        building2.Storage = 0;
        AddBuilding(building1, 2, 1);
        AddBuilding(building2, 6, 1);

        hauler.Haul(new TilePos(2, 1), new TilePos(6, 1));
        AddEntity(hauler);
    }

    public void AddEntity(Entity entity) {
        entities.Add(entity);
        display.AddEntity(entity);
    }

    public void AddBuilding(Building building, int x, int y) {
        tiles[x, y].Building = building;
        display.AddBuilding(building, x, y);
    }

    public Building GetBuilding(int x, int y) {
        return tiles[x, y].Building;
    }
}
