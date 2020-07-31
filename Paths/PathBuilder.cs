using System;
using System.Numerics;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public enum PathCategory
    {
        Road,
        Rail
    }

    public enum PathType
    {
        Driveway,
        SimpleRoad,
        OneWayRoad,
        Rail
    }

    public interface IPathBuilder
    {
        void BuildPath(PathType type, IntVector source, IntVector dest, bool buildFixed = false);
        bool CanBuildPath(PathType type, IntVector source, IntVector dest);
        bool CanBuildAt(PathType type, IntVector pos);
    }

    public abstract class PathBuilder
    {
        public static PathCategory GetCategory(PathType type)
        {
            switch (type)
            {
                case PathType.SimpleRoad: return PathCategory.Road;
                case PathType.OneWayRoad: return PathCategory.Road;
                case PathType.Driveway: return PathCategory.Road;
                case PathType.Rail: return PathCategory.Rail;
                default: return default(PathCategory);
            }
        }

        public static (Item item, int amount) GetResourceRequirement(PathType type)
        {
            switch (type)
            {
                case PathType.SimpleRoad: return (Item.Stone, 10);
                case PathType.OneWayRoad: return (Item.Stone, 10);
                case PathType.Rail: return (Item.Wood, 10);
                default: return (Item.None, 0);
            }
        }
    }

    public abstract class PathBuilder<TNode, TPath> : PathBuilder, IPathBuilder
        where TNode : PathNode<TNode, TPath> where TPath : Path<TNode>
    {
        protected IPathContainer<TNode, TPath> _manager;

        public PathBuilder(IPathContainer<TNode, TPath> manager)
        {
            _manager = manager;
        }

        public abstract TPath MakePath(PathType type, TNode source, TNode dest);
        public abstract TNode MakeNode(IntVector pos, PathCategory category);

        public virtual bool CanBuildPath(PathType type, IntVector source, IntVector dest)
        {
            if (source == dest) return false;
            var dir = source.Direction(dest);
            foreach (var pos in source.GetPointsBetween(dest))
            {
                if (!CanBuildAt(type, pos, dir)) return false;
            }
            return true;
        }

        public virtual bool CanBuildAt(PathType type, IntVector pos, Vector2 direction)
        {
            if (_manager.GetPath(pos) is VehiclePath p)
            {
                if (p.Direction.IsParallelTo(direction))
                {
                    if (p.Category != GetCategory(type)) return false;
                }
                else if (p.Fixed) return false;
            }
            return true;
        }

        public bool CanBuildAt(PathType type, IntVector pos) => CanBuildAt(type, pos, Vector2.Zero);

        public virtual void BuildPath(PathType type, IntVector source, IntVector dest, bool buildFixed)
        {
            var category = GetCategory(type);

            var sourceNode = AddNode(source, category, buildFixed);
            var destNode = AddNode(dest, category, buildFixed);

            TNode prev = sourceNode;
            var dir = source.Direction(dest);

            // Build path between every node encountered
            foreach (var pos in source.GetPointsBetween(dest))
            {
                if (_manager.GetPath(pos) is TPath p && !p.Fixed)
                {
                    if (p.Direction.IsParallelTo(dir)) DeletePath(p);
                    else AddNode(pos, category, buildFixed);
                }
                if (_manager.GetNode(pos) is TNode n && n != sourceNode)
                {
                    if (!prev.Connections.ContainsKey(n)) AddPath(type, prev, n, buildFixed);
                    prev = n;
                }
            }

            // Merge any nodes that we can
            foreach (var pos in source.GetPointsBetween(dest))
            {
                if (_manager.GetNode(pos) is TNode n && CanMergeNode(n)) MergeNode(n);
            }
        }

        private void AddPath(PathType type, TNode source, TNode dest, bool fixedPath)
        {
            var path = MakePath(type, source, dest);
            source.Connect(dest, path);
            dest.Connect(source, path);
            path.Fixed = fixedPath;
            _manager.AddPath(path);
        }

        private TNode AddNode(IntVector pos, PathCategory category, bool fixedNode)
        {
            // If node already exists, return that
            if (_manager.GetNode(pos) is TNode n)
            {
                if (fixedNode) n.Fixed = true;
                return n;
            }

            var node = MakeNode(pos, category);
            node.Fixed = fixedNode;

            // If path exists at pos, delete and build paths to source and dest 
            if (_manager.GetPath(pos) is TPath p)
            {
                DeletePath(p);
                AddPath(p.PathType, p.Source, node, p.Fixed);
                AddPath(p.PathType, node, p.Dest, p.Fixed);
            }

            _manager.AddNode(node);
            return node;
        }

        protected bool CanMergeNode(TNode node)
        {
            if (node.Fixed) return false;
            if (node.Connections.Count != 2) return false;
            var paths = new List<TPath>(node.Connections.Values);
            if (paths[0].PathType != paths[1].PathType) return false;
            if (!paths[0].Direction.IsParallelTo(paths[1].Direction)) return false;
            return true;
        }

        protected void MergeNode(TNode node)
        {
            var nodes = new List<TNode>(node.Connections.Keys);
            var type = node.Connections[nodes[0]].PathType;
            _manager.RemoveNode(node);
            AddPath(type, nodes[0], nodes[1], node.Fixed);
        }

        private void DeleteNode(TNode node)
        {
            var nodes = new List<TNode>(node.Connections.Keys);
            _manager.RemoveNode(node);
            foreach (var n in nodes)
            {
                if (CanMergeNode(n)) MergeNode(n);
            }
        }

        private void DeletePath(TPath path) => _manager.RemovePath(path);

        public virtual void DeletePathSegment(IntVector pos)
        {
            if (_manager.GetPath(pos) is TPath p && !p.Fixed)
            {
                DeletePath(p);
                BuildPath(p.PathType, p.Source.Pos, pos + pos.Direction8(p.Source.Pos), p.Fixed);
                BuildPath(p.PathType, p.Dest.Pos, pos + pos.Direction8(p.Dest.Pos), p.Fixed);
            }
            if (_manager.GetNode(pos) is TNode n && !n.Fixed)
            {
                var nodes = new List<TNode>(n.Connections.Keys);
                foreach (var connection in nodes) AddNode(pos + pos.Direction8(connection.Pos), n.Category, n.Fixed);
                DeleteNode(n);
            }
        }
    }
}
