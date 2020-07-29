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

        private static ISaveProvider[] _providers = new ISaveProvider[] {
            new PathSaver(),
            new BuildingSaver(),
            new RouteSaver(),
            new VehicleSaver(),
            new ResourceSaver(),
        };

        private static Stream? GetReadStream(string path)
        {
            if (_zip) return _archive.GetEntry(path).Open();
            else
            {
                var p = Path.Join("savegame", path);
                if (!File.Exists(p)) return null;
                return File.OpenRead(Path.Join("savegame", path));
            }
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

            foreach (var provider in _providers) provider.Save(GetWriteStream(provider.Path), map);

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

            foreach (var provider in _providers)
            {
                var file = GetReadStream(provider.Path);
                if (file != null) provider.Load(file, map);
            }

            if (_zip) _archive.Dispose();

            var end = DateTime.Now;
            var span = end - start;
            Console.WriteLine($"done (took {Math.Round(span.TotalMilliseconds)} ms).");
        }
    }
}