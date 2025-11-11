using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MapGeneratorCs.PathFinding;

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
