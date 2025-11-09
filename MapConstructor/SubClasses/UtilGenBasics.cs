using System;
using System.Collections.Generic;

namespace MapGeneratorCs
{
    partial class MapConstructor
    {
        internal static class GenerationBasics
        {
            public static void GenerateDefaultAndFlaggedNotes(MapConstructor map)
            {
                if (map.NodeContainer.NodesFloorRaw.Count > 0)
                {
                    Console.WriteLine("NodesFloor already generated. Skipping GenerateDefaultAndFlaggedNotes.");
                    return;
                }

                map.CurrentPos = (0, 0);
                int hitFloorCount = 0;

                while (map.NodeContainer.NodesFloorRaw.Count < map.Length)
                {
                    bool isStart = map.NodeContainer.NodesFloorRaw.Count == 0;
                    bool isEnd = map.NodeContainer.NodesFloorRaw.Count == map.Length - 1;
                    bool isBoss = map.SpawnTypeFlags.isBoss &&
                                  map.NodeContainer.NodesFloorRaw.Count == (int)(map.Length * 0.66);
                    bool isQuest = map.SpawnTypeFlags.isQuest &&
                                   map.NodeContainer.NodesFloorRaw.Count == (int)(map.Length * 0.33);

                    if (!map.NodeContainer.NodesFloorRaw.Contains(map.CurrentPos))
                    {
                        hitFloorCount = 0;

                        if (isStart) map.NodeContainer.NodesGenerate[map.CurrentPos] = TileSpawnType.StartGenerator;
                        else if (isEnd) map.NodeContainer.NodesGenerate[map.CurrentPos] = TileSpawnType.EndGenerator;
                        else if (isBoss) map.NodeContainer.NodesGenerate[map.CurrentPos] = TileSpawnType.BossGenerator;
                        else if (isQuest) map.NodeContainer.NodesGenerate[map.CurrentPos] = TileSpawnType.QuestGenerator;

                        map.NodeContainer.NodesFloorRaw.Add(map.CurrentPos);

                        if (map.Verbose)
                            Console.WriteLine($"Added node at {map.CurrentPos.x}, {map.CurrentPos.y} (total {map.NodeContainer.NodesFloorRaw.Count}/{map.Length})");
                    }
                    else
                    {
                        hitFloorCount++;
                        if (map.Verbose)
                            Console.WriteLine($"Position {map.CurrentPos.x}, {map.CurrentPos.y} already occupied. Hit count: {hitFloorCount}");
                    }

                    var directions = new List<(int x, int y)> { (1, 0), (-1, 0), (0, 1), (0, -1) };
                    var dir = directions[map.RNG.Next(directions.Count)];

                    if (hitFloorCount < 2)
                        map.CurrentPos = (map.CurrentPos.x + dir.x, map.CurrentPos.y + dir.y);
                    else
                        while (map.NodeContainer.NodesFloorRaw.Contains(map.CurrentPos))
                            map.CurrentPos = (map.CurrentPos.x + dir.x, map.CurrentPos.y + dir.y);

                    if (map.NodeContainer.NodesFloorRaw.Count % 1_000_000 == 0)
                        Console.WriteLine($"Progress: {map.NodeContainer.NodesFloorRaw.Count / 1_000_000}/{map.Length / 1_000_000} 1-million packs generated...");
                }

                ApplyNodeRepositionBounds(map);
                ApplyNodeFloorThickness(map);
            }

            public static void FillDefaultNodesWithTypeNodes(MapConstructor map)
            {
                Console.WriteLine("Filling Default Nodes to NodeContainer.NodesGenerate...");

                var candidates = new List<(int x, int y)>();
                foreach (var p in map.NodeContainer.NodesFloorRaw)
                    if (!map.NodeContainer.NodesGenerate.ContainsKey(p)) candidates.Add(p);

                // Fisher–Yates shuffle
                int n = candidates.Count;
                while (n > 1)
                {
                    int k = map.RNG.Next(n--);
                    (candidates[n], candidates[k]) = (candidates[k], candidates[n]);
                }

                foreach (var pos in candidates)
                {
                    if (map.RNG.NextDouble() < 0.9) continue; // skip chance
                    if (IsGenerateNodesRadiusOccupied(map, pos, map.CollisionRadius)) continue;

                    var type = GetRandomSpawnTypeFromSpawnFactors(map);
                    map.NodeContainer.NodesGenerate[pos] = type;

                    if (map.Verbose)
                        Console.WriteLine($"Assigned {map.NodeContainer.NodesGenerate[pos]} to node at {pos}");
                }
            }

            private static bool IsGenerateNodesRadiusOccupied(MapConstructor map, (int x, int y) position, int radius)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        var p = (position.x + dx, position.y + dy);
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
                int roll = map.RNG.Next(1, total + 1), cum = 0;

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

                int offsetX = map.Padding - minX + map.Thickness;
                int offsetY = map.Padding - minY + map.Thickness;

                var repositioned = new HashSet<(int x, int y)>();
                foreach (var p in map.NodeContainer.NodesFloorRaw)
                    repositioned.Add((p.x + offsetX, p.y + offsetY));
                map.NodeContainer.NodesFloorRaw = repositioned;

                var newDict = new Dictionary<(int x, int y), TileSpawnType>();
                foreach (var kv in map.NodeContainer.NodesGenerate)
                    newDict[(kv.Key.x + offsetX, kv.Key.y + offsetY)] = kv.Value;
                map.NodeContainer.NodesGenerate = newDict;
            }

            private static void ApplyNodeFloorThickness(MapConstructor map)
            {
                Console.WriteLine("Applying thickness to NodeContainer.NodesFloor...");

                if (map.Thickness <= 0)
                {
                    map.NodeContainer.NodesFloor = map.NodeContainer.NodesFloorRaw;
                    return;
                }

                var result = new HashSet<(int x, int y)>();
                foreach (var p in map.NodeContainer.NodesFloorRaw)
                {
                    for (int dx = -map.Thickness; dx <= map.Thickness; dx++)
                        for (int dy = -map.Thickness; dy <= map.Thickness; dy++)
                            if (dx * dx + dy * dy <= map.Thickness * map.Thickness)
                                result.Add((p.x + dx, p.y + dy));

                    if (result.Count % 1_000_000 == 0)
                        Console.WriteLine($"Progress: {result.Count / 1_000_000}/{map.Length / 1_000_000} 1-million packs generated...");
                }

                map.NodeContainer.NodesFloor = result;
            }
        }
    }
}