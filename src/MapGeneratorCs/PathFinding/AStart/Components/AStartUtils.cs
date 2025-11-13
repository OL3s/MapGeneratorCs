using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.PathFinding;

namespace MapGeneratorCs.PathFinding.AStar.Utils;
    
public static class AStarUtils
{

    public static List<Vect2D>? FindPath(Dictionary<Vect2D, PathNode> nodes, Vect2D start, Vect2D goal, bool resetNodeCosts)
    {
        var timeLogger = new PathFindingUtils.TimeLogger();
        if (!nodes.ContainsKey(start) || !nodes.ContainsKey(goal))
            return null;

        // Reset node costs if this is not the first calculation (optimization)
        if (resetNodeCosts)
            PathFindingUtils.ResetNodeCosts(nodes);

        timeLogger.Print("PathGenerator: Starting pathfinding...", false);
        var openSet = new SortedSet<PathNode>(new PathNodeComparer());
        var closedSet = new HashSet<PathNode>();

        var startNode = nodes[start];
        var goalNode = nodes[goal];

        startNode.CostFromStart = 0f;
        startNode.HeuristicCost = PathFindingUtils.CalculateHeuristic(start, goal);

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            var current = openSet.Min!;
            if (ReferenceEquals(current, goalNode))
            {
                timeLogger.Print("PathGenerator: Finished pathfinding");
                var path = PathFindingUtils.RetracePath(goalNode);
                PathFindingUtils.ResetNodeCosts(nodes);
                return path;
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
                    neighbour.HeuristicCost = PathFindingUtils.CalculateHeuristic(neighbour.Position, goal);
                    neighbour.ParentNode = current;

                    openSet.Add(neighbour);
                }
            }
        }

        timeLogger.Print("PathGenerator: No path found");
        PathFindingUtils.ResetNodeCosts(nodes);
        return null;
    }

}