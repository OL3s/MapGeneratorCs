using System;

namespace MapGeneratorCs
{
    partial class MapConstructor
    {
        internal static class ObjectGenerator
        {
            public static void GenerateObjectDictionary(MapConstructor map)
            {
                Console.WriteLine("Converting Generate Nodes to Object Nodes in NodeContainer.NodesObjects...");

                foreach (var kvp in map.NodeContainer.NodesGenerate)
                {
                    var position = kvp.Key;
                    var genType = kvp.Value;

                    switch (genType)
                    {
                        case TileSpawnType.DefaultGenerator:
                            GenerateDefaultObject(map, position);
                            break;
                    }
                }
            }

            private static (int x, int y)? RandomFloorTile(MapConstructor map, (int x, int y) center, bool includeWalls = false)
            {
                int attempts = 0;
                int radius = map.CollisionRadius;
                int maxAttempts = radius * radius * 4;

                while (true)
                {
                    int dx = map.RNG.Next(-radius, radius + 1);
                    int dy = map.RNG.Next(-radius, radius + 1);
                    var position = (center.x + dx, center.y + dy);
                    attempts++;

                    if (dx * dx + dy * dy > radius * radius)
                        continue;

                    if (!includeWalls && !map.NodeContainer.NodesFloor.Contains(position))
                        continue;

                    if (attempts >= maxAttempts)
                        return null;

                    return position;
                }
            }

            private static int FloorTileCount((int x, int y) position, int radius, MapConstructor map, bool includeWalls = false)
            {
                int count = 0;
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (dx * dx + dy * dy <= radius * radius)
                        {
                            var p = (position.x + dx, position.y + dy);
                            if (map.NodeContainer.NodesFloor.Contains(p))
                                count++;
                        }
                    }
                }
                return count;
            }

            private static void GenerateDefaultObject(MapConstructor map, (int x, int y) position)
            {
                int propCount = FloorTileCount(position, map.CollisionRadius, map) / 3;

                while (propCount-- > 0)
                {
                    var testPos = RandomFloorTile(map, position);
                    if (testPos.HasValue)
                    {
                        map.NodeContainer.NodesObjects[testPos.Value] = TileSpawnType.PropObject;
                    }
                }
            }
        }
    }
}