using System;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public enum RouteDirection { Forwards, Backwards };

    public class Route : Route<VehicleNode>
    {
        public Route(MapInfo info, VehicleNode src, VehicleNode dest) : base(info, src, dest) { }
    }

    public class Route<T> : MapObject where T : IPathNode<T>
    {
        private List<T> _forwardsPath = new List<T>();
        private List<T> _backwardsPath = new List<T>();
        private IPathfinder<T> _pathfinder;
        private Item _item = Item.None;
        private bool _reroute = false;

        public IReadOnlyList<T> Path => _forwardsPath;
        public T Source { get; set; }
        public T Dest { get; set; }
        public MapInfo MapInfo { get; private set; }
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
        //public List<Hauler> Haulers { get; } = new List<Hauler>();

        //public int NumHaulers => Haulers.Count;

        public Route(MapInfo info, T src, T dest)
        {
            MapInfo = info;
            Source = src;
            Dest = dest;
            _pathfinder = new AStarPathfinder<T>();
        }

        public T Next(T current, RouteDirection direction)
        {
            switch (direction)
            {
                case RouteDirection.Forwards: return GetNext(current, _forwardsPath);
                case RouteDirection.Backwards: return GetNext(current, _backwardsPath);
                default: throw new NotSupportedException();
            }
        }

        private T GetNext(T current, List<T> path)
        {
            int index = path.IndexOf(current) + 1;
            if (index < path.Count) return path[index];
            return current;
        }

        public void Pathfind()
        {
            FindPath(Source, Dest, ref _forwardsPath);
            FindPath(Dest, Source, ref _backwardsPath);
        }

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

        public void AddHauler()
        {
            //if (!MapInfo.Population.Use()) return;
            /*var hauler = new Hauler(MapInfo, Source.X, Source.Y);
            hauler.Route = this;
            hauler.Item = Item;
            Haulers.Add(hauler);
            hauler.Haul();
            MapInfo.AddEntity(hauler);*/
        }

        public void RemoveHauler()
        {
            /*if (Haulers.Count == 0) return;
             Hauler h = Haulers[0];
             Haulers.RemoveAt(0);
             h.Route = null;
             MapInfo.RemoveEntity(h);*/
            //MapInfo.Population.Free();
        }
    }
}
