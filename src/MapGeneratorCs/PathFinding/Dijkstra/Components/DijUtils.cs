using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.Logging;

namespace MapGeneratorCs.PathFinding.Dijkstra.Utils;
public static class DijUtils
{
    // Creates a raw Dijkstra path not using precomputation
    public static List<Vect2D>? CreateDijPathFromPathNodes(PathNodes pathNodes, Vect2D start, Vect2D end)
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("DijUtils.CreateDijPathFromPathNodes starting", false);

        if (pathNodes == null || pathNodes.Count == 0)
            throw new ArgumentException("Path nodes dictionary is empty.");
        if (!pathNodes.ContainsKey(start) || !pathNodes.ContainsKey(end))
            throw new ArgumentException("Start or end position not found in path nodes.");
        if (start.Equals(end))
        {
            timeLogger.Print("DijUtils.CreateDijPathFromPathNodes completed", true);
            return new List<Vect2D> { start };
        }

        var dist = new Dictionary<Vect2D, float>(pathNodes.Count);
        var prev = new Dictionary<Vect2D, Vect2D?>(pathNodes.Count);
        foreach (var kv in pathNodes) { dist[kv.Key] = float.MaxValue; prev[kv.Key] = null; }
        dist[start] = 0f;

        var pq = new PriorityQueue<Vect2D, float>();
        pq.Enqueue(start, 0f);

        while (pq.TryDequeue(out var cur, out var prio))
        {
            if (!dist[cur].Equals(prio)) continue;
            if (cur.Equals(end)) break;

            var node = pathNodes[cur];
            foreach (var nRef in node.Neighbours)
            {
                var np = nRef.Position;
                bool diagonal = np.x != cur.x && np.y != cur.y;
                float step = diagonal ? MathF.Sqrt(2) : 1f;
                float tentative = dist[cur] + step + nRef.MovementPenalty;
                if (tentative < dist[np])
                {
                    dist[np] = tentative;
                    prev[np] = cur;
                    pq.Enqueue(np, tentative);
                }
            }
        }

        if (!prev[end].HasValue) { timeLogger.Print("DijUtils.CreateDijPathFromPathNodes completed", true); return null; }

        var path = new List<Vect2D>();
        var c = end;
        while (true)
        {
            path.Add(c);
            if (c.Equals(start)) break;
            c = prev[c]!.Value;
        }
        path.Reverse();
        timeLogger.Print("DijUtils.CreateDijPathFromPathNodes completed", true);
        return path;
    }

    // Legacy helper kept for compatibility with DijNodes callers
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
    public float GetCostAt(Vect2D position)
    {
        if (this.ContainsKey(position))
            return this[position].CostFromStart;
        else
            return float.MaxValue;
    }
    private bool isComputed = false;
    public DijNodes(PathNodes pathNodes, Vect2D startPosition) : base(pathNodes)
    {
        StartPosition = startPosition;
    }
    public void InitFullMap()
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("DjiNodes-Precompile full map starting", false);
        ResetNodeCosts();

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

        timeLogger.Print("DjiNodes-Precompile full map completed", true);
    }

    public List<Vect2D>? FindPathTo(Vect2D goalPos)
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("DijNodes-FindPrecompiledPath starting", false);
        var path = DijUtils.FindDijPathFromDijNodes(this, goalPos);
        timeLogger.Print("DijNodes-FindPrecompiledPath completed", true);
        return path;
    }
}