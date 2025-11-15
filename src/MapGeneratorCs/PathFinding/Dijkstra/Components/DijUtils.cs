using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.Logging;
using MapGeneratorCs.PathFinding.Utils;

namespace MapGeneratorCs.PathFinding.Dijkstra.Utils;
public static class DijUtils
{
    // Creates a raw Dijkstra path from start to end using the computed Dij path map
   public static List<Vect2D>? CreateDijPathFromPathNodes(PathNodes pathNodes, Vect2D start, Vect2D end)
    {
        Console.WriteLine("DijUtils: Starting Dij single-target pathfinding...");
        var _pathNodes = pathNodes.Clone();

        if (_pathNodes.Count == 0 || _pathNodes == null)
            throw new ArgumentException("Path nodes dictionary is empty.");

        var dijDict = new DijNodes(_pathNodes, start);

        // Validate start/end presence
        if (!dijDict.ContainsKey(start) || !dijDict.ContainsKey(end))
            throw new ArgumentException("Start or end position not found in path nodes.");

        // If start == end, return trivial path
        if (start.Equals(end))
            return new List<Vect2D> { start };

        dijDict[start].CostFromStart = 0f;

        // Use a priority queue for single-target Dijkstra as well.
        var pq2 = new PriorityQueue<Vect2D, float>();
        pq2.Enqueue(start, 0f);

        while (pq2.TryDequeue(out var currentPos2, out var currentPriority2))
        {
            var currentNode = dijDict[currentPos2];

            // Skip stale entries
            if (!currentNode.CostFromStart.Equals(currentPriority2))
                continue;

            // If we've reached the target, build and return path immediately
            if (currentPos2.Equals(end))
                return FindDijPathFromDijNodes(dijDict, end);


            foreach (var neighborRef in currentNode.Neighbours)
            {
                var neighborNode = dijDict[neighborRef.Position];

                bool diagonal = neighborRef.Position.x != currentNode.Position.x &&
                                neighborRef.Position.y != currentNode.Position.y;
                float stepCost = diagonal ? MathF.Sqrt(2) : 1f;

                float tentativeCost = currentNode.CostFromStart + stepCost + neighborRef.MovementPenalty;
                if (tentativeCost < neighborNode.CostFromStart)
                {
                    neighborNode.CostFromStart = tentativeCost;
                    neighborNode.ParentNode = currentNode;
                    pq2.Enqueue(neighborRef.Position, tentativeCost);
                }
            }
        }
        Console.WriteLine("DijUtils: No path found from start to end.");
        return null;
    }


    // Used to find the fastest path from a precomputed Dijkstra path map
    public static List<Vect2D>? FindDijPathFromDijNodes(DijNodes dijNodes, Vect2D end)
    {

        if (!dijNodes.ContainsKey(end))
            return null;

        var path = new List<Vect2D>();
        var currentNode = dijNodes[end];

        while (currentNode != null)
        {
            path.Add(currentNode.Position);
            if (currentNode.Position.Equals(dijNodes.StartPosition))
                break;
            currentNode = currentNode.ParentNode;
        }

        path.Reverse();
        return path;
    }
}

public class DijNodes : PathNodes
{
    public Vect2D StartPosition;
    private bool isComputed = false;
    public DijNodes(PathNodes pathNodes, Vect2D startPosition) : base(pathNodes)
    {
        StartPosition = startPosition;
    }
    public void InitFullMap()
    {
        Console.WriteLine("DijNodes: Initializing full map Dijkstra computation...");
        PathFindingUtils.ResetNodeCosts(this);

        // Return if start node not in pathNodes dictionary
        if (!ContainsKey(StartPosition))
            throw new ArgumentException("Start node not found in pathNodes dictionary.");

        if (isComputed)
            throw new InvalidOperationException("Dijkstra full map computation has already been performed.");
        
        isComputed = true;
        this[StartPosition].CostFromStart = 0f; // Set start node cost to 0

        // Priority queue to avoid O(n^2)
        var priorityQueue = new PriorityQueue<Vect2D, float>();
        priorityQueue.Enqueue(StartPosition, 0f);
        int processed = 0;
        int estimatedTotal = Count;
        while (priorityQueue.TryDequeue(out var currentPos, out var currentPriority))
        {
            // Get the node from the dictionary
            var currentNode = this[currentPos];
            // Skip stale entries: if the priority doesn't match the current known cost
            if (!currentNode.CostFromStart.Equals(currentPriority))
                continue;

            processed++;
            if (processed % 1_000_000 == 0)
                Console.WriteLine($"DjiNodes: Processed {processed / 1_000_000}/{estimatedTotal / 1_000_000} million nodes...");

            // For each neighbor, update tentative cost
            foreach (var neighborRef in currentNode.Neighbours)
            {
                var neighborNode = this[neighborRef.Position];

                // Diagonal check
                bool diagonal = neighborRef.Position.x != currentNode.Position.x &&
                                neighborRef.Position.y != currentNode.Position.y;
                float stepCost = diagonal ? MathF.Sqrt(2) : 1f;

                float tentativeCost = currentNode.CostFromStart + stepCost + neighborRef.MovementPenalty;
                if (tentativeCost < neighborNode.CostFromStart)
                {
                    neighborNode.CostFromStart = tentativeCost;
                    neighborNode.ParentNode = currentNode;
                    priorityQueue.Enqueue(neighborRef.Position, tentativeCost);
                }
            }
        }
    }

    public List<Vect2D>? FindPathTo(Vect2D goalPos)
    {
        return DijUtils.FindDijPathFromDijNodes(this, goalPos);
    }
}