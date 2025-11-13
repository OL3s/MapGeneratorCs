using System.Runtime.CompilerServices;
using MapGeneratorCs.Types;
namespace MapGeneratorCs.PathFinding.Types;

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

public sealed class PathNodeComparer : IComparer<PathNode>
{
    public int Compare(PathNode? a, PathNode? b)
    {
        if (ReferenceEquals(a, b)) return 0;
        if (a is null) return -1;
        if (b is null) return 1;

        int compare = a.TotalCost.CompareTo(b.TotalCost);
        if (compare != 0) return compare;

        compare = a.HeuristicCost.CompareTo(b.HeuristicCost);
        if (compare != 0) return compare;

        compare = a.Position.x.CompareTo(b.Position.x);
        if (compare != 0) return compare;

        compare = a.Position.y.CompareTo(b.Position.y);
        if (compare != 0) return compare;

        return RuntimeHelpers.GetHashCode(a).CompareTo(RuntimeHelpers.GetHashCode(b));
    }
}