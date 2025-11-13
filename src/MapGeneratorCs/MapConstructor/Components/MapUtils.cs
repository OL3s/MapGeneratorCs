using MapGeneratorCs.Types;
namespace MapGeneratorCs.Generator.Utils;

public static class MapUtils
{
    public static List<Vect2D> FindClosestObjectNodesOfTypeByAirDistance(
            Vect2D from,
            Dictionary<Vect2D, TileSpawnType> objectNodes,
            TileSpawnType? searchType = null,
            int? maxSearchDistance = null,
            int? maxObjectCount = null)
    {
        var result = new List<Vect2D>();

        if (objectNodes == null || objectNodes.Count == 0)
            return result;

        if (maxObjectCount.HasValue && maxObjectCount.Value == 0)
            return result;

        if (maxSearchDistance.HasValue && maxSearchDistance.Value <= 0)
            return result;

        int allowedDistance = maxSearchDistance ?? int.MaxValue;
        int maxObjectsToReturn = maxObjectCount ?? int.MaxValue;
        bool useDistanceLimit = maxSearchDistance.HasValue;
        bool filterByType = searchType.HasValue;
        var wantedType = searchType.GetValueOrDefault();

        // Manhattan distance
        static int CalculateManhattanDistance(in Vect2D a, in Vect2D b) =>
            Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        // Fast path for small sets
        if (objectNodes.Count < 1000)
        {
            var evaluatedNodes = new List<(Vect2D position, int distance)>(objectNodes.Count);

            foreach (var entry in objectNodes)
            {
                if (filterByType && entry.Value != wantedType)
                    continue;

                int distance = CalculateManhattanDistance(entry.Key, from);
                if (useDistanceLimit && distance > allowedDistance)
                    continue;

                evaluatedNodes.Add((entry.Key, distance));
            }

            evaluatedNodes.Sort((a, b) => a.distance.CompareTo(b.distance));

            int take = Math.Min(maxObjectsToReturn, evaluatedNodes.Count);
            for (int i = 0; i < take; i++)
                result.Add(evaluatedNodes[i].position);

            return result;
        }

        // Large set path
        var candidateNodes = new List<(Vect2D position, int distance)>();
        candidateNodes.Capacity = Math.Min(objectNodes.Count, 4096);

        foreach (var entry in objectNodes)
        {
            if (filterByType && entry.Value != wantedType)
                continue;

            int distance = CalculateManhattanDistance(entry.Key, from);
            if (useDistanceLimit && distance > allowedDistance)
                continue;

            candidateNodes.Add((entry.Key, distance));
        }

        if (candidateNodes.Count == 0)
            return result;

        // Single best result (no sort)
        if (maxObjectsToReturn == 1)
        {
            var bestNode = candidateNodes[0];

            for (int i = 1; i < candidateNodes.Count; i++)
            {
                if (candidateNodes[i].distance < bestNode.distance)
                    bestNode = candidateNodes[i];
            }

            result.Add(bestNode.position);
            return result;
        }

        // Sort and take top N
        candidateNodes.Sort((a, b) => a.distance.CompareTo(b.distance));

        int takeCount = Math.Min(maxObjectsToReturn, candidateNodes.Count);
        for (int i = 0; i < takeCount; i++)
            result.Add(candidateNodes[i].position);

        return result;
    }
}