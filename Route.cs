using System;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public class Route : Route<VehicleNode>
    {
        public List<Hauler> Haulers { get; } = new List<Hauler>();

        public Route(Map info, VehicleNode src, VehicleNode dest) : base(info, src, dest) { }

        public void AddHauler(VehicleType type)
        {
            var hauler = Vehicle.Create(type, this);
            if (hauler is Hauler h)
            {
                Haulers.Add(h);
                Map.AddVehicle(h);
            }
            else throw new ArgumentException($"Vehicle {type} is not a hauler");
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

        public Route(Map info, T src, T dest)
        {
            Map = info;
            Source = src;
            Dest = dest;
            _pathfinder = new AStarPathfinder<T>();
        }

        public (int pos, T node) Next(int currentPos)
        {
            int index = currentPos + 1;
            if (index > _path.Count - 1) index = 0;
            return (index, _path[index]);
        }

        public void Pathfind()
        {
            FindPath(Source, Dest, ref _path);
            var reversed = _path.GetRange(1, _path.Count - 2);
            reversed.Reverse();
            _path.AddRange(reversed);
        }

        public void SetPath(IEnumerable<T> path) => _path = new List<T>(path);

        private void FindPath(T source, T dest, ref List<T> pathStorage)
        {
            var path = _pathfinder.FindPath(new PathGraph<T>(), source, dest);
            if (path != null)
            {
                pathStorage = path;
                foreach (T node in pathStorage)
                {
                    node.Changed += NodeChanged;
                    node.Removed += () => _reroute = true;
                }
            }
            else
            {
                Console.WriteLine("No path found! :(");
                pathStorage = new List<T>();
                return;
            }
        }

        public void Update()
        {
            if (_reroute)
            {
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
