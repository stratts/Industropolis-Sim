using System;
using System.Collections.Generic;

public static class PathUtils
{
    public static (TPath, TPath) Split<TPath, TNode>(TPath path, TNode node)
        where TPath : Path<TNode> where TNode : IPathNode
    {
        var path1 = (TPath)Activator.CreateInstance(path.GetType(), path.Source, node);
        var path2 = (TPath)Activator.CreateInstance(path.GetType(), node, path.Dest);
        path.OnPathSplit();
        return (path1, path2);
    }

    public static TPath Merge<TPath, TNode>(TPath path1, TPath path2)
        where TPath : Path<TNode> where TNode : IPathNode
    {
        var nodes1 = new HashSet<TNode>(new[] { path1.Source, path1.Dest });
        var nodes2 = new HashSet<TNode>(new[] { path2.Source, path2.Dest });

        nodes1.SymmetricExceptWith(nodes2);
        var ends = new List<TNode>(nodes1);

        var path = (TPath)Activator.CreateInstance(path1.GetType(), ends[0], ends[1]);
        return path;
    }
}

public abstract class Path<T> : MapObject where T : IPathNode
{
    public abstract PathCategory Category { get; }
    public abstract PathType PathType { get; }

    public T Source { get; private set; }
    public T Dest { get; private set; }
    public float Length { get; private set; }
    public IntVector Direction { get; private set; }

    public event Action? PathSplit;

    public Path(T source, T dest)
    {
        Source = source;
        Dest = dest;
        Length = Source.Pos.Distance(Dest.Pos);
        Direction = Source.Pos.Direction(Dest.Pos);
    }

    public bool OnPath(IntVector pos)
    {
        if (pos == Source.Pos || pos == Dest.Pos) return true;
        if (Source.Pos.FloatDirection(pos) == Dest.Pos.FloatDirection(pos).Negate()) return true;
        return false;
    }

    public void OnPathSplit() => PathSplit?.Invoke();
}