
public class House : ProductionBuilding {
    public House(PopulationInfo population) {
        var input = new DirectConsumer(5, 1, Item.Food);
        Input = input;
        Consumer = input;
        Producer = new PopulationOutput(population);
        ProcessingTime = 5;
    }
}