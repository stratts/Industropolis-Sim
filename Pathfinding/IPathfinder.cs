using System.Collections.Generic;

public interface IPathfinder<T>
{
    List<T>? FindPath(IGraph<T> graph, T src, T dest);
}

public interface IGraph<T>
{
    bool Accessible(T src, T dest);
    IEnumerable<T> GetConnections(T node);
    float CalculateCost(T src, T dest);
    float EstimateCost(T src, T dest);
}