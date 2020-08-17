using OpenSimplex;

namespace Industropolis.Sim
{
    public static class MapGenerator
    {

        private static OpenSimplexNoise _noise = new OpenSimplexNoise(5);

        private struct ResourceDef
        {
            public Item Item { get; set; }
            public double Scale { get; set; }
            public double Threshold { get; set; }
            public double Amount { get; set; }
        }

        private static ResourceDef[] Resources = new[] {
            new ResourceDef() {
                Item = Item.Stone,
                Scale = 1,
                Threshold = 1,
                Amount = 1
            },
            new ResourceDef() {
                Item = Item.Wood,
                Scale = 1.2,
                Threshold = 0.8,
                Amount = 1
            },
            new ResourceDef() {
                Item = Item.IronOre,
                Scale = 1,
                Threshold = 1,
                Amount = 1
            },
            new ResourceDef() {
                Item = Item.CopperOre,
                Scale = 1,
                Threshold = 1,
                Amount = 1
            }
    };

        public static Tile[,] GenerateChunk(int x, int y, int width, int height, long seed)
        {
            var noise = new OpenSimplexNoise(seed);
            var tiles = new Tile[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    tiles[i, j] = new Tile();
                }
            }

            foreach (ResourceDef resource in Resources)
            {
                GenerateResource(x * width, y * height, tiles, resource, seed);
            }

            GenerateNutrients(x * width, y * height, tiles, seed);

            return tiles;
        }

        private static void GenerateResource(int offsetX, int offsetY, Tile[,] tiles, ResourceDef resource, long seed)
        {
            var noise = new OpenSimplexNoise(seed + (int)resource.Item);
            var scale = 10 * resource.Scale;
            var threshold = 0.6 * resource.Threshold;
            var amount = 500 * resource.Amount;

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    double _x = x + offsetX;
                    double _y = y + offsetY;
                    double value = noise.EvaluateOctave(_x / scale, _y / scale, 2, 0.5);
                    if (value > threshold)
                    {

                        tiles[x, y].Resource = resource.Item;
                        tiles[x, y].ResourceCount = (short)(amount * ((value - threshold) / (1 - threshold)));
                    }
                }
            }
        }

        private static void GenerateNutrients(int offsetX, int offsetY, Tile[,] tiles, long seed)
        {
            var noise = new OpenSimplexNoise(seed + 256);
            var scale = 30;
            var amount = 500;

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    double _x = x + offsetX;
                    double _y = y + offsetY;
                    double value = (noise.EvaluateOctave(_x / scale, _y / scale, 2, 0.5) + 0.5) / 1.5;
                    if (value < 0) continue;
                    tiles[x, y].Nutrients = (short)(amount * value);
                }
            }
        }
    }
}
