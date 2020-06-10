
public class Hauler : Vehicle
{
    public Item Item => Route.Item;

    public Hauler(Route route) : base(route) { }

    protected override void DestinationReached()
    {
        var building = ((BuildingNode)Destination).Building;
        if (building.Input != null) _action = Unload;
        else if (building.Output != null) _action = Load;
    }

    private void Load()
    {
        SetDirection(Route.Direction.Forwards);
        GoNext();
    }

    private void Unload()
    {
        SetDirection(Route.Direction.Backwards);
        GoNext();
    }


}