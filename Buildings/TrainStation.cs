

namespace Industropolis.Sim.Buildings
{
    public class TrainStation : Building
    {
        private Map _map;

        public TrainStation(Map map)
        {
            _map = map;
            Width = 2;
            Height = 4;
            Input = new InfiniteDirectInput();
            Output = new InfiniteDirectOutput(Item.Wood);
            Type = BuildingType.TrainStation;
            Entrance = new BuildingEntrance(this, (2, 0), (2, 3), PathType.Rail);
        }
    }
}