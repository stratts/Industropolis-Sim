using System;
using System.IO;
using System.Buffers;
using System.Text.Json;

namespace Industropolis.Sim.SaveGame
{
    public static class TestSave
    {
        public static void Save(Map map)
        {
            Directory.CreateDirectory("savegame");
            Directory.CreateDirectory("savegame/chunks");
            Console.Write("Saving... ");
            var start = DateTime.Now;

            // Save paths
            using (var writer = new RowWriter("savegame/paths.csv", map.VehiclePaths.Paths.Count))
            {
                foreach (var path in map.VehiclePaths.Paths)
                {
                    if (path.PathType == PathType.Driveway || path.Fixed) continue;
                    writer.WriteRow(path.Source, path.Dest, path.PathType);
                }
            }

            // Save buildings
            using (var writer = new RowWriter("savegame/buildings.csv", map.Buildings.Count))
            {
                foreach (var building in map.Buildings)
                {
                    writer.StartRow();
                    writer.WriteFields(building.Pos, building.Type);
                    if (building is Workshop w) writer.WriteField(w.Recipe.Name);
                    if (building.Output is DirectProducer p) writer.WriteField(p.Buffer);
                    if (building.Input is DirectConsumer c)
                    {
                        foreach (var buffer in c.Buffers) writer.WriteFields(buffer.Item, buffer.Buffer);
                    }
                }
            }

            // Save chunks
            foreach (var chunk in map.Chunks)
            {
                using (var writer = new RowWriter($"savegame/chunks/chunk_{chunk.Pos.X}_{chunk.Pos.Y}.csv", chunk.Tiles.Length))
                {
                    foreach (var tile in chunk.Tiles)
                    {
                        if (tile.Resource == Item.None) writer.WriteRow(tile.Nutrients);
                        else writer.WriteRow(tile.Nutrients, tile.Resource, tile.ResourceCount);
                    }
                }
            }

            var end = DateTime.Now;
            var span = end - start;
            Console.WriteLine($"done (took {Math.Round(span.TotalMilliseconds)} ms).");
        }

        public static void Load(Map map)
        {
            Console.Write("Loading... ");
            var start = DateTime.Now;

            // Load paths
            using (var reader = new RowReader("savegame/paths.csv"))
            {
                while (reader.GetRow())
                {
                    var source = reader.ReadIntVector();
                    var dest = reader.ReadIntVector(); ;
                    var type = reader.ReadEnum<PathType>();
                    map.BuildPath(type, source, dest);
                }
            }

            // Load buildings
            using (var reader = new RowReader("savegame/buildings.csv"))
            {
                while (reader.GetRow())
                {
                    var pos = reader.ReadIntVector();
                    var type = reader.ReadEnum<BuildingType>();
                    var building = Building.Create(map, type, pos);
                    if (building is Workshop w) w.Recipe = Recipes.GetRecipe(reader.ReadString());
                    if (building.Output is DirectProducer p) p.SetBuffer(reader.ReadInt());
                    map.AddBuilding(building, pos);
                }
            }

            var end = DateTime.Now;
            var span = end - start;
            Console.WriteLine($"done (took {Math.Round(span.TotalMilliseconds)} ms).");
        }
    }
}