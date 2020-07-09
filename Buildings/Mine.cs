
namespace Industropolis.Sim
{
    public class Mine : ProductionBuilding
    {
        public Mine(Map map, IntVector pos)
        {
            Type = BuildingType.Mine;
            Consumer = new ResourceInput(map, pos, 5, Item.Stone);
            var output = new DirectProducer(5, 1, Item.Stone);
            Output = output;
            Producer = output;
            ProcessingTime = 2;
            Width = 2;
            Height = 2;
            Entrance = new BuildingEntrance(this, new IntVector(0, 1), PathCategory.Road);
        }
    }
}
