using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.PathFinding.Utils;
using MapGeneratorCs.Logging;

namespace MapGeneratorCs.PathFinding.AStar.Utils;
    
public static class AStarUtils
{
        public static PathResult? FindPath(PathNodes pathNodes, Vect2D start, Vect2D goal, float maxSearchCost = float.MaxValue)
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("AStarUtils.FindPath starting", false);
        if (!pathNodes.ContainsKey(start) || !pathNodes.ContainsKey(goal))
            return null;

        // per-run state
        var g = new Dictionary<Vect2D, float>(pathNodes.Count);
        var cameFrom = new Dictionary<Vect2D, Vect2D?>(pathNodes.Count);
        foreach (var kv in pathNodes) { g[kv.Key] = float.MaxValue; cameFrom[kv.Key] = null; }
        g[start] = 0f;

        float Heuristic(Vect2D a) => PathFindingUtils.CalculateHeuristic(a, goal);

        var open = new PriorityQueue<Vect2D, float>();
        open.Enqueue(start, Heuristic(start));

        var closed = new HashSet<Vect2D>();

        while (open.TryDequeue(out var currentPos, out var priority))
        {
            // check max search cost
            if (g[currentPos] > maxSearchCost)
            {
                timeLogger.Print($"AStarUtils.FindPath exceeded max search cost ({g[currentPos]})", true);
                return null;
            }

            var fNow = g[currentPos] + Heuristic(currentPos);
            if (!fNow.Equals(priority))
                continue;

            if (currentPos.Equals(goal))
            {
                var path = new List<Vect2D>();
                var c = goal;
                while (true)
                {
                    path.Add(c);
                    if (c.Equals(start)) break;
                    c = cameFrom[c]!.Value;
                }
                path.Reverse();
                var result = new PathResult(path, g[goal]);
                result.VisitedCount = closed.Count;
                timeLogger.Print($"AStarUtils.FindPath completed, len {path.Count}, cost {g[goal]}", true);
                return result;
            }

            closed.Add(currentPos);
            var current = pathNodes[currentPos];

            foreach (var neighbour in current.Neighbours)
            {
                var np = neighbour.Position;
                if (closed.Contains(np)) continue;

                bool diagonal = np.x != currentPos.x && np.y != currentPos.y;
                float stepCost = diagonal ? MathF.Sqrt(2) : 1f;

                // NEW: add corner penalty for diagonals (sum of the two corner tiles' penalties)
                float cornerPenalty = diagonal
                    ? PathFindingUtils.CalculateCornerPenalty(pathNodes, currentPos, np)
                    : 0f;

                float tentative = g[currentPos] + stepCost + neighbour.MovementPenalty + cornerPenalty;

                if (tentative < g[np])
                {
                    g[np] = tentative;
                    cameFrom[np] = currentPos;
                    open.Enqueue(np, tentative + Heuristic(np));
                }
            }
        }

        timeLogger.Print("AStarUtils.FindPath completed (not found)", true);
        return null;
    }
}