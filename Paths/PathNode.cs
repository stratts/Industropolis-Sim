using System;
using System.Collections.Generic;

public interface IPathNode
{
    IntVector Pos { get; }
    PathCategory Category { get; }
}

public interface IPathNode<T> : IPathNode
{
    void Disconnect(T node);
}

public abstract class PathNode<TNode, TPath> : MapObject, IPathNode<TNode> where TNode : PathNode<TNode, TPath>
{
    public PathCategory Category { get; }

    public IntVector Pos { get; private set; }
    public IReadOnlyDictionary<TNode, TPath> Connections => _connections;
    private Dictionary<TNode, TPath> _connections;

    public abstract event Action<TNode>? Changed;

    public PathNode(IntVector pos, PathCategory category)
    {
        Category = category;
        Pos = pos;
        _connections = new Dictionary<TNode, TPath>();
    }

    public void Connect(TNode node, TPath path)
    {
        if (node == this)
        {
            throw new System.ArgumentException("PathNode cannot connect to itself");
        }
        if (_connections.ContainsKey(node))
        {
            throw new System.ArgumentException($"PathNode {this} is already connected to {node}");
        }
        _connections.Add(node, path);
        OnChange();
    }

    public void Disconnect(TNode node)
    {
        if (!_connections.ContainsKey(node))
        {
            throw new System.ArgumentException("Path is not contained in connections");
        }
        _connections.Remove(node);
        OnChange();
    }

    public abstract void OnChange();

    public bool IsConnected(TNode node) => _connections.ContainsKey(node);

    public override string ToString() => $"{this.Pos}";
}
