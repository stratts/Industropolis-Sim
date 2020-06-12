using System;
using System.Collections.Generic;


public class PathBuilder
{
    private Map _map;

    public PathBuilder(Map map)
    {
        _map = map;
    }

    private PathNode AddPathNode(IntVector pos)
    {
        PathNode n = _map.GetNode(pos);
        if (n == null)
        {
            n = new PathNode(pos);
            Path p = _map.GetPath(pos);
            if (p != null)
            {
                var (path1, path2) = Path.Split(p, n);
                path1.Connect();
                path2.Connect();
                _map.AddPath(path1);
                _map.AddPath(path2);
                _map.RemovePath(p);
            }
            _map.AddNode(n);
        }
        return n;
    }

    public void BuildPath<T>(IntVector source, IntVector dest) where T : Path, new()
    {
        IntVector inc = source.Direction(dest);
        IntVector prev = source;
        IntVector cur = source;
        bool onPath = false;
        var toConnect = new List<Building>();

        while (cur != dest)
        {
            cur += inc;
            Path p = _map.GetPath(cur);
            PathNode n = _map.GetNode(cur);
            if (p != null && p.Direction.IsParallelTo(inc))
            {
                onPath = true;
            }
            else if (onPath && n != null)
            {
                onPath = false;
                prev = cur;
            }
            else if (!onPath && (cur == dest || p != null || n != null))
            {
                BuildPathSegment<T>(prev, cur);
                prev = cur;
            }
            foreach (var building in _map.Buildings)
            {
                if (building.HasEntrance && building.Entrance.ConnectionPos == cur
                    && !building.Entrance.Connected)
                {
                    toConnect.Add(building);
                }
            }
        }

        foreach (var building in toConnect) ConnectBuilding(building);
    }

    public void BuildPathSegment<T>(IntVector source, IntVector dest) where T : Path, new()
    {
        PathNode s = AddPathNode(source);
        PathNode d = AddPathNode(dest);
        Path path = new T();
        path.SetNodes(s, d);
        path.Connect();
        _map.AddPath(path);
        if (s.Connections.Count == 2) TryMergeNode(s);
        if (d.Connections.Count == 2) TryMergeNode(d);
    }

    public void DeletePathSegment(IntVector pos)
    {
        Path p = _map.GetPath(pos);
        PathNode n = _map.GetNode(pos);
        if (p == null && n == null)
        {
            throw new ArgumentException($"Position {pos} does not contain a path to delete");
        }
        if (p != null)
        {
            AddPathNode(pos + p.Direction);
            AddPathNode(pos - p.Direction);
            var newPath = _map.GetPath(pos);
            newPath.Disconnect();
            _map.RemovePath(newPath);
        }
        else if (n != null)
        {
            var connections = new List<PathNode>(n.Connections.Keys);
            foreach (PathNode connection in connections)
            {
                AddPathNode(n.Pos + n.Pos.Direction(connection.Pos));
            }
            _map.RemoveNode(n);
        }
    }

    private void TryMergeNode(PathNode node)
    {
        Path path1 = null;
        Path path2 = null;
        foreach (Path p in node.Connections.Values)
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

        Path newPath = Path.Merge(path1, path2);
        path1.Disconnect();
        path2.Disconnect();
        _map.RemovePath(path1);
        _map.RemovePath(path2);
        newPath.Connect();
        _map.AddPath(newPath);
        _map.RemoveNode(node);
    }

    public void ConnectBuilding(Building building)
    {
        var entrance = building.Entrance;
        if (_map.GetPath(entrance.ConnectionPos) != null || _map.GetNode(entrance.ConnectionPos) != null)
        {
            var node = new BuildingNode(entrance.Pos, building);
            entrance.Connect(node);
            _map.AddNode(entrance.Node);
            BuildPath<Path>(entrance.Pos, entrance.ConnectionPos);
        }
    }

    public void DisconnectBuilding(Building building)
    {
        PathNode n = building.Entrance.Node;
        PathNode pathCon = _map.GetNode(n.Pos + new IntVector(0, 1));
        _map.RemoveNode(n);
        if (pathCon != null && pathCon.Connections.Count == 2) TryMergeNode(pathCon);
        building.Entrance.Disconnect();
    }
}