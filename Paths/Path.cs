using System;
using System.Collections.Generic;


public class Path : MapObject {
	public PathNode Source { get; private set; }
	public PathNode Dest { get; private set; }
	public float Length { get; private set; }
	public IntVector Direction { get; private set; }

	public IReadOnlyCollection<PathLane> Lanes => _lanes;
	private List<PathLane> _lanes;

	public event Action PathSplit;

	public Path() { }

	public Path(PathNode source, PathNode dest) {
		SetNodes(source, dest);
	}

	public void SetNodes(PathNode source, PathNode dest) {
		Source = source;
		Dest = dest;
		Length = Source.Pos.Distance(Dest.Pos);
		Direction = Source.Pos.Direction(Dest.Pos);
	}

	public void Connect() {
		_lanes = new List<PathLane>();
		_lanes.Add(new PathLane(LaneDir.Source));
		_lanes.Add(new PathLane(LaneDir.Dest));
		Source.Connect(Dest, this);
		Dest.Connect(Source, this);
	}

	public void Disconnect() {
		Source.Disconnect(Dest);
		Dest.Disconnect(Source);
		_lanes.Clear();
	}

	// Split path at given node, and return new paths 
	public static (Path, Path) Split(Path path, PathNode node) {
		var path1 = (Path)Activator.CreateInstance(path.GetType());
		var path2 = (Path)Activator.CreateInstance(path.GetType());
		path.Disconnect();
		path1.SetNodes(path.Source, node);
		path2.SetNodes(node, path.Dest);
		path.PathSplit?.Invoke();
		return (path1, path2);
	} 

	// Merge two paths into one, return new path
	public static Path Merge(Path path1, Path path2) {
		PathNode start;
		PathNode end;

		// Todo: handle different path types
		if (path1.Source == path2.Source) {
			start = path1.Dest;
			end = path2.Dest;
		}
		else if (path1.Dest == path2.Dest) {
			start = path1.Source;
			end = path2.Source;
		}
		else if (path1.Dest == path2.Source) {
			start = path1.Source;
			end = path2.Dest;
		}
		else {
			start = path2.Source;
			end = path1.Dest;
		}
		var path = new Path(start, end);
		return path;
	}

	public bool OnPath(IntVector pos) {
		if (pos == Source.Pos || pos == Dest.Pos) return true;
		
		IntVector pathDiff = Dest.Pos - Source.Pos;
		IntVector posDiff = pos - Source.Pos;

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
