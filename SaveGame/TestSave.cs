using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Buffers;
using System.Text.Json;

namespace Industropolis.Sim.SaveGame
{
    public static class TestSave
    {
        private static bool _zip = false;
        private static ZipArchive _archive = null!;

        private static Stream GetReadStream(string path)
        {
            if (_zip) return _archive.GetEntry(path).Open();
            else return File.OpenRead(Path.Join("savegame", path));
        }

        private static Stream GetWriteStream(string path)
        {
            if (_zip) return _archive.CreateEntry(path, CompressionLevel.NoCompression).Open();
            else return File.Open(Path.Join("savegame", path), FileMode.Create);
        }

        public static void Save(Map map)
        {
            Console.Write("Saving... ");
            var start = DateTime.Now;

            // Create ZIP file if using
            var stream = new MemoryStream();
            if (_zip) _archive = new ZipArchive(stream, ZipArchiveMode.Create, true);
            else
            {
                if (Directory.Exists("savegame")) Directory.Delete("savegame", true);
                Directory.CreateDirectory("savegame");
                Directory.CreateDirectory("savegame/chunks");
            }

            // Save paths
            using (var writer = new RowWriter(GetWriteStream("paths.csv"), map.VehiclePaths.Paths.Count))
            {
                foreach (var path in map.VehiclePaths.Paths)
                {
                    if (path.PathType == PathType.Driveway || path.Fixed) continue;
                    writer.WriteRow(path.Source, path.Dest, path.PathType);
                }
            }

            // Save buildings
            using (var writer = new RowWriter(GetWriteStream("buildings.csv"), map.Buildings.Count))
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

            // Save routes
            using (var writer = new RowWriter(GetWriteStream("routes.csv"), map.Routes.Count))
            {
                foreach (var route in map.Routes)
                {
                    writer.StartRow();
                    writer.WriteFields(route.Source.Pos, route.Dest.Pos, route.Item);
                    foreach (var node in route.Path) writer.WriteField(node.Pos);
                }
            }

            // Save chunks
            foreach (var chunk in map.Chunks)
            {
                using (var writer = new RowWriter(GetWriteStream($"chunks/chunk_{chunk.Pos.X}_{chunk.Pos.Y}.csv"), chunk.Tiles.Length))
                {
                    foreach (var tile in chunk.Tiles)
                    {
                        if (tile.Resource == Item.None) writer.WriteRow(tile.Nutrients);
                        else writer.WriteRow(tile.Nutrients, tile.Resource, tile.ResourceCount);
                    }
                }
            }

            if (_zip)
            {
                _archive.Dispose();
                using (var f = File.Open("savegame.zip", FileMode.Create))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(f);
                }
            }
            stream.Dispose();

            var end = DateTime.Now;
            var span = end - start;
            Console.WriteLine($"done (took {Math.Round(span.TotalMilliseconds)} ms).");
        }

        public static void Load(Map map)
        {
            Console.Write("Loading... ");
            var start = DateTime.Now;

            // Load ZIP file if using
            var stream = new MemoryStream();
            if (_zip)
            {
                using (var f = File.OpenRead("savegame.zip"))
                {
                    f.CopyTo(stream);
                }
                _archive = new ZipArchive(stream, ZipArchiveMode.Read);
            }

            // Load paths
            using (var reader = new RowReader(GetReadStream("paths.csv")))
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
            using (var reader = new RowReader(GetReadStream("buildings.csv")))
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

            // Load routes
            using (var reader = new RowReader(GetReadStream("routes.csv")))
            {
                while (reader.GetRow())
                {
                    var source = reader.ReadIntVector();
                    var dest = reader.ReadIntVector();
                    var item = reader.ReadEnum<Item>();
                    var path = new List<VehicleNode>();
                    while (reader.HasField())
                    {
                        var pos = reader.ReadIntVector();
                        var node = map.GetNode(pos);
                        if (node == null) throw new NullReferenceException($"Node {pos} on map is null or does not exist");
                        path.Add(node);
                    }
                    map.AddRoute(map.GetNode(source)!, map.GetNode(dest)!, item, path);
                }
            }

            if (_zip) _archive.Dispose();

            var end = DateTime.Now;
            var span = end - start;
            Console.WriteLine($"done (took {Math.Round(span.TotalMilliseconds)} ms).");
        }
    }
}