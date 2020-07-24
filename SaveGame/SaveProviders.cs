using System;
using System.IO;
using System.Collections.Generic;
using Industropolis.Sim.Buildings;

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

        public void Error(string err) => Console.Write($"ERROR: {err}");

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
                if (building.Input is DirectConsumer c)
                {
                    while (stack.HasItem())
                    {
                        var item = stack.PopEnum<Item>();
                        int amount = stack.PopInt();
                        c.GetBuffer(item).Buffer = amount;
                    }
                }
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
                writer.PushItem(route.Id);
                writer.PushItem(route.Destinations.Count);
                foreach (var destination in route.Destinations)
                {
                    var action = route.GetAction(destination);
                    writer.PushItems(destination.Pos, action.Type, action.Item);
                }
                foreach (var node in route.Path) writer.PushItem(node.Pos);
                writer.WriteStack();
            }
        }

        protected override void Load(StackReader reader, Map map)
        {
            while (reader.TryGetStack(out var stack))
            {
                var route = new Route(map);
                var id = stack.PopId();
                var dests = stack.PopInt();

                route.SetId(id);

                for (int i = 0; i < dests; i++)
                {
                    var (pos, type, item) = (stack.PopIntVector(), stack.PopEnum<Route.ActionType>(), stack.PopEnum<Item>());
                    var node = map.GetNode(pos);
                    if (node == null) throw new NullReferenceException($"Node {pos} on map is null or does not exist");
                    route.AddDestination(node, type, item);
                }
                var path = new List<VehicleNode>();
                while (stack.HasItem())
                {
                    var pos = stack.PopIntVector();
                    var node = map.GetNode(pos);
                    if (node == null) throw new NullReferenceException($"Node {pos} on map is null or does not exist");
                    path.Add(node);
                }
                route.SetPath(path);
                map.AddRoute(route);
            }
        }
    }

    public class VehicleSaver : StackSaver
    {
        public override string Path => "vehicles.csv";

        protected override void Save(StackWriter writer, Map map)
        {
            foreach (var vehicle in map.Vehicles)
            {
                writer.PushItems(vehicle.Type, vehicle.Route.Id, vehicle.RouteIndex, vehicle.FrontPos);
                if (vehicle is Hauler h) writer.PushItems(h.Carrying);
                writer.WriteStack();
            }
        }

        protected override void Load(StackReader reader, Map map)
        {
            foreach (var stack in reader)
            {
                var type = stack.PopEnum<VehicleType>();
                var (routeId, routeIdx) = (stack.PopId(), stack.PopInt());
                var frontPos = stack.PopFloat();
                var route = map.GetRoute(routeId);
                if (route == null)
                {
                    Error($"Vehicle route {routeId} not found");
                    continue;
                }
                var vehicle = Vehicle.Create(type, route);
                vehicle.SetPosition(routeIdx, frontPos);
                if (vehicle is Hauler h)
                {
                    h.Carrying = stack.PopInt();
                    route.AddHauler(h);
                }
            }
        }
    }
}