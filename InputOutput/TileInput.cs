
public class TileInput : IConsumer
{
    private TilePos pos;
    private MapInfo map;
    private Item resource;
    private int size;

    private Tile currentTile = null;

    public TileInput(MapInfo map, TilePos pos, int size, Item resource) {
        this.map = map;
        this.pos = pos;
        this.resource = resource;
        this.size = size;
    }

    public bool CanConsume => GetCurrentTile() != null;

    public bool Consume()
    {
        Tile t = GetCurrentTile();
        if (t == null) return false;
        t.ResourceCount--;
        return true;
    }

    private Tile GetCurrentTile() {
        if (currentTile == null || currentTile.ResourceCount <= 0) {
            currentTile = NextTileWithResource();
        }
        return currentTile;
    }

    private Tile NextTileWithResource() {
        int s = size / 2;
        for (int x = -s; x <= s; x++) {
            for (int y = -s; y <= s; y++) {
                Tile t = map.GetTile(new TilePos(pos.X + x, pos.Y + y));
                if (t != null) {
                    if (t.Resource == resource && t.ResourceCount > 0) 
                        return t;
                }
            }
        }

        return null;
    }
}