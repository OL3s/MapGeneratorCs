using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.AStar.Utils;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.Image;
using MapGeneratorCs.Logging;

namespace MapGeneratorCs.PathFinding.AStar;

public class AStarGenerator
{
    public PathNodes PathNodes { get; private set; }
    public AStarGenerator(PathNodes pathNodes)
    {
        PathNodes = pathNodes;
    }
    public PathResult? FindPath(Vect2D startPos, Vect2D goalPos, float maxSearchCost = float.MaxValue)
    {
        var Return = AStarUtils.FindPath(PathNodes, startPos, goalPos, maxSearchCost);
        return Return;
    }

    public void SavePathAndMapToImage(List<Vect2D> path)
    {
        Imagify.SavePathToImage(PathNodes, path, "a_star_path_output.png");
    }
    
}
