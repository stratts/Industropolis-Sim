
namespace Industropolis.Sim.Buildings
{
    public class Core : Building
    {
        public Core(Map map)
        {
            Input = new GlobalResourceInput(map);
            Width = 4;
            Height = 4;
            Entrance = new BuildingEntrance(this, (1, 3), (1, 4), PathType.Driveway);
            Type = BuildingType.Core;
        }
    }
}