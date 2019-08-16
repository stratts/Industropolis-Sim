
public abstract class MapObject : GameObject {
    protected MapInfo MapInfo { get; set; }

    public MapObject(MapInfo mapInfo) {
        this.MapInfo = mapInfo;
    }
}