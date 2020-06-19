using System;
using System.Collections.Generic;

public class PathNode : MapObject
{
    public PathCategory Category { get; }

    public IntVector Pos { get; private set; }
    public IReadOnlyDictionary<PathNode, Path> Connections => _connections;
    private Dictionary<PathNode, Path> _connections;

    public bool Occupied { get; set; } = false;

    public event Action<PathNode>? Changed;

    public PathNode(IntVector pos, PathCategory category)
    {
        Category = category;
        Pos = pos;
        _connections = new Dictionary<PathNode, Path>();
    }

    public void Connect(PathNode node, Path path)
    {
        if (node == this)
        {
            throw new System.ArgumentException("PathNode cannot connect to itself");
        }
        if (_connections.ContainsKey(node))
        {
            throw new System.ArgumentException("PathNode is already connected");
        }
        _connections.Add(node, path);
        Changed?.Invoke(this);
    }

    public void Disconnect(PathNode node)
    {
        if (!_connections.ContainsKey(node))
        {
            throw new System.ArgumentException("Path is not contained in connections");
        }
        _connections.Remove(node);
        Changed?.Invoke(this);
    }

    public bool IsConnected(PathNode node) => _connections.ContainsKey(node);

    public bool HasPathTo(PathNode node) => IsConnected(node) && _connections[node].HasLaneTo(node);

    public bool CanProceed(PathNode source, PathNode dest) =>
        !Occupied &&
        (dest == this ||
        !_connections[dest].GetLaneFrom(this).AtCapacity);
}
