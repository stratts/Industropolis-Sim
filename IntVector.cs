
using System;
using System.Collections.Generic;

public static class IntExtensions {
    public static bool IsMultipleOf(this int a, int b) => (a == 0 && b == 0) || (b != 0 && a % b == 0);
}

public struct IntVector {
    public int X { get; set; }
    public int Y { get; set; }

    public static readonly IntVector Zero = new IntVector(0, 0);

    public IntVector(int x, int y) {
        X = x;
        Y = y;
    }

    public static IntVector operator+ (IntVector a, IntVector b) {
        return new IntVector(a.X + b.X, a.Y + b.Y);
    }

    public static bool operator== (IntVector a, IntVector b) {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator!= (IntVector a, IntVector b) {
        return a.X != b.X || a.Y != b.Y;
    }

    public static IntVector operator- (IntVector a, IntVector b) {
        return new IntVector(a.X - b.X, a.Y - b.Y);
    }

    public static IntVector operator- (IntVector a) {
        return new IntVector(-a.X, -a.Y);
    }

    public override bool Equals(object obj) {
        if (obj is IntVector p) {
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

    public IEnumerable<IntVector> Neighbours {
        get {
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    if (x == 0 && y == 0) continue;
                    yield return new IntVector(X + x, Y + y);
                }
            }
        }
    }

    public IntVector Direction(IntVector dest) {
        if (dest == this) return Zero;
        IntVector dirVector = VectorTo(dest);
		int max = Math.Max(Math.Abs(dirVector.X), Math.Abs(dirVector.Y));
		return new IntVector(dirVector.X / max, dirVector.Y / max);
    }

    public IntVector VectorTo(IntVector dest) => dest - this;

    public float Distance(IntVector dest) {
        return (float)Math.Sqrt(Math.Pow(X - dest.X, 2) + Math.Pow(Y - dest.Y, 2));
    }

    public bool IsParallelTo(IntVector vector) {
        if (this == vector || this == -vector) return true;
        if (this.IsMultipleOf(vector) || vector.IsMultipleOf(this)) return true;
        return false;
    }

    public bool IsMultipleOf(IntVector pos) {
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
