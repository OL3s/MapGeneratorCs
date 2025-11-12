using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Utils;
using MapGeneratorCs.Image;

namespace MapGeneratorCs.PathFinding;

public class PathGenerator
{
    public Dictionary<Vect2D, PathNode> Nodes { get; private set; }
    private bool isCalculated = false;
    public PathGenerator(NodeContainerData container, bool includePrintLog = false)
    {
        PathFindingUtils.IncludeTimerLog = includePrintLog;
        Nodes = PathFindingUtils.CreatePathNodesFromMap(container);
    }

    public List<Vect2D>? FindPath(Vect2D startPos, Vect2D goalPos)
    {
        var Return = PathFindingUtils.FindPath(Nodes, startPos, goalPos, isCalculated);
        isCalculated = true;
        return Return;
    }

    public void SavePathAndMapToImage(MapConstructor map, List<Vect2D> path)
    {
        PathImagify.SavePathAndMapToImage(map, path);
    }
    
}
