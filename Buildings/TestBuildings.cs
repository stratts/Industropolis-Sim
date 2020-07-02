

namespace Industropolis.Sim
{
    public class TestProducer : Building
    {
        public TestProducer()
        {
            Width = 3;
            Height = 3;
            Output = new InfiniteDirectOutput(Item.Wood);
            Entrance = new BuildingEntrance(this, new IntVector(1, 2));
        }
    }

    public class TestConsumer : Building
    {
        public TestConsumer()
        {
            Width = 2;
            Height = 2;
            Input = new InfiniteDirectInput();
            Entrance = new BuildingEntrance(this, new IntVector(0, 1));
        }
    }
}
