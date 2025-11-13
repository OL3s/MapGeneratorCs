using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.AStar.Utils;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.PathFinding.AStar.Image;

namespace MapGeneratorCs.PathFinding.AStar;

public class AStarGenerator
{
    public Dictionary<Vect2D, PathNode> Nodes { get; private set; }
    private bool isCalculated = false;
    public AStarGenerator(NodeContainerData container, bool includePrintLog = false)
    {
        AStarUtils.IncludeTimerLog = includePrintLog;
        Nodes = AStarUtils.CreatePathNodesFromMap(container);
    }

    public List<Vect2D>? FindPath(Vect2D startPos, Vect2D goalPos)
    {
        var Return = AStarUtils.FindPath(Nodes, startPos, goalPos, isCalculated);
        isCalculated = true;
        return Return;
    }

    public void SavePathAndMapToImage(MapConstructor map, List<Vect2D> path)
    {
        AStarImagify.SavePathAndMapToImage(map, path);
    }
    
}
