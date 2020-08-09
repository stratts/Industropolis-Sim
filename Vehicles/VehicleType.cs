
namespace Industropolis.Sim
{
    public enum VehicleType
    {
        Hauler
    }

    public static class VehicleFactory
    {
        public static Vehicle Create(VehicleType type, Route route)
        {
            switch (type)
            {
                case VehicleType.Hauler: return new Hauler(route);
                default: return new Hauler(route);
            }
        }

        public static RequiredResources GetRequiredResources(this VehicleType type)
        {
            (Item, int)[] res = type switch
            {
                VehicleType.Hauler => new[] { (Item.Wood, 10) },
                _ => new (Item, int)[0]
            };
            return new RequiredResources(res);
        }
    }
}