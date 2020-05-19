using System;
using System.Collections.Generic;


public class Path {
	public PathNode Source { get; private set; }
	public PathNode Dest { get; private set; }
	public float Length { get; private set; }

	public IReadOnlyCollection<PathLane> Lanes => _lanes;
	private List<PathLane> _lanes;

	public event EventHandler PathSplit;

	public Path() {
		
	}

	public void Connect(PathNode source, PathNode dest) {
		_lanes = new List<PathLane>();
		Source = source;
		Dest = dest;
		Length = Source.Pos.Distance(Dest.Pos);
		_lanes.Add(new PathLane(LaneDir.Source));
		_lanes.Add(new PathLane(LaneDir.Dest));
		source.Connect(dest, this);
		dest.Connect(source, this);
	}

	// Split path at given node, and return new paths 
	public static (Path, Path) Split(Path path, PathNode node) {
		var path1 = (Path)Activator.CreateInstance(path.GetType());
		var path2 = (Path)Activator.CreateInstance(path.GetType());
		path.Source.Disconnect(path.Dest);
		path.Dest.Disconnect(path.Source);
		path1.Connect(path.Source, node);
		path2.Connect(node, path.Dest);
		if (path.PathSplit != null) path.PathSplit(path, null);
		return (path1, path2);
	} 

	public bool OnPath(TilePos pos) {
		if (pos == Source.Pos || pos == Dest.Pos) return true;
		
		TilePos pathDiff = Dest.Pos - Source.Pos;
		TilePos posDiff = pos - Source.Pos;

		if (pathDiff.IsMultipleOf(posDiff)) return true;
		return false;
	}
}



public class PathLane {
	public Path Parent { get; private set; }
	public LaneDir Direction { get; private set; }

	public PathLane(LaneDir direction) {
		Direction = direction;
	}
}

public enum LaneDir {
	Source,
	Dest
}