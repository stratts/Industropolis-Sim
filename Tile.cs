using System;
using System.Collections.Generic;

public class Tile
{
    public enum SurfaceType
    {
        Base,
        Path
    }

    private int _resourceCount = 0;
    private SurfaceType _surface;

    public event Action? ResourceExhausted;
    public event Action? SurfaceChanged;
    public event Action? PathChanged;

    public Building? Building { get; set; } = null;
    public int Nutrients = 0;
    public Item Resource { get; set; } = Item.None;
    public int ResourceCount
    {
        get => _resourceCount;
        set
        {
            if (value <= 0) ResourceExhausted?.Invoke();
            _resourceCount = value;
        }
    }
    public PathNode? Node { get; set; } = null;

    public SurfaceType Surface
    {
        get
        {
            return _surface;
        }
        set
        {
            _surface = value;
            SurfaceChanged?.Invoke();
        }
    }

    public float SpeedMultiplier => 1;
    //public List<Entity> Entities { get; set; } = new List<Entity>();
}