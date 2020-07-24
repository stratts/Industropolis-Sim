

namespace Industropolis.Sim.Buildings
{
    public class TestProducer : Building
    {
        public TestProducer()
        {
            Width = 3;
            Height = 3;
            Output = new InfiniteDirectOutput(Item.Wood);
            Entrance = new BuildingEntrance(this, (1, 2), (1, 3), PathType.Driveway);
            Type = BuildingType.TestProducer;
        }
    }

    public class TestConsumer : Building
    {
        public TestConsumer()
        {
            Width = 2;
            Height = 2;
            Input = new InfiniteDirectInput();
            Entrance = new BuildingEntrance(this, (0, 1), (0, 2), PathType.Driveway);
            Type = BuildingType.TestConsumer;
        }
    }
}
