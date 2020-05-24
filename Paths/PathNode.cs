using System;
using System.Collections.Generic;

public class PathNode : MapObject {
	public IntVector Pos { get; private set; }
	public IReadOnlyDictionary<PathNode, Path> Connections => _connections;
	private Dictionary<PathNode, Path> _connections;

	public PathNode(IntVector pos) {
		Pos = pos;
		_connections =new Dictionary<PathNode, Path>();
	}

	public void Connect(PathNode node, Path path) {
		if (node == this) {
			throw new System.ArgumentException("PathNode cannot connect to itself");
		}
		if (_connections.ContainsKey(node)) {
			throw new System.ArgumentException("PathNode is already connected");
		}
		_connections.Add(node, path);
	}

	public void Disconnect(PathNode node) {
		 if (!_connections.ContainsKey(node)) {
			throw new System.ArgumentException("Path is not contained in connections");
		 }
		 _connections.Remove(node);
	}

	public bool IsConnected(PathNode node) => _connections.ContainsKey(node);
}
