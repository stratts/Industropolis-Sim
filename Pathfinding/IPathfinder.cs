using System.Collections.Generic;

public interface IPathfinder<T> {
    List<T> FindPath(T src, T dest);
}