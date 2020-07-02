
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
            if (_building.Input != null) _action = Unload;
            else if (_building.Output != null) _action = Load;
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
                SetDirection(Route.Direction.Forwards);
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
                SetDirection(Route.Direction.Backwards);
                GoNext();
            }
        }
    }
}
