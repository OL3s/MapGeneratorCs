using MapGeneratorCs.Types;
using MapGeneratorCs.Generator.Types;
using MapGeneratorCs.PathFinding.Dijkstra.Utils;
using MapGeneratorCs.PathFinding.Image;
using MapGeneratorCs.PathFinding.Types;

namespace MapGeneratorCs.PathFinding.Dijkstra;

public class DijGenerator
{
    public DijNodes DijNodes { get; private set; }
    public Vect2D StartPosition => DijNodes.StartPosition;
    public DijGenerator(PathNodes pathNodes, Vect2D startPosition)
    {
        DijNodes = new DijNodes(pathNodes.Clone(), startPosition);
    }
    public List<Vect2D>? FindPath(Vect2D goalPos)
    {
        return DijNodes.FindPathTo(goalPos);
    }
    public void SavePathToImage(List<Vect2D> path)
    {
        PathImagify.SavePathToImage(DijNodes, path, "dij_precomp_output.png");
    }
}
