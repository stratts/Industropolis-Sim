using System.Collections.Generic;
using Industropolis.Sim.Buildings;

namespace Industropolis.Sim
{
    public enum BuildingType
    {
        None,
        Workshop,
        //House,
        Mine,
        Farm,
        Lumbermill,
        TestProducer,
        TestConsumer,
        TrainStation,
        Core
    }

    public static class BuildingTypeExtensions
    {
        public static IReadOnlyDictionary<Item, int>? GetRequiredResources(this BuildingType type) => BuildingFactory.GetRequiredResources(type);
    }

    public static class BuildingFactory
    {
        private static Map _map = new Map();

        public static Building Create(Map map, BuildingType type, IntVector pos)
        {
            switch (type)
            {
                case BuildingType.Workshop: return new Workshop();
                //case BuildingType.House: building = new House(this.Population); break;
                case BuildingType.Mine: return new Mine(map, pos);
                case BuildingType.Farm: return new Farm(map, pos);
                case BuildingType.TestConsumer: return new TestConsumer();
                case BuildingType.TestProducer: return new TestProducer();
                case BuildingType.TrainStation: return new TrainStation(map);
                case BuildingType.Core: return new Core(map);
                case BuildingType.Lumbermill: return new Lumbermill(map, pos);
                default: return new TestProducer();
            }
        }

        public static IReadOnlyDictionary<Item, int>? GetRequiredResources(BuildingType type)
        {
            return Create(_map, type, IntVector.Zero).RequiredResources;
        }
    }
}