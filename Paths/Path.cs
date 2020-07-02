using System;
using System.Collections.Generic;


namespace Industropolis.Sim
{
    public interface IPath
    {
        PathCategory Category { get; }
        PathType PathType { get; }
        IPathNode SourceNode { get; }
        IPathNode DestNode { get; }
        float Length { get; }
        IntVector Direction { get; }
        bool OnPath(IntVector pos);
    }

    public abstract class Path<T> : MapObject, IPath where T : IPathNode
    {
        public abstract PathCategory Category { get; }
        public abstract PathType PathType { get; }

        public IPathNode SourceNode => Source;
        public IPathNode DestNode => Dest;

        public T Source { get; private set; }
        public T Dest { get; private set; }
        public float Length { get; private set; }
        public IntVector Direction { get; private set; }

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
            foreach (var point in Source.Pos.GetPointsBetween(Dest.Pos))
            {
                if (point == pos) return true;
            }
            return false;
        }
    }
}
