using NoiseTest;

public static class MapGenerator {

    public static Tile[,] GenerateTiles(int width, int height, long seed) {
        var noise = new OpenSimplexNoise(seed);
        var tiles = new Tile[width,height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var t = new Tile();
                tiles[x,y] = t;
                var value = noise.Evaluate(((float)x)/10, ((float)y)/10);
                if (value > 0.6) {
                    t.Resource = Item.Stone;
                    t.ResourceCount = (int)(500 * ((value - 0.6)/(1 - 0.6)));
                }
            }
        }

        return tiles;
    }
}