using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Utils;
namespace MapGeneratorCs.PathFinding;

// Fast static pathfinding with instant results using Astar algorithm
public static class PathFindingStatic
{

}

// Dynamic pathfinding with cached nodes for easy updates and searches, but slower performance
public class PathFindingDynamic
{
    public Dictionary<Vect2D, PathNode> PathNodes = new Dictionary<Vect2D, PathNode>();
    private bool hasBeenUpdatedAtleastOnce = false;
    public PathFindingDynamic(NodeContainerData nodeContainer)
    {
        PathNodes = PathFindingUtils.CreatePathNodesFromMap(nodeContainer);
    }

    public void Update(Vect2D startPosition) {
        if (!hasBeenUpdatedAtleastOnce)
            hasBeenUpdatedAtleastOnce = true;

        PathFindingUtils.UpdateAllPathNodesFromPosition(PathNodes, startPosition);
    }
}