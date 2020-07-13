
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
    }
}