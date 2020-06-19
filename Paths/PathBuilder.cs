using System;
using System.Collections.Generic;

public enum PathCategory
{
    Road,
    Rail
}

public enum PathType
{
    Road,
    OneWayRoad,
    Rail
}

public class PathBuilder
{
    private Map _map;

    public PathBuilder(Map map)
    {
        _map = map;
    }

    public static Path MakePath(PathType type, PathNode source, PathNode dest)
    {
        switch (type)
        {
            case PathType.Road: return new Road(source, dest);
            case PathType.OneWayRoad: return new OneWayRoad(source, dest);
            case PathType.Rail: return new Rail(source, dest);
            default: return new Road(source, dest);
        }
    }

    public static PathCategory GetCategory(PathType type)
    {
        switch (type)
        {
            case PathType.Road: return PathCategory.Road;
            case PathType.Rail: return PathCategory.Rail;
            default: return default(PathCategory);
        }
    }

    private PathNode AddPathNode(PathCategory category, IntVector pos)
    {
        PathNode? n = _map.GetNode(pos);
        if (n == null || n.Category != category)
        {
            n = new PathNode(pos, category);
            Path? p = _map.GetPath(pos);
            if (p != null && p.Category == category)
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

    public void BuildPath(PathType type, IntVector source, IntVector dest)
    {
        IntVector inc = source.Direction(dest);
        IntVector prev = source;
        IntVector cur = source;
        bool onPath = false;
        var toConnect = new List<Building>();

        while (cur != dest)
        {
            cur += inc;
            Path? p = _map.GetPath(cur);
            PathNode? n = _map.GetNode(cur);
            if (p != null && p.Direction.IsParallelTo(inc))
            {
                if (p.Category != GetCategory(type)) return;
                onPath = true;
            }
            else if (onPath && n != null)
            {
                onPath = false;
                prev = cur;
            }
            else if (!onPath && (cur == dest || p != null || n != null))
            {
                BuildPathSegment(type, prev, cur);
                prev = cur;
            }
            foreach (var building in _map.Buildings)
            {
                if (building.HasEntrance && building.Entrance != null &&
                    building.Entrance.CanConnect(cur, GetCategory(type)))
                {
                    toConnect.Add(building);
                }
            }
        }

        foreach (var building in toConnect) ConnectBuilding(building);
    }

    public void BuildPathSegment(PathType type, IntVector source, IntVector dest)
    {
        var category = GetCategory(type);
        PathNode s = AddPathNode(category, source);
        PathNode d = AddPathNode(category, dest);
        Path path = MakePath(type, s, d);
        path.Connect();
        _map.AddPath(path);
        if (s.Connections.Count == 2) TryMergeNode(s);
        if (d.Connections.Count == 2) TryMergeNode(d);
    }

    public void DeletePathSegment(IntVector pos)
    {
        Path? p = _map.GetPath(pos);
        PathNode? n = _map.GetNode(pos);
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
            newPath.Disconnect();
            _map.RemovePath(newPath);
        }
        else if (n != null)
        {
            var connections = new List<PathNode>(n.Connections.Keys);
            foreach (PathNode connection in connections)
            {
                AddPathNode(n.Category, n.Pos + n.Pos.Direction(connection.Pos));
            }
            _map.RemoveNode(n);
        }
    }

    private void TryMergeNode(PathNode node)
    {
        Path? path1 = null;
        Path? path2 = null;
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
        if (entrance == null)
            throw new ArgumentException("Building does not have an entrance");
        var p = _map.GetPath(entrance.ConnectionPos);
        var n = _map.GetNode(entrance.ConnectionPos);
        if ((p != null && p.Category == entrance.Category) || (n != null && n.Category == entrance.Category))
        {
            var node = new BuildingNode(entrance.Pos, building);
            entrance.Connect(node);
            if (entrance.Node == null) throw new Exception("Could not connect entrance");
            _map.AddNode(entrance.Node);
            BuildPath(PathType.Road, entrance.Pos, entrance.ConnectionPos);
        }
    }

    public void DisconnectBuilding(Building building)
    {
        if (building.Entrance == null || building.Entrance.Node == null)
            throw new ArgumentException("Building does not have an entrance node");
        PathNode n = building.Entrance.Node;
        PathNode? pathCon = _map.GetNode(n.Pos + new IntVector(0, 1));
        _map.RemoveNode(n);
        if (pathCon != null && pathCon.Connections.Count == 2) TryMergeNode(pathCon);
        building.Entrance.Disconnect();
    }
}
