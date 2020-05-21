
using System;
using System.Collections.Generic;

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

    public static TilePos operator- (TilePos a) {
        return new TilePos(-a.X, -a.Y);
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
        return (X >= 0 && Y >= 0 && X < width && Y < height);
    }

    public IEnumerable<TilePos> Neighbours {
        get {
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if (x == 0 && y == 0) continue;
                    yield return new TilePos(X + x, Y + y);
                }
            }
        }
    }

    public (int x, int y) Direction(TilePos dest) {
        if (dest == this) return (0, 0);
        TilePos diff = dest - this;
		int max = Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y));
		return (diff.X / max, diff.Y / max);
    }

    public float Distance(TilePos dest) {
        return (float)Math.Sqrt(Math.Pow(X - dest.X, 2) + Math.Pow(Y - dest.Y, 2));
    }

    public bool IsMultipleOf(TilePos pos) {
        int x, y;
        if (pos.X == 0) x = 1;
        else x = pos.X;
        if (pos.Y == 0) y = 1;
        else y = pos.Y;
        var xMul = Math.Round((float)X / (float)pos.X, 1);
        var yMul = Math.Round((float)Y / (float)pos.Y, 1);
        if (X == 0 && pos.X == 0) xMul = yMul;
        if (Y == 0 && pos.Y == 0) yMul = xMul;

        if (xMul == yMul && xMul >= 1 && yMul >= 1) return true;
        return false;
    }

    public override int GetHashCode() {
        return X.GetHashCode() + Y.GetHashCode();
    }
}