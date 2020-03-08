
/*public class PopulationOutput : IProducer {
    public Item Item => Item.None;
    public bool CanProduce => _population.Population < _population.MaxPopulation;

    private PopulationInfo _population;

    public PopulationOutput(PopulationInfo population) {
        _population = population;
    }

    public bool Produce() {
        if (!CanProduce) return false;
        _population.Population++;
        return true;
    }
}*/