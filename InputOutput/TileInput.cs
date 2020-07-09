
namespace Industropolis.Sim
{
    public abstract class BaseTileInput : IConsumer
    {
        protected IntVector pos;
        protected Map map;
        protected int size;

        private Tile? currentTile = null;

        public BaseTileInput(Map map, IntVector pos, int size)
        {
            this.map = map;
            this.pos = pos;
            this.size = size;
        }

        public bool CanConsume => GetCurrentTile() != null;

        public bool Consume()
        {
            Tile? t = GetCurrentTile();
            if (t == null) return false;
            ConsumeResource(t);
            return true;
        }

        private Tile? GetCurrentTile()
        {
            if (currentTile == null || !HasResource(currentTile))
            {
                currentTile = NextTileWithResource();
            }
            return currentTile;
        }

        private Tile? NextTileWithResource()
        {
            int s = size / 2;
            for (int x = -s; x <= s; x++)
            {
                for (int y = -s; y <= s; y++)
                {
                    Tile? t = map.GetTile(new IntVector(pos.X + x, pos.Y + y));
                    if (t != null)
                    {
                        if (HasResource(t))
                            return t;
                    }
                }
            }

            return null;
        }

        protected abstract bool HasResource(Tile tile);

        protected abstract void ConsumeResource(Tile tile);
    }

    public class ResourceInput : BaseTileInput
    {
        private Item resource;

        public ResourceInput(Map map, IntVector pos, int size, Item resource) : base(map, pos, size)
        {
            this.resource = resource;
        }

        protected override bool HasResource(Tile tile)
        {
            return tile.Resource == resource && tile.ResourceCount > 0;
        }

        protected override void ConsumeResource(Tile tile)
        {
            tile.ResourceCount--;
        }
    }

    public class NutrientInput : BaseTileInput
    {
        public NutrientInput(Map map, IntVector pos, int size) : base(map, pos, size)
        {

        }

        protected override bool HasResource(Tile tile)
        {
            return tile.Nutrients > 0;
        }

        protected override void ConsumeResource(Tile tile)
        {
            tile.Nutrients--;
        }
    }
}
