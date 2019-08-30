
public class Mine : ProductionBuilding {
    public Mine(MapInfo map, TilePos pos) {
        Consumer = new ResourceInput(map, pos, 5, Item.Stone);
        var output = new DirectProducer(5, 1, Item.Stone);
        Output = output;
        Producer = output;
        ProcessingTime = 2;
        Width = 2;
        Height = 2;
    }
}