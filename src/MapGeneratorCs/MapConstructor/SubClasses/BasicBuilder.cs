using MapGeneratorCs.Types;
namespace MapGeneratorCs.Utils;
public static class BasicBuilder
{
    private static Vect2D currentPosition;
    public static void GenerateDefaultAndFlaggedNotes(MapConstructor map)
    {
        var mapConfig = map.mapConfig;
        var spawnData = map.spawnWeights;

        if (map.NodeContainer.NodesFloorRaw.Count > 0)
        {
            Console.WriteLine("NodesFloor already generated. Skipping GenerateDefaultAndFlaggedNotes.");
            return;
        }

        currentPosition = new Vect2D(0, 0);
        int hitFloorCount = 0;

        while (map.NodeContainer.NodesFloorRaw.Count < mapConfig.Length)
        {
            bool isStart = map.NodeContainer.NodesFloorRaw.Count == 0;
            bool isEnd = map.NodeContainer.NodesFloorRaw.Count == mapConfig.Length - 1;
            bool isBoss = mapConfig.FlagBoss &&
                            map.NodeContainer.NodesFloorRaw.Count == (int)(mapConfig.Length * 0.66);
            bool isQuest = mapConfig.FlagQuest &&
                            map.NodeContainer.NodesFloorRaw.Count == (int)(mapConfig.Length * 0.33);

            // Add floor node if not exists
            if (!map.NodeContainer.NodesFloorRaw.Contains(currentPosition))
            {
                hitFloorCount = 0;

                // add flagged nodes
                if (isStart) map.NodeContainer.NodesGenerate[currentPosition] = TileSpawnType.StartGenerator;
                else if (isEnd) map.NodeContainer.NodesGenerate[currentPosition] = TileSpawnType.EndGenerator;
                else if (isBoss) map.NodeContainer.NodesGenerate[currentPosition] = TileSpawnType.BossGenerator;
                else if (isQuest) map.NodeContainer.NodesGenerate[currentPosition] = TileSpawnType.QuestGenerator;
                map.NodeContainer.NodesFloorRaw.Add(currentPosition); // Add new floor node
            }

            // Count hits on existing floor nodes
            else
            {
                hitFloorCount++;
            }

            var directions = new List<Vect2D> { new Vect2D(1, 0), new Vect2D(-1, 0), new Vect2D(0, 1), new Vect2D(0, -1) };
            var dir = directions[map.random.Next(directions.Count)];

            if (hitFloorCount < 2)
                currentPosition = new Vect2D(currentPosition.x + dir.x, currentPosition.y + dir.y);
            else
                while (map.NodeContainer.NodesFloorRaw.Contains(currentPosition))
                    currentPosition = new Vect2D(currentPosition.x + dir.x, currentPosition.y + dir.y);

            if (map.NodeContainer.NodesFloorRaw.Count % 1_000_000 == 0)
                Console.WriteLine($"Progress: {map.NodeContainer.NodesFloorRaw.Count / 1_000_000}/{map.mapConfig.Length / 1_000_000} 1-million packs generated...");
        }

        ApplyNodeRepositionBounds(map); // Give nodes positive coordinates
        map.NodeContainer.NodesFloor = CreateNodeFloorThickness(map.NodeContainer.NodesFloorRaw, map.mapConfig);
    }

    public static void FillDefaultNodesWithTypeNodes(MapConstructor map)
    {
        Console.WriteLine("Filling Default Nodes to NodeContainer.NodesGenerate...");

        var candidates = new List<Vect2D>();
        foreach (var p in map.NodeContainer.NodesFloorRaw)
            if (!map.NodeContainer.NodesGenerate.ContainsKey(p)) candidates.Add(p);

        // Fisher–Yates shuffle
        int n = candidates.Count;
        while (n > 1)
        {
            int k = map.random.Next(n--);
            (candidates[n], candidates[k]) = (candidates[k], candidates[n]);
        }

        foreach (var pos in candidates)
        {
            if (map.random.NextDouble() < 0.9) continue; // skip chance
            if (IsGenerateNodesRadiusOccupied(map, pos, map.mapConfig.CollisionRadius)) continue;

            var type = GetRandomSpawnTypeFromSpawnFactors(map);
            map.NodeContainer.NodesGenerate[pos] = type;
        }
    }

