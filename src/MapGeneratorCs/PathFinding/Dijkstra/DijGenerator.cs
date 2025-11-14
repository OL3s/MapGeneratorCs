using MapGeneratorCs.Types;
using MapGeneratorCs.Generator.Types;
using MapGeneratorCs.PathFinding.Dijkstra.Utils;
using MapGeneratorCs.PathFinding.Image;

namespace MapGeneratorCs.PathFinding.Dijkstra;

public class DijGenerator
{
    public DijNodes DijNodes { get; private set; }
    public Vect2D StartPosition => DijNodes.StartPosition;
    public DijGenerator(NodeContainerData container, Vect2D startPosition)
    {
        DijNodes = DijUtils.CreateFullDijPathMap(container, startPosition);
    }
    public List<Vect2D>? FindPath(Vect2D goalPos)
    {
        return DijUtils.FindDijPathFromDijNodes(DijNodes, goalPos);
    }
    public void SavePathAndMapToImage(MapConstructor map, List<Vect2D> path)
    {
        PathImagify.SavePathAndMapToImage(map, path, "dij_path_output.png");
    }
}
