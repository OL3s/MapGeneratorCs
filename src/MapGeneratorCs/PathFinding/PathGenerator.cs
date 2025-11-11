using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Utils;

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

    public List<Vect2D>? FindPath(Vect2D start, Vect2D goal)
    {
        if (!Nodes.ContainsKey(start) || !Nodes.ContainsKey(goal))
            return null;

        // Reset node costs if this is not the first calculation (optimization)
        if (isCalculated)
            PathFindingUtils.ResetNodeCosts(Nodes);

        isCalculated = true;
        PathFindingUtils.PrintUpdateTimeLog("PathGenerator: Starting pathfinding...", false);
        var openSet = new SortedSet<PathNode>(new PathNodeComparer());
        var closedSet = new HashSet<PathNode>();

        var startNode = Nodes[start];
        var goalNode = Nodes[goal];

        startNode.CostFromStart = 0f;
        startNode.HeuristicCost = CalculateHeuristic(start, goal);

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            var current = openSet.Min!;
            if (ReferenceEquals(current, goalNode))
            {
                PathFindingUtils.PrintUpdateTimeLog("PathGenerator: Finished pathfinding");
                PathFindingUtils.ResetNodeCosts(Nodes);
                return RetracePath(goalNode);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbour in current.Neighbours)
            {
                if (closedSet.Contains(neighbour))
                    continue;

                bool diagonal =
                    neighbour.Position.x != current.Position.x &&
                    neighbour.Position.y != current.Position.y;

                float stepCost = diagonal ? MathF.Sqrt(2) : 1f;

                float newCost =
                    current.CostFromStart +
                    stepCost +
                    neighbour.MovementPenalty;

                if (newCost < neighbour.CostFromStart)
                {
                    if (openSet.Contains(neighbour))
                        openSet.Remove(neighbour);

                    neighbour.CostFromStart = newCost;
                    neighbour.HeuristicCost = CalculateHeuristic(neighbour.Position, goal);
                    neighbour.ParentNode = current;

                    openSet.Add(neighbour);
                }
            }
        }

        PathFindingUtils.PrintUpdateTimeLog("PathGenerator: No path found");
        PathFindingUtils.ResetNodeCosts(Nodes);
        return null;
    }

    private static List<Vect2D> RetracePath(PathNode goal)
    {
        var path = new List<Vect2D>();
        var current = goal;

        while (current != null)
        {
            path.Add(current.Position);
            current = current.ParentNode!;
        }

        path.Reverse();
        return path;
    }

    private float CalculateHeuristic(Vect2D a, Vect2D b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    public List<Vect2D> FindClosestObjectNodesOfType(
        Vect2D startPos, Dictionary<Vect2D,
        TileSpawnType> objectType,
        float? maxSearchDistance = null,
        int? maxObjectCount = null)
    {
        return PathFindingUtils.FindClosestObjectNodesOfType(
            startPos,
            objectType,
            maxSearchDistance: (int?)maxSearchDistance,
            maxObjectCount: maxObjectCount);

    }
}
