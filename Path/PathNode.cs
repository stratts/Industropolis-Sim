using System;
using System.Collections.Generic;

public class PathNode {
    public TilePos Pos { get; private set; }
    public IReadOnlyDictionary<PathNode, _Path> Connections => _connections;
    private Dictionary<PathNode, _Path> _connections;

    public PathNode(TilePos pos) {
        Pos = pos;
        _connections =new Dictionary<PathNode, _Path>();
    }

    public void Connect(PathNode node, _Path path) {
        if (node == this) {
            throw new System.ArgumentException("PathNode cannot connect to itself");
        }
        if (_connections.ContainsKey(node)) {
            throw new System.ArgumentException("PathNode is already connected");
        }
        _connections.Add(node, path);
    }

    public void Disconnect(PathNode node) {
         if (!_connections.ContainsKey(node)) {
            throw new System.ArgumentException("Path is not contained in connections");
         }
         _connections.Remove(node);
    }

    public bool IsConnected(PathNode node) => _connections.ContainsKey(node);
}

public class _Path {
    public PathNode Source { get; private set; }
    public PathNode Dest { get; private set; }
    public float Length { get; private set; }

    public IReadOnlyCollection<PathLane> Lanes => _lanes;
    private List<PathLane> _lanes;

    public event EventHandler PathSplit;

    public _Path() {
        
    }

    public void Connect(PathNode source, PathNode dest) {
        _lanes = new List<PathLane>();
        Source = source;
        Dest = dest;
        Length = Source.Pos.Distance(Dest.Pos);
        _lanes.Add(new PathLane(LaneDir.Source));
        _lanes.Add(new PathLane(LaneDir.Dest));
        source.Connect(dest, this);
        dest.Connect(source, this);
    }

    // Split path at given node, and return new paths 
    public (_Path, _Path) Split(PathNode node) {
        var path1 = (_Path)this.MemberwiseClone();
        var path2 = (_Path)this.MemberwiseClone();
        Source.Disconnect(Dest);
        Dest.Disconnect(Source);
        path1.Connect(Source, node);
        path2.Connect(node, Dest);
        if (PathSplit != null) PathSplit(this, null);
        return (path1, path2);
    } 
}

public class PathLane {
    public _Path Parent { get; private set; }
    public LaneDir Direction { get; private set; }

    public PathLane(LaneDir direction) {
        Direction = direction;
    }
}

public enum LaneDir {
    Source,
    Dest
}