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
            using (var writer = new StackWriter(GetWriteStream("paths.csv")))
            {
                foreach (var path in map.VehiclePaths.Paths)
                {
                    if (path.PathType == PathType.Driveway || path.Fixed) continue;
                    writer.WriteStack(path.Source, path.Dest, path.PathType);
                }
            }

            // Save buildings
            using (var writer = new StackWriter(GetWriteStream("buildings.csv")))
            {
                foreach (var building in map.Buildings)
                {
                    writer.PushItems(building.Pos, building.Type);
                    if (building is Workshop w) writer.PushItem(w.Recipe.Name);
                    if (building.Output is DirectProducer p) writer.PushItem(p.Buffer);
                    if (building.Input is DirectConsumer c)
                    {
                        foreach (var buffer in c.Buffers) writer.PushItems(buffer.Item, buffer.Buffer);
                    }
                    writer.WriteStack();
                }
            }

            // Save routes
            using (var writer = new StackWriter(GetWriteStream("routes.csv")))
            {
                foreach (var route in map.Routes)
                {
                    writer.PushItems(route.Source.Pos, route.Dest.Pos, route.Item);
                    foreach (var node in route.Path) writer.PushItem(node.Pos);
                    writer.WriteStack();
                }
            }

            // Save chunks
            foreach (var chunk in map.Chunks)
            {
                using (var writer = new StackWriter(GetWriteStream($"chunks/chunk_{chunk.Pos.X}_{chunk.Pos.Y}.csv")))
                {
                    foreach (var tile in chunk.Tiles)
                    {
                        if (tile.Resource == Item.None) writer.WriteStack(tile.Nutrients);
                        else writer.WriteStack(tile.Nutrients, tile.Resource, tile.ResourceCount);
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
            using (var reader = new StackReader(GetReadStream("paths.csv")))
            {
                while (reader.TryGetStack(out var stack))
                {
                    var source = stack.PopIntVector();
                    var dest = stack.PopIntVector();
                    var type = stack.PopEnum<PathType>();
                    map.BuildPath(type, source, dest);
                }
            }

            // Load buildings
            using (var reader = new StackReader(GetReadStream("buildings.csv")))
            {
                while (reader.TryGetStack(out var stack))
                {
                    var pos = stack.PopIntVector();
                    var type = stack.PopEnum<BuildingType>();
                    var building = Building.Create(map, type, pos);
                    if (building is Workshop w) w.Recipe = Recipes.GetRecipe(stack.PopString());
                    if (building.Output is DirectProducer p) p.SetBuffer(stack.PopInt());
                    map.AddBuilding(building, pos);
                }
            }

            // Load routes
            using (var reader = new StackReader(GetReadStream("routes.csv")))
            {
                while (reader.TryGetStack(out var stack))
                {
                    var source = stack.PopIntVector();
                    var dest = stack.PopIntVector();
                    var item = stack.PopEnum<Item>();
                    var path = new List<VehicleNode>();
                    while (stack.HasItem())
                    {
                        var pos = stack.PopIntVector();
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