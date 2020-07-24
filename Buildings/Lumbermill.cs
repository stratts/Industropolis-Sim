
namespace Industropolis.Sim.Buildings
{
    public class Lumbermill : ProductionBuilding
    {
        public Lumbermill(Map map, IntVector pos)
        {
            Type = BuildingType.Lumbermill;
            Width = 2;
            Height = 2;
            Consumer = new ResourceInput(map, pos - (1, 1), 4, Item.Wood);
            var producer = new DirectProducer(5, 1, Item.Wood);
            Producer = producer;
            Output = producer;
            ProcessingTime = 1;
            Entrance = new BuildingEntrance(this, (0, 1), (0, 2), PathType.Driveway);
        }
    }
}