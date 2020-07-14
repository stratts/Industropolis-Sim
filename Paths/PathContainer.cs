using System;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public interface IPathContainer<TNode, TPath> where TNode : class where TPath : class
    {
        TNode? GetNode(IntVector pos);
        void AddNode(TNode node);
        void RemoveNode(TNode node);
        TPath? GetPath(IntVector pos);
        void AddPath(TPath path);
        void RemovePath(TPath path);
        event Action<TNode>? NodeAdded;
        event Action<TPath>? PathAdded;
        IReadOnlyList<TPath> Paths { get; }
    }

    public class PathContainer<TNode, TPath> : IPathContainer<TNode, TPath>
        where TNode : PathNode<TNode, TPath> where TPath : Path<TNode>
    {
        public event Action<TNode>? NodeAdded;
        public event Action<TPath>? PathAdded;

        private Dictionary<IntVector, TNode> _nodes = new Dictionary<IntVector, TNode>();
        private List<TPath> _paths = new List<TPath>();
        private Dictionary<IntVector, TPath> _pathLocs = new Dictionary<IntVector, TPath>();

        public IReadOnlyList<TPath> Paths => _paths;

        public TNode? GetNode(IntVector pos)
        {
            if (_nodes.TryGetValue(pos, out var node)) return node;
            return null;
        }

        public void AddNode(TNode node)
        {
            _nodes[node.Pos] = node;
            NodeAdded?.Invoke(node);
        }

        public void RemoveNode(TNode node)
        {
            var _paths = new List<TPath>(node.Connections.Values);
            foreach (TPath path in _paths) RemovePath(path);
            _nodes.Remove(node.Pos);
            node.Remove();
        }

        public void AddPath(TPath path)
        {
            this._paths.Add(path);
            foreach (var pos in path.Source.Pos.GetPointsBetween(path.Dest.Pos)) _pathLocs[pos] = path;
            PathAdded?.Invoke(path);
        }

        public TPath? GetPath(IntVector pos)
        {
            if (_pathLocs.TryGetValue(pos, out var path) && GetNode(pos) == null) return path;
            return null;
        }

        public IEnumerable<TPath> GetPaths(IntVector pos)
        {
            foreach (TPath path in _paths)
            {
                if (path.OnPath(pos)) yield return path;
            }
        }

        public void RemovePath(TPath path)
        {
            path.Source.Disconnect(path.Dest);
            path.Dest.Disconnect(path.Source);
            foreach (var pos in path.Source.Pos.GetPointsBetween(path.Dest.Pos)) _pathLocs.Remove(pos);
            this._paths.Remove(path);
            path.Remove();
        }
    }
}
