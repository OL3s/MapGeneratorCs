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
    public ALTGenerator(Vect2D startPos, int landmarkCount, PathNodes pathNodes)
    {
        Landmarks = new Landmarks(landmarkCount, startPos, pathNodes);
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

        // Get nodes sorted, longest distance first 
        var dijNodesList = dijGenerator.DijNodes.Values.ToList();
        dijNodesList.Sort((a, b) => b.CostFromStart.CompareTo(a.CostFromStart));
        if (dijNodesList.Count == 0) return;

        // spacing radius ~ 0.5 * sqrt(area / k)
        var dims = pathNodes.Dimentions;
        float area = (float)dims.x * dims.y;
        int radius = Math.Max(1, (int)(0.5f * MathF.Sqrt(area / Math.Max(1, landmarkCount))));
        int radiusSq = radius * radius;

        // fast membership and blocking set to avoid O(k) checks per candidate
        var blocked = new HashSet<Vect2D>();
        // selection with startPos so it's included and blocked properly
        var selected = new List<Vect2D>(landmarkCount) { startPos };
        // mark blocked region for the seed landmark
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (Math.Max(Math.Abs(dx), Math.Abs(dy)) > radius) continue;
                var p = new Vect2D(startPos.x + dx, startPos.y + dy);
                if (pathNodes.ContainsKey(p)) blocked.Add(p);
            }
        }

        foreach (var node in dijNodesList)
        {
            if (selected.Count >= landmarkCount) break;
            var pos = node.Position;
            if (blocked.Contains(pos)) continue;

            // select this as new landmark
            selected.Add(pos);

            // mark chebyshev-blocked region around pos (grid friendly)
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // use chebyshev limit to match grid "radius"
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) > radius) continue;
                    var p = new Vect2D(pos.x + dx, pos.y + dy);
                    // only mark positions that are within map bounds and exist in pathNodes (optional)
                    if (pathNodes.ContainsKey(p))
                        blocked.Add(p);
                }
            }
        }

        // if we still need more landmarks, fill from remaining farthest nodes ignoring blocking
        if (selected.Count < landmarkCount)
        {
            foreach (var node in dijNodesList)
            {
                if (selected.Count >= landmarkCount) break;
                var pos = node.Position;
                if (!selected.Contains(pos))
                    selected.Add(pos);
            }
        }

        // replace this list with selected results
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