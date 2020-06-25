
using System;
using System.Collections.Generic;

public static class FloatTupleExtensions
{
    public static (float x, float y) Negate(this (float x, float y) floats)
    {
        return (-floats.x, -floats.y);
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

    public (float x, float y) FloatDirection(IntVector dest)
    {
        if (dest == this) return (0, 0);
        IntVector dirVector = VectorTo(dest);
        int max = Math.Max(Math.Abs(dirVector.X), Math.Abs(dirVector.Y));
        return ((float)Math.Round((float)dirVector.X / max, 2), (float)Math.Round((float)dirVector.Y / max, 2));
    }

    public IntVector Direction(IntVector dest)
    {
        var dir = FloatDirection(dest);
        return new IntVector((int)Math.Round(dir.x), (int)Math.Round(dir.y));
    }

    public IntVector VectorTo(IntVector dest) => dest - this;

    public float Distance(IntVector dest)
    {
        return (float)Math.Sqrt(Math.Pow(X - dest.X, 2) + Math.Pow(Y - dest.Y, 2));
    }

    public bool IsParallelTo(IntVector vector)
    {
        if (this == vector || this == -vector) return true;
        if (IntVector.Zero.FloatDirection(this) == IntVector.Zero.FloatDirection(vector)) return true;
        if (IntVector.Zero.FloatDirection(this) == IntVector.Zero.FloatDirection(-vector)) return true;
        return false;
    }

    public override int GetHashCode()
    {
        return X.GetHashCode() + Y.GetHashCode();
    }
}
