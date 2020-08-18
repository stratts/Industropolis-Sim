using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Buffers;
using System.Text.Json;

namespace Industropolis.Sim.SaveGame
{
    public static class GameSaver
    {
        private static bool _zip = false;
        private static ZipArchive _archive = null!;
        private const string _saveDirectory = "savegame";

        private static ISaveProvider[] _providers = new ISaveProvider[] {
            new PathSaver(),
            new BuildingSaver(),
            new RouteSaver(),
            new VehicleSaver(),
            new ResourceSaver(),
        };

        private static Stream? GetReadStream(string name, string path)
        {
            if (_zip) return _archive.GetEntry(path).Open();
            else
            {
                var p = SavePath(name, path);
                if (!File.Exists(p)) return null;
                return File.OpenRead(p);
            }
        }

        private static Stream GetWriteStream(string name, string path)
        {
            if (_zip) return _archive.CreateEntry(path, CompressionLevel.NoCompression).Open();
            else return File.Open(SavePath(name, path), FileMode.Create);
        }

        private static string SavePath(string name, string? path = null)
        {
            if (path == null) return Path.Join(_saveDirectory, name);
            else return Path.Join(_saveDirectory, name, path);
        }

        public static IEnumerable<string> GetSaves()
        {
            if (!Directory.Exists(_saveDirectory)) Directory.CreateDirectory(_saveDirectory);
            var info = new DirectoryInfo(_saveDirectory);
            if (_zip) return info.EnumerateFiles().Select(finfo => finfo.Name).OrderBy(n => n);
            else return info.EnumerateDirectories().Select(dinfo => dinfo.Name).OrderBy(n => n);
        }

        public static void DeleteSave(string name)
        {
            if (_zip) File.Delete(SavePath(name) + ".zip");
            else Directory.Delete(SavePath(name), true);
        }

        public static void Save(Map map, string name)
        {
            Console.Write("Saving... ");
            var start = DateTime.Now;

            if (!Directory.Exists(_saveDirectory)) Directory.CreateDirectory(_saveDirectory);

            // Create ZIP file if using
            var stream = new MemoryStream();
            if (_zip) _archive = new ZipArchive(stream, ZipArchiveMode.Create, true);
            else
            {
                if (Directory.Exists(SavePath(name))) Directory.Delete(SavePath(name), true);
                Directory.CreateDirectory(SavePath(name));
                Directory.CreateDirectory(SavePath(name, "chunks"));
            }

            foreach (var provider in _providers) provider.Save(GetWriteStream(name, provider.Path), map);

            // Save chunks
            foreach (var chunk in map.Chunks)
            {
                using (var writer = new StackWriter(GetWriteStream(name, $"chunks/chunk_{chunk.Pos.X}_{chunk.Pos.Y}.csv")))
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
                using (var f = File.Open(SavePath(name) + ".zip", FileMode.Create))
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

        public static void Load(Map map, string name)
        {
            Console.Write($"Loading {name}... ");
            var start = DateTime.Now;

            // Load ZIP file if using
            var stream = new MemoryStream();
            if (_zip)
            {
                using (var f = File.OpenRead(SavePath(name) + ".zip"))
                {
                    f.CopyTo(stream);
                }
                _archive = new ZipArchive(stream, ZipArchiveMode.Read);
            }

            foreach (var provider in _providers)
            {
                var file = GetReadStream(name, provider.Path);
                if (file != null) provider.Load(file, map);
            }

            if (_zip) _archive.Dispose();

            var end = DateTime.Now;
            var span = end - start;
            Console.WriteLine($"done (took {Math.Round(span.TotalMilliseconds)} ms).");
        }
    }
}