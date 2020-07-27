
namespace Industropolis.Sim.Buildings
{
    public class Farm : ProductionBuilding
    {
        public Farm(Map map, IntVector pos)
        {
            Type = BuildingType.Farm;
            Consumer = new NutrientInput(map, this, (0, 0), 4);
            var producer = new DirectProducer(5, 1, Item.Food);
            Producer = producer;
            Output = producer;
            ProcessingTime = 2;
            Width = 4;
            Height = 4;
            Cost = 50;
        }
    }
}
