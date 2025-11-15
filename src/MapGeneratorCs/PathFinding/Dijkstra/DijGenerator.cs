using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Image;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.Logging;

namespace MapGeneratorCs.PathFinding.Dijkstra;

public class DijGenerator
{
    private readonly PathNodes _graph;
    public Vect2D StartPosition { get; }
    public Dictionary<Vect2D, float> Dist { get; } = new();
    public Dictionary<Vect2D, Vect2D?> Parent { get; } = new();

    public DijGenerator(PathNodes pathNodes, Vect2D startPosition)
    {
        _graph = pathNodes;
        StartPosition = startPosition;
        ComputeFullMap();
    }

    private void ComputeFullMap()
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("Dijkstra precompute (dist+parent) starting", false);

        if (!_graph.ContainsKey(StartPosition))
            throw new ArgumentException("Start node not found in path nodes.");

        Dist.Clear();
        Parent.Clear();

        // init dist with +inf, parent null
        foreach (var kv in _graph)
        {
            Dist[kv.Key] = float.MaxValue;
            Parent[kv.Key] = null;
        }
        Dist[StartPosition] = 0f;

        var pq = new PriorityQueue<Vect2D, float>();
        pq.Enqueue(StartPosition, 0f);

        int processed = 0;
        int estimatedTotal = _graph.Count;

        while (pq.TryDequeue(out var cur, out var prio))
        {
            if (!Dist[cur].Equals(prio))
                continue;

            processed++;
            if (processed % 1_000_000 == 0)
                Console.WriteLine($"Dijkstra precompute: {processed / 1_000_000}/{estimatedTotal / 1_000_000} million nodes...");

            var node = _graph[cur];
            foreach (var nRef in node.Neighbours)
            {
                var np = nRef.Position;
                bool diagonal = np.x != cur.x && np.y != cur.y;
                float step = diagonal ? MathF.Sqrt(2) : 1f;
                float tentative = Dist[cur] + step + nRef.MovementPenalty;

                if (tentative < Dist[np])
                {
                    Dist[np] = tentative;
                    Parent[np] = cur;
                    pq.Enqueue(np, tentative);
                }
            }
        }

        timeLogger.Print("Dijkstra precompute (dist+parent) completed", true);
    }

    public float GetCostAt(Vect2D position) =>
        Dist.TryGetValue(position, out var d) ? d : float.MaxValue;

    public List<Vect2D>? FindPath(Vect2D goalPos)
    {
        if (!Dist.TryGetValue(goalPos, out var d) || d == float.MaxValue)
            return null;

        var path = new List<Vect2D>();
        var cur = goalPos;
        while (true)
        {
            path.Add(cur);
            if (cur.Equals(StartPosition)) break;
            var ok = Parent.TryGetValue(cur, out var p) && p.HasValue;
            if (!ok) return null;
            cur = p!.Value;
        }
        path.Reverse();
        return path;
    }

    public void SavePathToImage(List<Vect2D> path)
    {
        PathImagify.SavePathToImage(_graph, path, "dij_precomp_output.png");
    }
}