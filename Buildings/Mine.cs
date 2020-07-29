
namespace Industropolis.Sim.Buildings
{
    public class Mine : ProductionBuilding
    {
        public Mine(Map map, IntVector pos)
        {
            Type = BuildingType.Mine;
            Consumer = new ResourceInput(map, this, (-1, -1), 4, Item.Stone);
            var output = new DirectProducer(5, 1, Item.Stone);
            Output = output;
            Producer = output;
            ProcessingTime = 2;
            Width = 2;
            Height = 2;
            Entrance = new BuildingEntrance(this, (0, 1), (0, 2), PathType.Driveway);
            SetRequiredResources((Item.Stone, 100));
        }
    }
}
