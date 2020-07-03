using System;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public enum RouteDirection { Forwards, Backwards };

    // Temporary!
    public class RoadRoute : Route<RoadNode>
    {
        public RoadRoute(MapInfo info, RoadNode src, RoadNode dest) : base(info, src, dest) { }
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
            int shift = 0;
            var curr = _path.IndexOf(current);

            if (direction == RouteDirection.Forwards) shift = 1;
            else if (direction == RouteDirection.Backwards) shift = -1;

            var newPos = curr + shift;
            if (newPos >= 0 && newPos < _path.Count) return _path[newPos];
            else return current;
        }

        public void Pathfind()
        {
            var path = _pathfinder.FindPath(new PathGraph<T>(), Source, Dest);
            if (path != null)
            {
                _path = path;
                foreach (T node in _path)
                {
                    node.Changed += NodeChanged;
                    node.Removed += () => _reroute = true;
                }
            }
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
