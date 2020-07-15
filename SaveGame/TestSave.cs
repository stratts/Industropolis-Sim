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

            var paths = map.VehiclePaths.Paths;
            var writer = new RowWriter("savegame/paths.csv", paths.Count);

            foreach (var path in paths)
            {
                if (path.PathType == PathType.Driveway) continue;
                writer.WriteRow(
                    path.Source,
                    path.Dest,
                    path.PathType,
                    path.Fixed
                );
            }
            writer.SaveFile();

            writer = new RowWriter("savegame/buildings.csv", map.Buildings.Count);

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

            writer.SaveFile();

            foreach (var chunk in map.Chunks)
            {
                writer = new RowWriter($"savegame/chunks/chunk_{chunk.Pos.X}_{chunk.Pos.Y}.csv", ((32 * 32) + 1) * map.Chunks.Count);
                for (int x = 0; x < chunk.Size.X; x++)
                {
                    for (int y = 0; y < chunk.Size.Y; y++)
                    {
                        var tile = chunk.Tiles[x, y];
                        if (tile.Resource == Item.None) writer.WriteRow(tile.Nutrients);
                        else writer.WriteRow(tile.Nutrients, tile.Resource, tile.ResourceCount);
                    }
                }
                writer.SaveFile();
            }

            var end = DateTime.Now;
            var span = end - start;
            Console.WriteLine($"done (took {Math.Round(span.TotalMilliseconds)} ms).");
        }

        public static void Load(Map map)
        {
            Console.Write("Loading... ");
            var start = DateTime.Now;

            var reader = new RowReader("savegame/paths.csv");
            reader.LoadFile();
            while (!reader.AtEnd)
            {
                var source = reader.ReadIntVector();
                var dest = reader.ReadIntVector(); ;
                var type = reader.ReadEnum<PathType>();
                var isFixed = reader.ReadBool();
                map.BuildPath(type, source, dest, isFixed);
                reader.NextRow();
            }

            reader = new RowReader("savegame/buildings.csv");
            reader.LoadFile();
            while (!reader.AtEnd)
            {
                var pos = reader.ReadIntVector();
                var type = reader.ReadEnum<BuildingType>();
                var building = Building.Create(map, type, pos);
                if (building is Workshop w) w.Recipe = Recipes.GetRecipe(reader.ReadString());
                if (building.Output is DirectProducer p) p.SetBuffer(reader.ReadInt());
                map.AddBuilding(building, pos);
                reader.NextRow();
            }

            var end = DateTime.Now;
            var span = end - start;
            Console.WriteLine($"done (took {Math.Round(span.TotalMilliseconds)} ms).");
        }
    }
}