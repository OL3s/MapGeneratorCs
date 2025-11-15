using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.PathFinding.Dijkstra;
using MapGeneratorCs.PathFinding.Utils;
using MapGeneratorCs.Generator.Types;
using MapGeneratorCs.Logging;
using MapGeneratorCs.PathFinding.Dijkstra.Utils;

namespace MapGeneratorCs.PathFinding.ALT;

public class ALTGenerator
{
    public Landmarks Landmarks { get; set; }
    // hold the shared graph for running ALT-A*
    private readonly PathNodes _pathNodes;

    public ALTGenerator(Vect2D startPos, int landmarkCount, PathNodes pathNodes)
    {
        _pathNodes = pathNodes;
        Landmarks = new Landmarks(landmarkCount, startPos, pathNodes);
    }

    // ALT heuristic h(u,t) = max_l d(l,t) - d(l,u)
    private float HeuristicALT(in Vect2D node, in Vect2D goal)
    {
        float h = 0f;
        foreach (var lm in Landmarks)
        {
            var dGoal = lm.GetCostAt(goal);
            var dNode = lm.GetCostAt(node);
            if (dGoal == float.MaxValue || dNode == float.MaxValue)
                continue;

            float v = MathF.Abs(dGoal - dNode);
            if (v > h) h = v;
        }

        // fallback if all landmarks are unreachable for this pair
        if (h <= 0f)
            h = PathFindingUtils.CalculateHeuristic(node, goal);
        return h;
    }

    // A* that uses ALT heuristic
    public List<Vect2D>? FindPath(Vect2D startPos, Vect2D goalPos)
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("ALT AStar: FindPath starting", false);

        if (!_pathNodes.ContainsKey(startPos) || !_pathNodes.ContainsKey(goalPos))
            return null;

        _pathNodes.ResetNodeCosts();

        var open = new PriorityQueue<PathNode, float>();
        var closed = new HashSet<PathNode>();

        var startNode = _pathNodes[startPos];
        var goalNode  = _pathNodes[goalPos];

        startNode.CostFromStart = 0f;
        startNode.HeuristicCost = HeuristicALT(startPos, goalPos);
        open.Enqueue(startNode, startNode.TotalCost);

        while (open.TryDequeue(out var current, out var prio))
        {
            // skip stale entries
            if (!current.TotalCost.Equals(prio))
                continue;

            if (ReferenceEquals(current, goalNode))
            {
                var path = PathFindingUtils.RetracePath(goalNode);
                timeLogger.Print("ALT AStar: FindPath completed", true);
                return path;
            }

            closed.Add(current);

            foreach (var neighRef in current.Neighbours)
            {
                var neigh = _pathNodes[neighRef.Position];
                if (closed.Contains(neigh))
                    continue;

                bool diagonal =
                    neighRef.Position.x != current.Position.x &&
                    neighRef.Position.y != current.Position.y;
                float step = diagonal ? MathF.Sqrt(2) : 1f;

                float tentative = current.CostFromStart + step + neighRef.MovementPenalty;
                if (tentative < neigh.CostFromStart)
                {
                    neigh.CostFromStart = tentative;
                    neigh.HeuristicCost = HeuristicALT(neighRef.Position, goalPos);
                    neigh.ParentNode = current;
                    open.Enqueue(neigh, neigh.TotalCost);
                }
            }
        }

        timeLogger.Print("ALT AStar: FindPath completed (not found)", true);
        return null;
    }
}

public class Landmarks : List<Landmark>
{
    public Landmarks(int landmarkCount, Vect2D startPos, PathNodes pathNodes) : base()
    {
        GenerateLandmarkPositions(landmarkCount, startPos, pathNodes);
    }
    public Landmarks(IEnumerable<Landmark> collection) : base(collection) { }
    public List<Vect2D> GetPositions() 
    {
        return this.Select(landmark => landmark.StartPosition).ToList();
    }
    private void GenerateLandmarkPositions(int landmarkCount, Vect2D startPos, PathNodes pathNodes)
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("ALT Landmark Generation Started", false);
        var dijGenerator = new DijGenerator(pathNodes, startPos);

        // Use precomputed distances instead of DijNodes
        var farthestPositions = dijGenerator.Dist
            .Where(kv => kv.Value < float.MaxValue)
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .ToList();
        if (farthestPositions.Count == 0) return;

        var dims = pathNodes.Dimentions;
        float area = (float)dims.x * dims.y;
        int radius = Math.Max(1, (int)(0.5f * MathF.Sqrt(area / Math.Max(1, landmarkCount))));

        var blocked = new HashSet<Vect2D>();
        var selected = new List<Vect2D>(landmarkCount) { startPos };
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (Math.Max(Math.Abs(dx), Math.Abs(dy)) > radius) continue;
                var p = new Vect2D(startPos.x + dx, startPos.y + dy);
                if (pathNodes.ContainsKey(p)) blocked.Add(p);
            }
        }

        foreach (var pos in farthestPositions)
        {
            if (selected.Count >= landmarkCount) break;
            if (blocked.Contains(pos)) continue;

            selected.Add(pos);

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) > radius) continue;
                    var p = new Vect2D(pos.x + dx, pos.y + dy);
                    if (pathNodes.ContainsKey(p))
                        blocked.Add(p);
                }
            }
        }

        if (selected.Count < landmarkCount)
        {
            foreach (var pos in farthestPositions)
            {
                if (selected.Count >= landmarkCount) break;
                if (!selected.Contains(pos))
                    selected.Add(pos);
            }
        }

        this.Clear();
        this.AddRange(selected.Select(pos => new Landmark(pos, pathNodes)));
        timeLogger.Print($"ALT Landmark Generation Ended with positions:\n  {this}\n  ", true);
    }

    public override string ToString()
    {
        return $"Landmarks: [{string.Join(", ", this)}]";
    }
}

public class Landmark : DijGenerator
{
    public Landmark(Vect2D startPos, PathNodes pathNodes) : base(pathNodes, startPos) { }
    public override string ToString() 
    {
        return $"Landmark at {StartPosition}";
    } 
}