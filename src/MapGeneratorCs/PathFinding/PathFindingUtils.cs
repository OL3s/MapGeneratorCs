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

        PrintUpdateTimeLog("PathFindingUtils: Starting path node generation", false);
        foreach (var position in container.NodesFloor)
        {
            nodes.Add(position, new PathNode
            {
                Position = position,
                NodeType = TileSpawnType.Default
            });
        }

        PrintUpdateTimeLog("PathFindingUtils: Finished path node generation");
        foreach (var pair in nodes)
        {
            var position = pair.Key;
            var node = pair.Value;

            node.NodeType = GetNodeType(position, container);
            node.MovementPenalty = GetNodeMovementPenalty(node.NodeType);
            node.Neighbours = GetNeighbours(position, nodes);
        }

        PrintUpdateTimeLog("PathFindingUtils: Finished assigning node properties");
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
        PrintUpdateTimeLog("PathFindingUtils: Resetting node costs");
        foreach (var node in nodes.Values)
        {
            node.CostFromStart = float.MaxValue;
            node.HeuristicCost = float.MaxValue;
            node.ParentNode = null;
        }
        PrintUpdateTimeLog("PathFindingUtils: Finished resetting node costs");
    }

    public static List<Vect2D> FindClosestObjectNodesOfType(
        Vect2D from,
        Dictionary<Vect2D, TileSpawnType> objectNodes,
        int? maxSearchDistance = null,
        int? maxObjectCount = null)
    {
        var result = new List<Vect2D>();

        if (objectNodes == null || objectNodes.Count == 0)
            return result;

        if (maxObjectCount.HasValue && maxObjectCount.Value == 0)
            return result;

        if (maxSearchDistance.HasValue && maxSearchDistance.Value <= 0)
            return result;

        int allowedDistance = maxSearchDistance ?? int.MaxValue;
        int maxObjectsToReturn = maxObjectCount ?? int.MaxValue;
        bool useDistanceLimit = maxSearchDistance.HasValue;

        // Manhattan distance
        static int CalculateManhattanDistance(in Vect2D a, in Vect2D b) =>
            Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        // Fast path for small sets
        if (objectNodes.Count < 1000)
        {
            var evaluatedNodes = new List<(Vect2D position, int distance)>(objectNodes.Count);

            foreach (var entry in objectNodes)
            {
                int distance = CalculateManhattanDistance(entry.Key, from);
                if (useDistanceLimit && distance > allowedDistance)
                    continue;

                evaluatedNodes.Add((entry.Key, distance));
            }

            evaluatedNodes.Sort((a, b) => a.distance.CompareTo(b.distance));

            foreach (var node in evaluatedNodes.Take(maxObjectsToReturn))
                result.Add(node.position);

            return result;
        }

        // Large set path
        var candidateNodes = new List<(Vect2D position, int distance)>();
        candidateNodes.Capacity = Math.Min(objectNodes.Count, 4096);

        foreach (var entry in objectNodes)
        {
            int distance = CalculateManhattanDistance(entry.Key, from);
            if (useDistanceLimit && distance > allowedDistance)
                continue;

            candidateNodes.Add((entry.Key, distance));
        }

        if (candidateNodes.Count == 0)
            return result;

        // Single best result (no sort)
        if (maxObjectsToReturn == 1)
        {
            var bestNode = candidateNodes[0];

            for (int i = 1; i < candidateNodes.Count; i++)
            {
                if (candidateNodes[i].distance < bestNode.distance)
                    bestNode = candidateNodes[i];
            }

            result.Add(bestNode.position);
            return result;
        }

        // Sort and take top N
        candidateNodes.Sort((a, b) => a.distance.CompareTo(b.distance));

        foreach (var node in candidateNodes.Take(maxObjectsToReturn))
            result.Add(node.position);

        return result;
    }
}