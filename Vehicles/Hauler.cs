
namespace Industropolis.Sim
{
    public class Hauler : Vehicle
    {
        public Item Item => Route.Item;

        public int Carrying { get; set; } = 0;
        public int MaxCapacity { get; } = 5;

        public override VehicleType Type => VehicleType.Hauler;

        public Hauler(Route route) : base(route) { }

        protected override void DestinationReached()
        {
            Destination.Occupied = true;
            var building = Destination.GetLink<Building>();
            var action = Route.GetAction(Destination);
            if (action.Type == Route.ActionType.Pickup) _action = Load;
            else if (action.Type == Route.ActionType.Dropoff) _action = Unload;
        }

        private void Load()
        {
            var building = Destination.GetLink<Building>();
            if (Carrying < MaxCapacity && building.Output != null)
            {
                if (building.Output.CanRemove(Item))
                {
                    building.Output.Remove(Item);
                    Carrying++;
                }
            }
            else
            {
                Destination.Occupied = false;
                Reset(Destination, Route.GetNextDestination(RouteIndex));
                GoNext();
            }
        }

        private void Unload()
        {
            var building = Destination.GetLink<Building>();
            if (Carrying > 0 && building.Input != null && building.Input.CanInsert(Item))
            {
                building.Input.Insert(Item);
                Carrying--;
            }
            else
            {
                Destination.Occupied = false;
                Reset(Destination, Route.GetNextDestination(RouteIndex));
                GoNext();
            }
        }
    }
}
