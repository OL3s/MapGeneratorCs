using MapGeneratorCs.Types;

namespace MapGeneratorCs.PathFinding.Utils;

public static class PathFindingUtils
{
    public static void PrintUpdateTimeLog(string message, bool printTime = true)
    {
        if (IncludeTimerLog == false)
            return;

        var endTime = DateTime.Now;
        var duration = endTime - startTime;
        string logMessage = printTime
            ? $"{message} in {duration.TotalMilliseconds} ms"
            : message;
        Console.WriteLine(logMessage);
        startTime = DateTime.Now;
    }

    private static DateTime startTime;
    public static bool IncludeTimerLog { get; set; } = false;

    // Create path nodes from the map data
    public static Dictionary<Vect2D, PathNode> CreatePathNodesFromMap(
        NodeContainerData container)
    {
        var nodes = new Dictionary<Vect2D, PathNode>();

        PrintUpdateTimeLog("PathFindingUtils: Creating path node generation", false);
        foreach (var position in container.NodesFloor)
        {
            var type = container.NodesObjects.ContainsKey(position)
                ? container.NodesObjects[position]
                : TileSpawnType.Default;
            nodes.Add(position, new PathNode
            {
                Position = position,
                NodeType = type,
                MovementPenalty = GetNodeMovementPenalty(type)
            });
        }

        PrintUpdateTimeLog("PathFindingUtils: Finished path node generation");
        PrintUpdateTimeLog("PathFindingUtils: Assigning neighbours", false);
        foreach (var pair in nodes)
        {
            pair.Value.Neighbours = GetNeighbours(pair.Key, nodes);
        }

        PrintUpdateTimeLog("PathFindingUtils: Finished assigning node neighbours");
        return nodes;
    }

    // Determine the node type based on object spawns first, then default
    private static TileSpawnType GetNodeType(Vect2D pos, NodeContainerData container)
    {
        if (container.NodesObjects.ContainsKey(pos))
            return container.NodesObjects[pos];

        return TileSpawnType.Default;
    }

    // Determine movement penalty based on node type
    private static float GetNodeMovementPenalty(TileSpawnType type) =>
        type switch
        {
            // Movementcost = (1, sqrt(2) for diagonal) + GetNodeMovementPenalty()
            TileSpawnType.Default => 0f,
            TileSpawnType.TrapObject => 3f,
            TileSpawnType.TreasureObject => 100f,
            TileSpawnType.LandmarkObject => 100f,
            TileSpawnType.PropObject => 100f,
            _ => 10f
        };

    // Retrieve orthogonal and diagonal neighbours from the node map
    private static HashSet<PathNode> GetNeighbours(
        Vect2D pos,
        Dictionary<Vect2D, PathNode> nodes)
    {
        var neighbours = new HashSet<PathNode>();

        var orthogonal = new[]
        {
            new Vect2D( 1, 0),
            new Vect2D(-1, 0),
            new Vect2D( 0, 1),
            new Vect2D( 0,-1),
        };

        foreach (var offset in orthogonal)
        {
            var neighbourPos = new Vect2D(pos.x + offset.x, pos.y + offset.y);
            if (nodes.TryGetValue(neighbourPos, out var neighbour))
                neighbours.Add(neighbour);
        }

        // diagonal only if both corners exist
        var diagonal = new[]
        {
            new Vect2D( 1, 1),
            new Vect2D( 1,-1),
            new Vect2D(-1, 1),
            new Vect2D(-1,-1)
        };

        foreach (var offset in diagonal)
        {
            var a = new Vect2D(pos.x + offset.x, pos.y);
            var b = new Vect2D(pos.x, pos.y + offset.y);

            if (!nodes.ContainsKey(a) || !nodes.ContainsKey(b))
                continue;

            var diagPos = new Vect2D(pos.x + offset.x, pos.y + offset.y);
            if (nodes.TryGetValue(diagPos, out var neighbour))
                neighbours.Add(neighbour);
        }

        return neighbours;
    }

    // Reset costs for all nodes in the dictionary
    public static void ResetNodeCosts(Dictionary<Vect2D, PathNode> nodes)
    {
        PrintUpdateTimeLog("PathFindingUtils: Resetting node costs", false);
        foreach (var node in nodes.Values)
        {
            node.CostFromStart = float.MaxValue;
            node.HeuristicCost = float.MaxValue;
            node.ParentNode = null;
        }
        PrintUpdateTimeLog("PathFindingUtils: Finished resetting node costs");
    }

        public static List<Vect2D>? FindPath(Dictionary<Vect2D, PathNode> nodes, Vect2D start, Vect2D goal, bool resetNodeCosts)
    {
        if (!nodes.ContainsKey(start) || !nodes.ContainsKey(goal))
            return null;

        // Reset node costs if this is not the first calculation (optimization)
        if (resetNodeCosts)
            ResetNodeCosts(nodes);

        PrintUpdateTimeLog("PathGenerator: Starting pathfinding...", false);
        var openSet = new SortedSet<PathNode>(new PathNodeComparer());
        var closedSet = new HashSet<PathNode>();

        var startNode = nodes[start];
        var goalNode = nodes[goal];

        startNode.CostFromStart = 0f;
        startNode.HeuristicCost = CalculateHeuristic(start, goal);

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            var current = openSet.Min!;
            if (ReferenceEquals(current, goalNode))
            {
                PrintUpdateTimeLog("PathGenerator: Finished pathfinding");
                var path = RetracePath(goalNode);
                ResetNodeCosts(nodes);
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
                    neighbour.HeuristicCost = CalculateHeuristic(neighbour.Position, goal);
                    neighbour.ParentNode = current;

                    openSet.Add(neighbour);
                }
            }
        }

        PrintUpdateTimeLog("PathGenerator: No path found");
        ResetNodeCosts(nodes);
        return null;
    }
    private static float CalculateHeuristic(Vect2D a, Vect2D b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
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
}