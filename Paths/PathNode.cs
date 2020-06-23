using System;
using System.Collections.Generic;

public interface IPathNode
{
    IntVector Pos { get; }
}

public interface IPathNode<T> : IPathNode
{
    void Disconnect(T node);
}

public static class NodeUtils
{
    public static void Connect<TNode, TPath>(TNode source, TNode dest, TPath path) where TNode : BaseNode<TNode, TPath>
    {
        source.Connect(dest, path);
        dest.Connect(source, path);
    }

    public static void Disconnect<TNode>(TNode source, TNode dest) where TNode : IPathNode<TNode>
    {
        source.Disconnect(dest);
        dest.Disconnect(source);
    }
}

public abstract class BaseNode<TNode, TPath> : MapObject, IPathNode<TNode> where TNode : BaseNode<TNode, TPath>
{
    public PathCategory Category { get; }

    public IntVector Pos { get; private set; }
    public IReadOnlyDictionary<TNode, TPath> Connections => _connections;
    private Dictionary<TNode, TPath> _connections;

    public abstract event Action<TNode>? Changed;

    public BaseNode(IntVector pos, PathCategory category)
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

public class PathNode : BaseNode<PathNode, Path>
{
    public bool Occupied { get; set; } = false;

    public PathNode(IntVector pos, PathCategory category) : base(pos, category)
    {
    }

    public override event Action<PathNode>? Changed;

    public override void OnChange() => Changed?.Invoke(this);

    public bool HasPathTo(PathNode node) => IsConnected(node) && Connections[node].HasLaneTo(node);

    public bool CanProceed(PathNode source, PathNode dest) =>
        !Occupied &&
        (dest == this ||
        !Connections[dest].GetLaneFrom(this).AtCapacity);
}
