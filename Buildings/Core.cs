
namespace Industropolis.Sim
{
    public class Core : Building
    {
        public Core(Map map)
        {
            Input = new GlobalResourceInput(map);
            Width = 4;
            Height = 4;
            Entrance = new BuildingEntrance(this, new IntVector(1, 3), PathCategory.Road);
        }
    }
}