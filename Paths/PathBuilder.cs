using System;
using System.Collections.Generic;

public enum PathCategory
{
    Road,
    Rail
}

public enum PathType
{
    SimpleRoad,
    OneWayRoad,
    Rail
}

public interface IPathManager<TNode, TPath> where TNode : class where TPath : class
{
    TNode? GetNode(IntVector pos);
    void AddNode(TNode node);
    void RemoveNode(TNode node);
    TPath? GetPath(IntVector pos);
    void AddPath(TPath path);
    void RemovePath(TPath path);
    IReadOnlyList<Building> Buildings { get; }
}

public abstract class PathBuilder<TNode, TPath>
    where TNode : PathNode<TNode, TPath> where TPath : Path<TNode>
{
    protected IPathManager<TNode, TPath> _map;

    public PathBuilder(IPathManager<TNode, TPath> map)
    {
        _map = map;
    }

    public abstract TPath MakePath(PathType type, TNode source, TNode dest);
    public abstract TNode MakeNode(IntVector pos, PathCategory category);

    public static PathCategory GetCategory(PathType type)
    {
        switch (type)
        {
            case PathType.SimpleRoad: return PathCategory.Road;
            case PathType.Rail: return PathCategory.Rail;
            default: return default(PathCategory);
        }
    }

    private TNode AddPathNode(PathCategory category, IntVector pos)
    {
        TNode? n = _map.GetNode(pos);
        if (n == null || n.Category != category)
        {
            n = MakeNode(pos, category);
            TPath? p = _map.GetPath(pos);
            if (p != null && p.Category == category)
            {
                var (path1, path2) = PathUtils.Split(p, n);
                NodeUtils.Connect(path1.Source, path1.Dest, path1);
                NodeUtils.Connect(path2.Source, path2.Dest, path2);
                _map.AddPath(path1);
                _map.AddPath(path2);
                _map.RemovePath(p);
            }
            _map.AddNode(n);
        }
        return n;
    }

    public virtual void BuildPath(PathType type, IntVector source, IntVector dest)
    {
        if (source == dest) return;

        IntVector inc = source.Direction(dest);
        IntVector segmentStart = source;
        IntVector current = source;
        var toConnect = new List<Building>();

        while (current != dest)
        {
            current += inc;
            TPath? path = _map.GetPath(current);
            TNode? node = _map.GetNode(current);

            bool build = false;
            bool setStart = false;

            if (node != null)
            {
                TPath? startPath = _map.GetPath(segmentStart);
                if (startPath != null && startPath.Direction.IsParallelTo(inc)) setStart = true;
                else
                {
                    TNode? startNode = _map.GetNode(segmentStart);
                    if (startNode == null || !startNode.IsConnected(node)) build = true;
                }
            }
            else if (path != null)
            {
                if (path.Direction.IsParallelTo(inc)) setStart = true;
                else build = true;
            }
            else if (current == dest) build = true;

            if (build) BuildPathSegment(type, segmentStart, current);
            if (build || setStart) segmentStart = current;
        }
    }

    public void BuildPathSegment(PathType type, IntVector source, IntVector dest)
    {
        var category = GetCategory(type);
        TNode s = AddPathNode(category, source);
        TNode d = AddPathNode(category, dest);
        TPath path = MakePath(type, s, d);
        NodeUtils.Connect(path.Source, path.Dest, path);
        _map.AddPath(path);
        if (s.Connections.Count == 2) TryMergeNode(s);
        if (d.Connections.Count == 2) TryMergeNode(d);
    }

    public void DeletePathSegment(IntVector pos)
    {
        TPath? p = _map.GetPath(pos);
        TNode? n = _map.GetNode(pos);
        if (p == null && n == null)
        {
            throw new ArgumentException($"Position {pos} does not contain a path to delete");
        }
        if (p != null)
        {
            AddPathNode(p.Category, pos + p.Direction);
            AddPathNode(p.Category, pos - p.Direction);
            var newPath = _map.GetPath(pos);
            if (newPath == null) throw new Exception("No path found when deleting path segment");
            NodeUtils.Disconnect(newPath.Source, newPath.Dest);
            _map.RemovePath(newPath);
        }
        else if (n != null)
        {
            var connections = new List<TNode>(n.Connections.Keys);
            foreach (TNode connection in connections)
            {
                AddPathNode(n.Category, n.Pos + n.Pos.Direction(connection.Pos));
            }
            _map.RemoveNode(n);
        }
    }

    protected void TryMergeNode(TNode node)
    {
        TPath? path1 = null;
        TPath? path2 = null;
        foreach (TPath p in node.Connections.Values)
        {
            if (path1 == null)
            {
                path1 = p; continue;
            }
            else if (path2 == null)
            {
                path2 = p; continue;
            }
            else break;
        }

        if (path1 == null || path2 == null) return;
        if (!path1.Direction.IsParallelTo(path2.Direction)) return;
        TPath newPath = PathUtils.Merge<TPath, TNode>(path1, path2);
        NodeUtils.Disconnect(path1.Source, path1.Dest);
        NodeUtils.Disconnect(path2.Source, path2.Dest);
        _map.RemovePath(path1);
        _map.RemovePath(path2);
        NodeUtils.Connect(newPath.Source, newPath.Dest, newPath);
        _map.AddPath(newPath);
        _map.RemoveNode(node);
    }
}