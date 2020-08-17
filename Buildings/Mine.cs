
namespace Industropolis.Sim.Buildings
{
    public class Mine : ProductionBuilding
    {
        public Mine(Map map, IntVector pos)
        {
            var item = map.GetNearestResource(pos + (Width / 2, Height / 2), 2, Item.Stone, Item.IronOre, Item.CopperOre);
            Type = BuildingType.Mine;
            Consumer = new ResourceInput(map, this, (-1, -1), 4, item);
            var output = new DirectProducer(5, 1, item);
            Output = output;
            Producer = output;
            ProcessingTime = 2;
            Width = 2;
            Height = 2;
            Entrance = new BuildingEntrance(this, (0, 1), (0, 2), PathType.Driveway);
        }
    }
}
