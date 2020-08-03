
namespace Industropolis.Sim
{
    public abstract class BaseTileInput : IConsumer
    {
        protected IntVector pos;
        protected Map map;
        protected int size;
        private Building _parent;

        private IntVector? currentTile = null;

        public IntVector Pos => pos;
        public int Size => size;

        public BaseTileInput(Map map, Building parent, IntVector pos, int size)
        {
            _parent = parent;
            this.map = map;
            this.pos = pos;
            this.size = size;
        }

        public bool CanConsume => GetCurrentTile() != null;

        public bool Consume()
        {
            IntVector? t = GetCurrentTile();
            if (!t.HasValue) return false;
            ConsumeResource(ref map.GetTile(t.Value));
            return true;
        }

        private IntVector? GetCurrentTile()
        {
            if (currentTile == null || !HasResource(map.GetTile(currentTile.Value)))
            {
                currentTile = NextTileWithResource();
            }
            return currentTile;
        }

        private IntVector? NextTileWithResource()
        {
            int s = size;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var p = _parent.Pos + pos + (x, y);
                    if (!map.ValidPos(p)) continue;
                    Tile t = map.GetTile(p);
                    if (HasResource(t)) return p;
                }
            }

            return null;
        }

        protected abstract bool HasResource(Tile tile);

        protected abstract void ConsumeResource(ref Tile tile);
    }

    public class ResourceInput : BaseTileInput
    {
        private Item resource;

        public ResourceInput(Map map, Building parent, IntVector pos, int size, Item resource) : base(map, parent, pos, size)
        {
            this.resource = resource;
        }

        protected override bool HasResource(Tile tile)
        {
            return tile.Resource == resource && tile.ResourceCount > 0;
        }

        protected override void ConsumeResource(ref Tile tile)
        {
            tile.ResourceCount--;
        }
    }

    public class NutrientInput : BaseTileInput
    {
        public NutrientInput(Map map, Building parent, IntVector pos, int size) : base(map, parent, pos, size)
        {

        }

        protected override bool HasResource(Tile tile)
        {
            return tile.Nutrients > 0;
        }

        protected override void ConsumeResource(ref Tile tile)
        {
            tile.Nutrients--;
        }
    }
}
