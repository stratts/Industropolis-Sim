
using System;
using System.Numerics;
using System.Collections.Generic;

namespace Industropolis.Sim
{
    public static class VectorExtensions
    {
        public static bool IsParallelTo(this Vector2 v1, Vector2 v2)
        {
            var n1 = Vector2.Normalize(v1).Round(2);
            var n2 = Vector2.Normalize(v2).Round(2);
            return n1 == n2 || n1 == -n2;
        }

        public static Vector2 Round(this Vector2 v, int digits = 0)
        {
            return new Vector2((float)Math.Round(v.X, digits), (float)Math.Round(v.Y, digits));
        }
    }

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

        public static IntVector Parse(Span<char> input)
        {
            Span<char> trimChars = stackalloc char[] { '(', ')' };
            var trimmed = input.Trim(trimChars);
            var sep = trimmed.IndexOf(',');
            var x = trimmed.Slice(0, sep).Trim();
            var y = trimmed.Slice(sep + 1).Trim();
            return (int.Parse(x), int.Parse(y));
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

        public Vector2 Direction(IntVector dest) => Vector2.Normalize((dest - this).ToVector2()).Round(2);

        public IntVector Direction8(IntVector dest)
        {
            var dir = Direction(dest).Round();
            return new IntVector((int)dir.X, (int)dir.Y);
        }

        public float Distance(IntVector dest) => (dest - this).ToVector2().Length();

        public bool IsParallelTo(IntVector vector) => this.ToVector2().IsParallelTo(vector.ToVector2());

        public Vector2 ToVector2() => new Vector2(X, Y);

        public static implicit operator IntVector((int x, int y) t) => new IntVector(t.x, t.y);

        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode();
        }
    }
}
