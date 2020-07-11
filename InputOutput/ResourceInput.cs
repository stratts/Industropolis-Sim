namespace Industropolis.Sim
{
    public class GlobalResourceInput : IDirectInput
    {
        private Map _map;

        public GlobalResourceInput(Map map)
        {
            _map = map;
        }

        public bool Accepts(Item item) => true;

        public bool CanInsert(Item item) => true;

        public bool Insert(Item item)
        {
            if (!CanInsert(item)) return false;
            _map.AddResource(item, 1);
            return true;
        }
    }
}