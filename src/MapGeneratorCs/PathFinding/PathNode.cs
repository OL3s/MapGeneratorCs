using MapGeneratorCs.Types;
using System.Collections.Generic;

namespace MapGeneratorCs.PathFinding;

public class PathNode
{
    public Vect2D Position;
    public TileSpawnType NodeType = TileSpawnType.Default;
    public float MovementPenalty = 0;
    public HashSet<PathNode> Neighbours = new();
    public float CostFromStart = float.MaxValue;  
    public float HeuristicCost = float.MaxValue;  
    public float TotalCost => CostFromStart + HeuristicCost;
    public PathNode? ParentNode = null;
}
