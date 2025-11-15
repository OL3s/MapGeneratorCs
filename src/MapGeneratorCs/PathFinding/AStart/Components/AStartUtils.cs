using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.Logging;
using MapGeneratorCs.PathFinding.Utils;
using System.Transactions;

namespace MapGeneratorCs.PathFinding.AStar.Utils;
    
public static class AStarUtils
{
    public static List<Vect2D>? FindPath(PathNodes pathNodes, Vect2D start, Vect2D goal)
    {
        var timeLogger = new TimeLogger();
        if (!pathNodes.ContainsKey(start) || !pathNodes.ContainsKey(goal))
            return null;

        pathNodes.ResetNodeCosts();
        timeLogger.Print("AStarUtils: Starting pathfinding...", false);

        var closedSet = new HashSet<PathNode>();
        var openPq = new PriorityQueue<PathNode, float>();

        var startNode = pathNodes[start];
        var goalNode = pathNodes[goal];

        startNode.CostFromStart = 0f;
        startNode.HeuristicCost = PathFindingUtils.CalculateHeuristic(start, goal);

        openPq.Enqueue(startNode, startNode.TotalCost);

        while (openPq.TryDequeue(out var current, out var priority))
        {
            // Skip stale entries (priority may be outdated)
            if (!current.TotalCost.Equals(priority))
                continue;

            if (ReferenceEquals(current, goalNode))
            {
                timeLogger.Print("AStarUtils: Finished pathfinding");
                var path = PathFindingUtils.RetracePath(goalNode);
                PathFindingUtils.ResetNodeCosts(pathNodes);
                return path;
            }

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
                    neighbour.CostFromStart = newCost;
                    neighbour.HeuristicCost = PathFindingUtils.CalculateHeuristic(neighbour.Position, goal);
                    neighbour.ParentNode = current;

                    // enqueue with updated f = g + h; we allow duplicates and skip stale on dequeue
                    openPq.Enqueue(neighbour, neighbour.TotalCost);
                }
            }
        }

        timeLogger.Print("AStarUtils: No path found");
        PathFindingUtils.ResetNodeCosts(pathNodes);
        return null;
    }

}