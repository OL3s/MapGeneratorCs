using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Types;

namespace MapGeneratorCs.PathFinding;

public static class PathFindingUtils
{

    public static float GetNodeMovementPenalty(TileSpawnType type) =>
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

    public static HashSet<PathNode> GetNeighbours(
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

    public static void ResetNodeCosts(Dictionary<Vect2D, PathNode> pathNodes)
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("PathFindingUtils: Resetting node costs", false);
        foreach (var node in pathNodes.Values)
        {
            node.CostFromStart = float.MaxValue;
            node.HeuristicCost = float.MaxValue;
            node.ParentNode = null;
        }
        timeLogger.Print("PathFindingUtils: Finished resetting node costs");
    }

    public static float CalculateHeuristic(Vect2D a, Vect2D b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    public static List<Vect2D> RetracePath(PathNode goal)
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

    public static Dictionary<Vect2D, PathNode> CreatePathNodesFromMap(
        NodeContainerData container)
    {
        var nodes = new Dictionary<Vect2D, PathNode>();
        var timeLogger = new TimeLogger();

        timeLogger.Print("PathFindingUtils: Creating path node generation", false);
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

        timeLogger.Print("PathFindingUtils: Finished path node generation");
        timeLogger.Print("PathFindingUtils: Assigning neighbours", false);
        foreach (var pair in nodes)
        {
            pair.Value.Neighbours = GetNeighbours(pair.Key, nodes);
        }

        timeLogger.Print("PathFindingUtils: Finished assigning node neighbours");
        return nodes;
    }

    public class TimeLogger
    {
        public DateTime StartTime { get; set; } = DateTime.Now;
        private bool includePrintLog = false;
        public TimeLogger(bool includePrintLog = true)
        {
            this.includePrintLog = includePrintLog;
            StartTime = DateTime.Now;
        }
        public void Print(string message, bool printTime = true)
        {
            if (includePrintLog == false)
                return;

            var endTime = DateTime.Now;
            var duration = endTime - StartTime;
            string logMessage = printTime
                ? $"{message} in {duration.TotalMilliseconds} ms"
                : message;
            Console.WriteLine(logMessage);
            StartTime = DateTime.Now;
        }
    }
}
