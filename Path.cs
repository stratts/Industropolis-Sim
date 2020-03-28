
public abstract class Path {
    public double SpeedMultiplier { get; set; } = 1;
    public int Elevation { get; set; } = 0;
    public bool CanElevate { get; set; } = false;
    
    public TilePos Position { get; set; }

    public abstract bool CanConnect(Path path);
    public abstract bool CanCoExist(Path path);
}

public abstract class RoadPath : Path
{
    public override bool CanCoExist(Path path) => !(path is RoadPath);
    public override bool CanConnect(Path path) => path is RoadPath;
}

public class DirtPath : RoadPath {
    public DirtPath() {
        SpeedMultiplier = 1.5;
    }
}