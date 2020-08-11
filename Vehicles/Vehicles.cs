
namespace Industropolis.Sim.Vehicles
{
    public class Wagon : Hauler
    {
        public override VehicleType Type => VehicleType.Wagon;
        public override int MaxCapacity => 5;
        public override float Length => 0.5f;
        public override float Speed => 1;

        public Wagon(Route route) : base(route) { }
    }

    public class Truck : Hauler
    {
        public override VehicleType Type => VehicleType.Truck;
        public override int MaxCapacity => 20;
        public override float Length => 0.75f;
        public override float Speed => 1.5f;

        public Truck(Route route) : base(route) { }
    }
}