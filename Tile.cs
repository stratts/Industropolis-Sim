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

    private void OnResourceExhausted() {
        if (ResourceExhausted != null) ResourceExhausted(this, null);
    }

    private void OnSurfaceChanged() {
        if (SurfaceChanged != null) SurfaceChanged(this, null);
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

    public float SpeedMultiplier {
        get {
            switch (Surface) {
                case SurfaceType.Path: return 2f;
                default: return 1;
            }
        }
    }
    //public List<Entity> Entities { get; set; } = new List<Entity>();
}