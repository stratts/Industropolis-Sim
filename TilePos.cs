
using System;

public struct TilePos {
    public int X { get; set; }
    public int Y { get; set; }

    public TilePos(int x, int y) {
        X = x;
        Y = y;
    }

    public static TilePos operator+ (TilePos a, TilePos b) {
        return new TilePos(a.X + b.X, a.Y + b.Y);
    }

    public static bool operator== (TilePos a, TilePos b) {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator!= (TilePos a, TilePos b) {
        return a.X != b.X || a.Y != b.Y;
    }

    public static TilePos operator- (TilePos a, TilePos b) {
        return new TilePos(a.X - b.X, a.Y - b.Y);
    }

    public override bool Equals(object obj) {
        if (obj is TilePos p) {
            return this == p;
        }
        else return false;
    }

    public override string ToString() {
        return $"({X}, {Y})";
    }

    public bool WithinBounds(int width, int height) {
        return (X > 0 && Y > 0 && X < width && Y < height);
    }

    public float Distance(TilePos dest) {
        return (float)Math.Sqrt(Math.Pow(X - dest.X, 2) + Math.Pow(Y - dest.Y, 2));
    }

    public override int GetHashCode() {
        return X.GetHashCode() + Y.GetHashCode();
    }
}