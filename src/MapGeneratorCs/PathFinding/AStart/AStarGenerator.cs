using MapGeneratorCs.Types;
using MapGeneratorCs.Generator.Types;
using MapGeneratorCs.PathFinding.AStar.Utils;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.PathFinding.Image;
using MapGeneratorCs.PathFinding.Utils;

namespace MapGeneratorCs.PathFinding.AStar;

public class AStarGenerator
{
    public PathNodes PathNodes { get; private set; }
    private bool isCalculated = false;
    public AStarGenerator(NodeContainerData container)
    {
        PathNodes = PathFindingUtils.CreatePathNodesFromMap(container);
    }

    public List<Vect2D>? FindPath(Vect2D startPos, Vect2D goalPos)
    {
        var Return = AStarUtils.FindPath(PathNodes, startPos, goalPos, isCalculated);
        isCalculated = true;
        return Return;
    }

    public void SavePathAndMapToImage(MapConstructor map, List<Vect2D> path)
    {
        PathImagify.SavePathAndMapToImage(map, path, "a_star_path_output.png");
    }
    
}
