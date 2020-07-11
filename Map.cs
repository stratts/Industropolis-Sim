using System;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public struct MapChunk
    {
        public IntVector Pos { get; }
        public IntVector MapOffset { get; }
        public IntVector Size { get; }
        public Tile[,] Tiles { get; }

        public MapChunk(IntVector pos, IntVector size, Tile[,] tiles)
        {
            Pos = pos;
            Size = size;
            Tiles = tiles;
            MapOffset = new IntVector(Pos.X * Size.X, Pos.Y * Size.Y);
        }

        public Tile GetTile(IntVector pos) => Tiles[pos.X, pos.Y];
    }

    public class Map
    {
        private List<Building> buildings;
        private List<Route> routes;
        private int _currentMoney = 0;
        private VehiclePathBuilder _pathBuilder;
        private Dictionary<IntVector, MapChunk> _chunks = new Dictionary<IntVector, MapChunk>();
        private int _chunkSize = 32;
        private Dictionary<Item, int> _resources = new Dictionary<Item, int>();

        public IPathContainer<VehicleNode, VehiclePath> VehiclePaths { get; } = new PathContainer<VehicleNode, VehiclePath>();
        public IReadOnlyList<Building> Buildings => buildings;
        public List<Vehicle> Vehicles = new List<Vehicle>();
        public IReadOnlyList<Route> Routes => routes;
        public IReadOnlyDictionary<Item, int> Resources => _resources;

        //public PopulationInfo Population { get; } = new PopulationInfo();
        public int CurrentMoney
        {
            get
            {
                return _currentMoney;
            }
            set
            {
                _currentMoney = value;
                if (_currentMoney < 0) _currentMoney = 0;
            }
        }

        public event Action<VehiclePath>? PathAdded;
        public event Action<VehicleNode>? PathNodeAdded;
        public event Action<Building>? BuildingAdded;
        public event Action<Route>? RouteAdded;
        public event Action<Vehicle>? VehicleAdded;
        public event Action<MapChunk>? ChunkLoaded;

        public Map()
        {
            routes = new List<Route>();
            buildings = new List<Building>();

            VehiclePaths.PathAdded += (VehiclePath road) => PathAdded?.Invoke(road);
            VehiclePaths.NodeAdded += (VehicleNode node) => PathNodeAdded?.Invoke(node);

            _pathBuilder = new VehiclePathBuilder(this);
        }

        public void Generate()
        {
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    var tiles = MapGenerator.GenerateChunk(x, y, _chunkSize, _chunkSize, 1);
                    var pos = new IntVector(x, y);
                    var chunk = new MapChunk(pos, new IntVector(_chunkSize, _chunkSize), tiles);
                    _chunks[pos] = chunk;
                    ChunkLoaded?.Invoke(chunk);
                    Console.WriteLine($"Generated chunk {pos} at {chunk.MapOffset}");
                }
            }
        }

        public bool CanBuild(Building building, IntVector pos)
        {
            for (int x = 0; x < building.Width; x++)
            {
                for (int y = 0; y < building.Height; y++)
                {
                    var p = new IntVector(pos.X + x, pos.Y + y);
                    if (GetPath(p) != null) return false;
                    if (GetNode(p) != null) return false;
                    if (GetBuilding(p) != null) return false;
                }
            }

            return true;
        }

        public void AddBuilding(Building building, IntVector pos)
        {
            /*if (building.RequiredResources != null) {
                foreach (var resource in building.RequiredResources) {
                    if (!HasResource(resource.Key, resource.Value)) {
                        Godot.GD.Print($"Not enough resources to build {building.GetType().Name}");
                        return;
                    }
                }
                foreach (var resource in building.RequiredResources) {
                    GetResource(resource.Key, resource.Value);
                }
            }*/
            if (CurrentMoney < building.Cost)
            {
                Console.WriteLine($"Not enough money to build {building.GetType().Name}");
                return;
            }
            else
            {
                CurrentMoney -= building.Cost;
            }
            building.Pos = pos;
            if (!CanBuild(building, pos))
            {
                Console.WriteLine("Cannot build here");
                return;
            }
            building.Pos = pos;
            buildings.Add(building);
            if (building.HasEntrance) _pathBuilder.ConnectBuilding(building);
            if (building is TrainStation t) t.Setup();
            BuildingAdded?.Invoke(building);
        }

        public void AddVehicle(Vehicle vehicle)
        {
            Vehicles.Add(vehicle);
            vehicle.Removed += () => Vehicles.Remove(vehicle);
            VehicleAdded?.Invoke(vehicle);
        }

        public Building? GetBuilding(IntVector pos)
        {
            foreach (var b in buildings)
            {
                foreach (var t in GetBuildingTiles(b))
                {
                    if (t == pos) return b;
                }
            }
            return null;
        }

        public IEnumerable<IntVector> GetBuildingTiles(Building building)
        {
            for (int x = 0; x < building.Width; x++)
            {
                for (int y = 0; y < building.Height; y++)
                {
                    yield return building.Pos + new IntVector(x, y);
                }
            }
        }

        public void RemoveBuilding(Building building)
        {
            building.Remove();
            buildings.Remove(building);
            if (building.HasEntrance && building.Entrance!.Node != null) _pathBuilder.DisconnectBuilding(building);
        }

        public void Update(float delta)
        {
            foreach (Route r in routes) r.Update();
            foreach (Building b in buildings) b.Update(delta);
            foreach (Vehicle v in Vehicles) v.Update(delta);
        }

        public Route AddRoute(VehicleNode start, VehicleNode dest, Item item)
        {
            var route = new Route(this, start, dest);
            route.Item = item;
            route.SourceOutput = start.GetLink<Building>().Output;
            route.DestInput = dest.GetLink<Building>().Input;
            route.Pathfind();
            routes.Add(route);
            RouteAdded?.Invoke(route);

            return route;
        }

        public Route? GetRoute(IntVector pos)
        {
            /*foreach (Route r in routes) {
                foreach (PathNode t in r.Path) {
                    if (t == pos) return r;
                }
            }*/

            return null;
        }

        public void RemoveRoute(Route route)
        {
            routes.Remove(route);
            route.Remove();
        }

        public void BuildPath(PathType type, IntVector source, IntVector dest, bool buildFixed = false)
        {
            _pathBuilder.BuildPath(type, source, dest, buildFixed);
        }

        public VehicleNode? GetNode(IntVector pos) => VehiclePaths.GetNode(pos);
        public void RemoveNode(VehicleNode node) => VehiclePaths.RemoveNode(node);

        public VehiclePath? GetPath(IntVector pos) => VehiclePaths.GetPath(pos);
        public void RemovePath(VehiclePath path) => VehiclePaths.RemovePath(path);
        public void DeletePathSegment(IntVector pos) => _pathBuilder.DeletePathSegment(pos);

        public void AddResource(Item item, int amount)
        {
            if (!_resources.ContainsKey(item)) _resources[item] = 0;
            _resources[item] += amount;
        }

        public int GetResourceAmount(Item item)
        {
            bool has = _resources.TryGetValue(item, out int count);
            if (!has) return 0;
            return count;
        }

        public bool HasResource(Item item, int amount) => GetResourceAmount(item) >= amount;

        public void UseResource(Item item, int amount)
        {
            if (!HasResource(item, amount)) throw new ArgumentException($"Not enough resources to use {amount} of {item}");
            _resources[item] -= amount;
        }

        public Tile GetTile(IntVector pos)
        {
            var chunkData = GetChunkAt(pos);
            if (!ValidPos(pos) || !chunkData.HasValue) throw new ArgumentException("Invalid tile");
            var chunk = chunkData.Value;
            var chunkIndex = pos - chunk.MapOffset;
            return chunk.Tiles[chunkIndex.X, chunkIndex.Y];
        }

        public bool ValidPos(IntVector pos) => GetChunkAt(pos).HasValue;

        private MapChunk? GetChunkAt(IntVector pos)
        {
            var chunkX = (int)Math.Floor((float)pos.X / _chunkSize);
            var chunkY = (int)Math.Floor((float)pos.Y / _chunkSize);

            var chunkPos = new IntVector((int)chunkX, (int)chunkY);
            if (!_chunks.ContainsKey(chunkPos)) return null;
            return _chunks[chunkPos];
        }

        public void CreateResourcePatch(int x, int y, int size, Item resource, int amount)
        {
            for (int i = x; i < x + size; i++)
            {
                for (int j = y; j < y + size; j++)
                {
                    Tile? t = GetTile(new IntVector(i, j));
                    if (t != null)
                    {
                        t.Resource = resource;
                        t.ResourceCount = amount;
                    }
                }
            }
        }
    }
}
