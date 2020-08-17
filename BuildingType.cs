using System.Collections.Generic;
using Industropolis.Sim.Buildings;

namespace Industropolis.Sim
{
    using static BuildingType;

    public enum BuildingType
    {
        None,
        Workshop,
        Smelter,
        //House,
        Mine,
        Farm,
        Lumbermill,
        TestProducer,
        TestConsumer,
        TrainStation,
        Core
    }

    public static class BuildingFactory
    {
        private static Map _map = new Map();

        public static Building Create(Map map, BuildingType type, IntVector pos)
        {
            switch (type)
            {
                case Workshop: return new Workshop();
                //case BuildingType.House: building = new House(this.Population); break;
                case Mine: return new Mine(map, pos);
                case Farm: return new Farm(map, pos);
                case TestConsumer: return new TestConsumer();
                case TestProducer: return new TestProducer();
                case TrainStation: return new TrainStation(map);
                case Core: return new Core(map);
                case Lumbermill: return new Lumbermill(map, pos);
                case Smelter: return new Smelter();
                default: return new TestProducer();
            }
        }

        public static RequiredResources GetRequiredResources(this BuildingType type)
        {
            return type switch
            {
                Lumbermill => new[] { (Item.Wood, 100) },
                Mine => new[] { (Item.Stone, 100) },
                Workshop => new[] { (Item.Stone, 100), (Item.Wood, 100) },
                _ => RequiredResources.None
            };
        }
    }
}