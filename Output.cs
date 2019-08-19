
public interface IProducer {
    Item Item { get; }
    bool CanProduce { get; }
    bool Produce();
}

public class DirectOutput : DirectBase, IProducer {
    public int ProduceAmount { get; set; }

    public bool CanProduce => buffer <= bufferSize - ProduceAmount;
    public bool CanRemove => buffer > 0;

    public DirectOutput(int bufferSize, int produceAmount, Item item) {
        this.bufferSize = bufferSize;
        this.Item = item;
        this.ProduceAmount = produceAmount;
    }

    public bool Produce() {
        if (CanProduce) {
            buffer += ProduceAmount;
            return true;
            
        }
        return false;
    }

    public bool Remove() {
        if (!CanRemove) return false;
        buffer -= 1;
        return true;
    }
}

public class PopulationOutput : IProducer {
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
}