using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Utils;
using MapGeneratorCs.Generator.Types;
using MapGeneratorCs.Logging;

namespace MapGeneratorCs.PathFinding.Types;

public class PathNode
{
    // == PRECOMPILED == see CreatePathNodesFromMap();
    public Vect2D Position;
    public TileSpawnType NodeType = TileSpawnType.Default;
    public float MovementPenalty = 0;
    public HashSet<PathNode> Neighbours = new();
    public float TotalCost => CostFromStart + HeuristicCost;

    // == RUNTIME == see AStar and Dij methods
    public float CostFromStart = float.MaxValue;  
    public float HeuristicCost = float.MaxValue;  
    public PathNode? ParentNode = null;
}

public class PathNodes : Dictionary<Vect2D, PathNode>
{
    public PathNodes() : base( ) { }
    public PathNodes(IDictionary<Vect2D, PathNode> pathNodes) : base(pathNodes) { }
    public Vect2D Dimentions
    {
        get
        {
            var maxX = this.Keys.Max(position => position.x);
            var maxY = this.Keys.Max(position => position.y);
            return new Vect2D(maxX + 1, maxY + 1);
        }
    }
    public void Generate(NodeContainerData container)
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("PathNodes.Generate starting", false);
        this.Clear();

        // Create new path nodes from the container
        foreach (var position in container.NodesFloor)
        {
            var type = container.NodesObjects.ContainsKey(position)
                ? container.NodesObjects[position]
                : TileSpawnType.Default;
            this.Add(position, new PathNode
            {
                Position = position,
                NodeType = type,
                MovementPenalty = PathFindingUtils.GetNodeMovementPenalty(type)
            });
        }

        // Assign neighbours
        foreach (var pair in this)
        {
            pair.Value.Neighbours = PathFindingUtils.GetNeighbours(pair.Key, this);
        }
        timeLogger.Print("PathNodes.Generate completed", true);
    }

    public void ResetNodeCosts()
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("PathNodes.ResetNodeCosts starting", false);
        foreach (var node in Values)
        {
            node.CostFromStart = float.MaxValue;
            node.HeuristicCost = float.MaxValue;
            node.ParentNode = null;
        }
        timeLogger.Print("PathNodes.ResetNodeCosts completed", true);
    }

    //
    public PathNodes Clone()
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("PathNodes.Clone starting", false);
        var newDict = new PathNodes();

        // nodes
        foreach (var kvp in this)
        {
            var src = kvp.Value;
            var clone = new PathNode
            {
                Position = src.Position,
                NodeType = src.NodeType,
                MovementPenalty = src.MovementPenalty,
                CostFromStart = float.MaxValue, // reset
                HeuristicCost = float.MaxValue, // reset
                ParentNode = null,
                Neighbours = new HashSet<PathNode>() // filled below
            };
            newDict[kvp.Key] = clone;
        }

        // neighbours
        foreach (var kvp in this)
        {
            var newNode = newDict[kvp.Key];
            var mapped = new HashSet<PathNode>();
            foreach (var neigh in kvp.Value.Neighbours)
            {
                if (newDict.TryGetValue(neigh.Position, out var mappedNeigh))
                    mapped.Add(mappedNeigh);
            }
            newNode.Neighbours = mapped;
        }
        timeLogger.Print("PathNodes.Clone completed", true);
        return newDict;
    }
}

public class PathResult : List<Vect2D>
{
    public float Cost { get; init; }

    public PathResult(List<Vect2D> path, float cost) : base(path)
    {
        Cost = cost;
    }
    public IReadOnlyList<Vect2D> Positions => this;
    public override string ToString()
    {
        return $"PathResult(len={this.Count}, cost={Cost})";
    }
}