
using System;
using System.Numerics;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public struct IntVector
    {
        public int X { get; set; }
        public int Y { get; set; }

        public static readonly IntVector Zero = new IntVector(0, 0);

        public IntVector(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static IntVector operator +(IntVector a, IntVector b)
        {
            return new IntVector(a.X + b.X, a.Y + b.Y);
        }

        public static bool operator ==(IntVector a, IntVector b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(IntVector a, IntVector b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public static IntVector operator -(IntVector a, IntVector b)
        {
            return new IntVector(a.X - b.X, a.Y - b.Y);
        }

        public static IntVector operator -(IntVector a)
        {
            return new IntVector(-a.X, -a.Y);
        }

        public override bool Equals(object? obj)
        {
            if (obj is IntVector p)
            {
                return this == p;
            }
            else return false;
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public bool WithinBounds(int width, int height)
        {
            return (X >= 0 && Y >= 0 && X < width && Y < height);
        }

        public IEnumerable<IntVector> Neighbours
        {
            get
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;
                        yield return new IntVector(X + x, Y + y);
                    }
                }
            }
        }

        public IEnumerable<IntVector> GetPointsBetween(IntVector dest)
        {
            var pos = new Vector2(X, Y);
            var diff = new Vector2(dest.X - X, dest.Y - Y);
            var len = Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y));
            var norm = diff / len;
            for (int i = 0; i <= len; i++)
            {
                yield return new IntVector((int)Math.Round(pos.X), (int)Math.Round(pos.Y));
                pos += norm;
            }
        }

        public Vector2 Direction(IntVector dest)
        {
            var vec = (dest - this).ToVector2();
            var norm = vec / vec.Length();
            return new Vector2((float)Math.Round(norm.X, 2), (float)Math.Round(norm.Y, 2));
        }

        public IntVector Direction8(IntVector dest)
        {
            var dir = Direction(dest);
            return new IntVector((int)Math.Round(dir.X), (int)Math.Round(dir.Y));
        }

        public float Distance(IntVector dest) => (dest - this).ToVector2().Length();

        public bool IsParallelTo(IntVector vector)
        {
            if (this == vector || this == -vector) return true;
            if (IntVector.Zero.Direction(this) == IntVector.Zero.Direction(vector)) return true;
            if (IntVector.Zero.Direction(this) == IntVector.Zero.Direction(-vector)) return true;
            return false;
        }

        public Vector2 ToVector2() => new Vector2(X, Y);

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }
    }
}
