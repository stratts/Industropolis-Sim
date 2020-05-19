using System;
using System.Collections.Generic;

public class Tile {
    public enum SurfaceType {
        Base,
        Path
    }

    private int _resourceCount = 0;
    private SurfaceType _surface;

    public event EventHandler ResourceExhausted;
    public event EventHandler SurfaceChanged;
    public event EventHandler PathChanged;

    public Building Building { get; set; } = null;
    public int Nutrients = 0;
    public Item Resource { get; set; } = Item.None;
    public int ResourceCount {
        get => _resourceCount;
        set {
            if (value <= 0) OnResourceExhausted();
            _resourceCount = value;
        }
    }
    public PathNode Node { get; set; } = null;

    private void OnResourceExhausted() {
        if (ResourceExhausted != null) ResourceExhausted(this, null);
    }

    private void OnSurfaceChanged() {
        if (SurfaceChanged != null) SurfaceChanged(this, null);
    }

    private void OnPathChanged() {
        if (PathChanged != null) PathChanged(this, null);
    }

    public SurfaceType Surface { 
        get {
            return _surface;
        } 
        set {
            _surface = value;
            OnSurfaceChanged();    
        } 
    }

    public float SpeedMultiplier => 1;
    //public List<Entity> Entities { get; set; } = new List<Entity>();
}