using MapGeneratorCs.Types;
using MapGeneratorCs.Utils;
namespace MapGeneratorCs.Builders;

public static class ObjectBuilder
{
    // Convert generate nodes to object nodes based on spawn factors
    public static void GenerateObjectNodes(MapConstructor map)
    {
        Console.WriteLine("Converting Generate Nodes to Object Nodes in NodeContainer.NodesObjects...");
        Dictionary<int, GenerateObjectWeights> config = ConfigLoader.LoadObjectWeights();
        (int i, int p) count = (0, 0);

        foreach (var kvp in map.NodeContainer.NodesGenerate)
        {
            var position = kvp.Key;
            var generateType = kvp.Value;

            // call only when config entry exists (use TryGetValue to avoid double lookup)
            if (config.TryGetValue((int)generateType, out var spawnWeights))
                GenerateObjectsRandomWeighted(map, position, spawnWeights);

            switch (generateType)
            {
                case TileSpawnType.EnemyGenerator:
                    GenerateObjectsRandomCount(map, position, TileSpawnType.EnemyObject, map.random.Next(1, 4));
                    break;

                // JSON list index 2
                case TileSpawnType.LandmarkGenerator:
                    GenerateObjectsRandomCount(map, position, TileSpawnType.LandmarkObject, 1);
                    break;

                // JSON list index 3
                case TileSpawnType.TreasureGenerator:
                    GenerateObjectsRandomCount(map, position, TileSpawnType.TreasureObject, 1);
                    break;

                // JSON list index 4
                case TileSpawnType.TrapGenerator:
                    GenerateObjectsRandomCount(map, position, TileSpawnType.TrapObject, map.random.Next(1, 3));
                    break;

                // JSON list index 6
                case TileSpawnType.BossGenerator:
                    GenerateObjectsRandomCount(map, position, TileSpawnType.BossObject, 1);
                    break;

                // JSON list index 7
                case TileSpawnType.QuestGenerator:
                    GenerateObjectsRandomCount(map, position, TileSpawnType.QuestObject, 1);
                    break;

                // JSON list index 8
                case TileSpawnType.StartGenerator:
                    GenerateObjectsRandomCount(map, position, TileSpawnType.StartObject, 1);
                    break;

                case TileSpawnType.EndGenerator:
                    GenerateObjectsRandomCount(map, position, TileSpawnType.EndObject, 1);
                    break;
            }

            count.i++;
            if (count.i % 100_000 == 0)
            {
                count.p++;
                Console.WriteLine($"Progress: {count.p} hundred-thousand object nodes processed...");
            }
        }
    }

    private static void GenerateObjectsRandomWeighted(MapConstructor map, Vect2D position, GenerateObjectWeights spawnWeights)
    {
        var possiblePositions = GetOpenTiles(map, position, map.mapConfig.CollisionRadius);
        int propCount = possiblePositions.Count / 3;

        while (propCount-- > 0 && possiblePositions.Count > 0)
        {
            var spawnPos = possiblePositions[map.random.Next(possiblePositions.Count)];
            var objType = spawnWeights.GetRandomObject(map.random);
            if (objType != TileSpawnType.Empty && !map.NodeContainer.NodesObjects.ContainsKey(spawnPos))
                map.NodeContainer.NodesObjects[spawnPos] = objType;
            possiblePositions.Remove(spawnPos);
        }
    }

    private static void GenerateObjectsRandomCount(MapConstructor map, Vect2D position, TileSpawnType objType, int count)
    {

        var flagNodes = new List<TileSpawnType>
        {
            TileSpawnType.StartObject,
            TileSpawnType.EndObject,
            TileSpawnType.BossObject,
            TileSpawnType.MainBossObject,
            TileSpawnType.QuestObject
        };

        var possiblePositions = GetOpenTiles(map, position, map.mapConfig.CollisionRadius);

        while (count-- > 0 && possiblePositions.Count > 0)
        {
            var spawnPos = possiblePositions[map.random.Next(possiblePositions.Count)];
            if (objType != TileSpawnType.Empty && !map.NodeContainer.NodesObjects.ContainsKey(spawnPos))
            {
                map.NodeContainer.NodesObjects[spawnPos] = objType;
                // Add to flagged nodes if applicable
                if (flagNodes.Contains(objType))
                    map.NodeContainer.NodesFlags[spawnPos] = objType;
            }
            possiblePositions.Remove(spawnPos);
        }
    }

    private static List<Vect2D> GetOpenTiles(MapConstructor map, Vect2D center, int radius)
    {
        var outTiles = new List<Vect2D>();
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy <= radius * radius)
                {
                    var position = new Vect2D(center.x + dx, center.y + dy);
                    if (map.NodeContainer.NodesFloor.Contains(position) &&
                        !map.NodeContainer.NodesObjects.ContainsKey(position))
                    {
                        outTiles.Add(position);
                    }
                }
            }
        }
        return outTiles;
    }
}