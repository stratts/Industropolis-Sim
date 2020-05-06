using System.Collections.Generic;

public class PathNode {
    public TilePos Pos { get; private set; }
    public HashSet<PathNode> Connections { get; }

    public PathNode(TilePos pos) {
        Pos = pos;
        Connections = new HashSet<PathNode>();
    }

    public void Connect(PathNode node) {
        if (node == this) {
            throw new System.ArgumentException("PathNode cannot connect to itself");
        }
        Connections.Add(node);
    }

    public void Disconnect(PathNode node) {
         if (node == this) {
             throw new System.ArgumentException("PathNode cannot connect to itself");
         }
         if (!Connections.Contains(node)) {
            throw new System.ArgumentException("PathNode is not contained in connections");
         }
         Connections.Remove(node);
    }
}