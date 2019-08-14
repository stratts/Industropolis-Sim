
public abstract class MapObject {
    protected MapInfo MapInfo { get; set; }

    public MapObject(MapInfo mapInfo) {
        this.MapInfo = mapInfo;
    }
}