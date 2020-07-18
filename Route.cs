using System;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public class Route : Route<VehicleNode>
    {
        private Dictionary<VehicleNode, DestinationAction> _actions = new Dictionary<VehicleNode, DestinationAction>();

        public enum ActionType { Pickup, Dropoff }

        public struct DestinationAction
        {
            public ActionType Type { get; }
            public Item Item { get; }

            public DestinationAction(ActionType type, Item item)
            {
                Type = type;
                Item = item;
            }
        }

        public List<Hauler> Haulers { get; } = new List<Hauler>();

        public VehicleNode Source => Destinations[0];
        public VehicleNode Dest => Destinations[1];

        public Route(Map info, VehicleNode src, VehicleNode dest, Item item) : base(info, item)
        {
            AddDestination(src, ActionType.Pickup, item);
            AddDestination(dest, ActionType.Dropoff, item);
        }

        public void AddDestination(VehicleNode node, ActionType type, Item item)
        {
            AddDestination(node);
            _actions[node] = new DestinationAction(type, item);
        }

        public DestinationAction GetAction(VehicleNode destination) => _actions[destination];

        public void AddHauler(VehicleType type)
        {
            var hauler = Vehicle.Create(type, this);
            if (hauler is Hauler h) AddHauler(h);
            else throw new ArgumentException($"Vehicle {type} is not a hauler");
        }

        public void AddHauler(Hauler hauler)
        {
            Haulers.Add(hauler);
            Map.AddVehicle(hauler);
        }

        public void RemoveHauler()
        {
            if (Haulers.Count == 0) return;
            var hauler = Haulers[Haulers.Count - 1];
            Haulers.Remove(hauler);
            hauler.Remove();
        }
    }

    public class Route<T> : MapObject where T : IPathNode<T>
    {
        private List<T> _destinations = new List<T>();
        private List<T> _path = new List<T>();
        private IPathfinder<T> _pathfinder;
        private Item _item = Item.None;
        private bool _reroute = false;

        public IReadOnlyList<T> Destinations => _destinations;
        public IReadOnlyList<T> Path => _path;
        public Map Map { get; private set; }
        public event Action<Route<T>>? Changed;

        public Guid Id { get; private set; } = Guid.NewGuid();

        public Item Item
        {
            get => _item;
            set
            {
                _item = value;
                //foreach (var h in Haulers) h.Item = value;
            }
        }

        public Route(Map info, Item item)
        {
            Map = info;
            Item = item;
            _pathfinder = new AStarPathfinder<T>();
        }

        protected void AddDestination(T node) => _destinations.Add(node);

        public void SetId(Guid g) => Id = g;

        public int WrapIndex(int index)
        {
            if (index > _path.Count - 1) return index % _path.Count;
            else if (index < 0) return WrapIndex(_path.Count + index);
            else return index;
        }

        public T GetNode(int index)
        {
            return _path[WrapIndex(index)];
        }

        public (int pos, T node) Next(int currentPos)
        {
            int index = WrapIndex(currentPos + 1);
            return (index, _path[index]);
        }

        public T GetNextDestination(int currentPos)
        {
            for (int i = 0; i < Path.Count; i++)
            {
                int idx = WrapIndex(currentPos + i);
                var node = _path[idx];
                if (_destinations.Contains(node)) return node;
            }
            throw new ArgumentException("Could not find next destination");
        }

        public void Pathfind()
        {
            var path = new List<T>();
            var graph = new PathGraph<T>();
            for (int i = 0; i < _destinations.Count; i++)
            {
                T source = _destinations[i];
                T dest = i == _destinations.Count - 1 ? _destinations[0] : _destinations[i + 1];

                var segment = _pathfinder.FindPath(graph, source, dest);
                if (segment == null)
                {
                    Console.WriteLine("No path found! :(");
                    continue;
                }

                path.AddRange(segment.GetRange(0, segment.Count - 1));
            }
            SetPath(path);
        }

        public void SetPath(IEnumerable<T> path)
        {
            _path = new List<T>(path);
            foreach (T node in _path)
            {
                node.Changed += NodeChanged;
                node.Removed += () => _reroute = true;
            }
        }

        private void FindPath(T source, T dest)
        {
            var path = _pathfinder.FindPath(new PathGraph<T>(), source, dest);
            if (path != null) SetPath(path);
            else
            {
                Console.WriteLine("No path found! :(");
                _path = new List<T>();
                return;
            }
        }

        public void Update()
        {
            if (_reroute)
            {
                Console.WriteLine("Reroute!");
                Pathfind();
                Changed?.Invoke(this);
                _reroute = false;
            }
        }

        private void NodeChanged(T node)
        {
            // Unsubscribe because the node may not be in the rerouted path
            node.Changed -= NodeChanged;
            _reroute = true;
        }
    }
}