    private static bool IsGenerateNodesRadiusOccupied(MapConstructor map, Vect2D position, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                var p = new Vect2D(position.x + dx, position.y + dy);
                if (map.NodeContainer.NodesGenerate.TryGetValue(p, out var node) &&
                    node != TileSpawnType.Default)
                    return true;
            }
        }
        return false;
    }

    private static TileSpawnType GetRandomSpawnTypeFromSpawnFactors(MapConstructor map)
    {
        int total = map.spawnWeights.enemy + map.spawnWeights.landmark + map.spawnWeights.treasure
                    + map.spawnWeights.empty + map.spawnWeights._default + map.spawnWeights.trap;
        int roll = map.random.Next(1, total + 1), cum = 0;

        if ((cum += map.spawnWeights.enemy) >= roll) return TileSpawnType.EnemyGenerator;
        if ((cum += map.spawnWeights.landmark) >= roll) return TileSpawnType.LandmarkGenerator;
        if ((cum += map.spawnWeights.treasure) >= roll) return TileSpawnType.TreasureGenerator;
        if ((cum += map.spawnWeights.empty) >= roll) return TileSpawnType.EmptyGenerator;
        if ((cum += map.spawnWeights._default) >= roll) return TileSpawnType.DefaultGenerator;
        if ((cum += map.spawnWeights.trap) >= roll) return TileSpawnType.TrapGenerator;
        return TileSpawnType.Default;
    }

    private static void ApplyNodeRepositionBounds(MapConstructor map)
    {
        if (map.NodeContainer.NodesFloorRaw.Count == 0)
            throw new InvalidOperationException("No NodesFloor to calculate bounds.");

        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (var p in map.NodeContainer.NodesFloorRaw)
        {
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
        }

        int offsetX = map.padding - minX + map.mapConfig.Thickness;
        int offsetY = map.padding - minY + map.mapConfig.Thickness;

        var repositioned = new HashSet<Vect2D>();
        foreach (var p in map.NodeContainer.NodesFloorRaw)
            repositioned.Add(new Vect2D(p.x + offsetX, p.y + offsetY));
        map.NodeContainer.NodesFloorRaw = repositioned;

        var newDict = new Dictionary<Vect2D, TileSpawnType>();
        foreach (var kv in map.NodeContainer.NodesGenerate)
            newDict[new Vect2D(kv.Key.x + offsetX, kv.Key.y + offsetY)] = kv.Value;
        map.NodeContainer.NodesGenerate = newDict;
    }

    private static HashSet<Vect2D> CreateNodeFloorThickness(HashSet<Vect2D> mapToThicken, MapConfig mapConfig)
    {
        Console.WriteLine("Applying thickness to NodeContainer.NodesFloor...");

        // Ignore thickness zero or less
        if (mapConfig.Thickness <= 0)
        {
            Console.WriteLine("No thickness applied.");
            return mapToThicken;
        }

        // Create floor with thickness
        var nodesFloorThickness = new HashSet<Vect2D>();
        foreach (var p in mapToThicken)
        {
            for (int dx = -mapConfig.Thickness; dx <= mapConfig.Thickness; dx++)
                for (int dy = -mapConfig.Thickness; dy <= mapConfig.Thickness; dy++)
                    if (dx * dx + dy * dy <= mapConfig.Thickness * mapConfig.Thickness)
                        nodesFloorThickness.Add(new Vect2D(p.x + dx, p.y + dy));

            if (nodesFloorThickness.Count % 1_000_000 == 0)
                Console.WriteLine($"Progress: {nodesFloorThickness.Count / 1_000_000}/{mapConfig.Length / 1_000_000} 1-million packs generated...");
        }

        return nodesFloorThickness;
    }
}