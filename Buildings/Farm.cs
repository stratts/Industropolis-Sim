
public class Farm : ProductionBuilding {
    public Farm(MapInfo map, TilePos pos) {
        Type = BuildingType.Farm;
        Consumer = new NutrientInput(map, pos, 3);
        var producer = new DirectProducer(5, 1, Item.Food);
        Producer = producer;
        Output = producer;
        ProcessingTime = 2;
        Width = 2;
        Height = 2;
    }
}