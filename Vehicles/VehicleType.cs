using System;
using Industropolis.Sim.Vehicles;

namespace Industropolis.Sim
{
    public enum VehicleType
    {
        Wagon,
        Truck
    }

    public static class VehicleFactory
    {
        public static Vehicle Create(VehicleType type, Route route)
        {
            switch (type)
            {
                case VehicleType.Wagon: return new Wagon(route);
                case VehicleType.Truck: return new Truck(route);
                default: throw new ArgumentException("Invalid vehicle type");
            }
        }

        public static RequiredResources GetRequiredResources(this VehicleType type)
        {
            (Item, int)[] res = type switch
            {
                VehicleType.Wagon => new[] { (Item.Wood, 10) },
                VehicleType.Truck => new[] { (Item.Wood, 10), (Item.Stone, 10) },
                _ => new (Item, int)[0]
            };
            return new RequiredResources(res);
        }
    }
}