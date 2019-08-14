
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

    public override int GetHashCode() {
        return X.GetHashCode() + Y.GetHashCode();
    }
}