using System;
using System.Collections.Generic;

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
}

public class PathContainer<TNode, TPath> : IPathContainer<TNode, TPath>
    where TNode : PathNode<TNode, TPath> where TPath : Path<TNode>
{
    public event Action<TNode>? NodeAdded;
    public event Action<TPath>? PathAdded;

    private List<TNode> _nodes = new List<TNode>();
    private List<TPath> _paths = new List<TPath>();

    public TNode? GetNode(IntVector pos)
    {
        foreach (TNode node in _nodes)
        {
            if (node.Pos == pos) return node;
        }

        return null;
    }

    public void AddNode(TNode node)
    {
        _nodes.Add(node);
        NodeAdded?.Invoke(node);
    }

    public void RemoveNode(TNode node)
    {
        var _paths = new List<TPath>(node.Connections.Values);
        foreach (TPath path in _paths)
        {
            NodeUtils.Disconnect(path.Source, path.Dest);
            RemovePath(path);
        }
        _nodes.Remove(node);
        node.Remove();
    }

    public void AddPath(TPath path)
    {
        this._paths.Add(path);
        path.PathSplit += () =>
        {
            RemovePath(path);
        };
        PathAdded?.Invoke(path);
    }

    public TPath? GetPath(IntVector pos)
    {
        if (GetNode(pos) == null)
        {
            foreach (TPath path in GetPaths(pos)) return path;
        }
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
        this._paths.Remove(path);
        path.Remove();
    }

}