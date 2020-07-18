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

        public Route(Map info, VehicleNode src, VehicleNode dest, Item item) : base(info, src, dest, item)
        {
            _actions[src] = new DestinationAction(ActionType.Pickup, item);
            _actions[dest] = new DestinationAction(ActionType.Dropoff, item);
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
        private List<T> _path = new List<T>();
        private IPathfinder<T> _pathfinder;
        private Item _item = Item.None;
        private bool _reroute = false;

        public IReadOnlyList<T> Path => _path;
        public T Source { get; set; }
        public T Dest { get; set; }
        public Map Map { get; private set; }
        public event Action<Route<T>>? Changed;

        public string Id => $"{Source.Pos}{Dest.Pos}{Item}";

        public IDirectOutput? SourceOutput { get; set; }
        public IDirectInput? DestInput { get; set; }

        public Item Item
        {
            get => _item;
            set
            {
                _item = value;
                //foreach (var h in Haulers) h.Item = value;
            }
        }

        public Route(Map info, T src, T dest, Item item)
        {
            Map = info;
            Source = src;
            Dest = dest;
            Item = item;
            _pathfinder = new AStarPathfinder<T>();
        }

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
            if (currentPos < _path.IndexOf(Dest)) return Dest;
            else return Source;
        }

        public void Pathfind()
        {
            FindPath(Source, Dest);
            var reversed = _path.GetRange(1, _path.Count - 2);
            reversed.Reverse();
            _path.AddRange(reversed);
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
