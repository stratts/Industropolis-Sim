using System;
using System.IO;
using System.Collections.Generic;

namespace Industropolis.Sim.SaveGame
{
    public interface ISaveProvider
    {
        string Path { get; }
        void Load(Stream stream, Map map);
        void Save(Stream stream, Map map);
    }

    public abstract class StackSaver : ISaveProvider
    {
        public abstract string Path { get; }

        public void Save(Stream stream, Map map)
        {
            using (var writer = new StackWriter(stream)) { Save(writer, map); }
        }

        public void Load(Stream stream, Map map)
        {
            using (var reader = new StackReader(stream)) { Load(reader, map); }
        }

        protected abstract void Save(StackWriter writer, Map map);
        protected abstract void Load(StackReader reader, Map map);
    }

    public class BuildingSaver : StackSaver
    {
        public override string Path => "buildings.csv";

        protected override void Save(StackWriter writer, Map map)
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

        protected override void Load(StackReader reader, Map map)
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
    }

    public class PathSaver : StackSaver
    {
        public override string Path => "paths.csv";

        protected override void Save(StackWriter writer, Map map)
        {
            foreach (var path in map.VehiclePaths.Paths)
            {
                if (path.PathType == PathType.Driveway || path.Fixed) continue;
                writer.WriteStack(path.Source, path.Dest, path.PathType);
            }
        }

        protected override void Load(StackReader reader, Map map)
        {
            while (reader.TryGetStack(out var stack))
            {
                var source = stack.PopIntVector();
                var dest = stack.PopIntVector();
                var type = stack.PopEnum<PathType>();
                map.BuildPath(type, source, dest);
            }
        }
    }

    public class RouteSaver : StackSaver
    {
        public override string Path => "routes.csv";

        protected override void Save(StackWriter writer, Map map)
        {
            foreach (var route in map.Routes)
            {
                writer.PushItems(route.Source.Pos, route.Dest.Pos, route.Item);
                foreach (var node in route.Path) writer.PushItem(node.Pos);
                writer.WriteStack();
            }
        }

        protected override void Load(StackReader reader, Map map)
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
    }
}