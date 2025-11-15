using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Utils;
using MapGeneratorCs.Generator.Types;

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
    public void Generate(NodeContainerData container)
    {

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
    }

    public void ResetNodeCosts()
    {
        foreach (var node in Values)
        {
            node.CostFromStart = float.MaxValue;
            node.HeuristicCost = float.MaxValue;
            node.ParentNode = null;
        }
    }
    public PathNodes Clone()
    {
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
                CostFromStart = src.CostFromStart,
                HeuristicCost = src.HeuristicCost,
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

        return newDict;
    }
}