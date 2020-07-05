
namespace Industropolis.Sim
{
    public class Hauler : Vehicle
    {
        public Item Item => Route.Item;

        public int Carrying { get; private set; } = 0;
        public int MaxCapacity { get; } = 50;

        private Building? _building;

        public Hauler(Route route) : base(route) { }

        protected override void DestinationReached()
        {
            _building = ((BuildingNode)Destination).Building;
            if (_building.Output != null && Carrying <= 0) _action = Load;
            else if (_building.Input != null) _action = Unload;
        }

        private void Load()
        {
            if (Carrying < MaxCapacity)
            {
                _building!.Output!.Remove(Item);
                Carrying++;
            }
            else
            {
                FlipDirection();
                GoNext();
            }
        }

        private void Unload()
        {
            if (Carrying > 0)
            {
                _building!.Input!.Insert(Item);
                Carrying--;
            }
            else
            {
                FlipDirection();
                GoNext();
            }
        }
    }
}
