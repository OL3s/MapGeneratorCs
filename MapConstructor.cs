using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;

namespace MapGeneratorCs
{

    // === Main Map Constructor Class ===
    class MapConstructor
    {

        // Private Fields

        private readonly bool ENABLE_DETAILED_LOGGING;
        private (int x, int y) currentPosition;
        public int Seed { get; private set; }
        private Random random;
        private int length;
        private int thickness;
        private int collisionRadius;
        private (
            int enemy,
            int landmark,
            int treasure,
            int _default,
            int empty
            ) spawnFactor;
        private (bool isBoss, bool isQuest) spawnTypeFlags;
        private int[,]? TileMap2D;
        private int padding = 1;
        private struct NodeContainerData
        {
            public HashSet<(int x, int y)> NodesFloor;
            public HashSet<(int x, int y)> NodesFloorRaw;
            public Dictionary<(int x, int y), TileSpawnType> NodesGenerate;
            public Dictionary<(int x, int y), TileSpawnType> NodesObjects;
        };
        private NodeContainerData NodeContainer;



        // Constructor with initialization

        public MapConstructor(
            int length, int thickness, int collisionRadius, int seed,
            (int enemyFactor, int landmarkFactor, int treasureFactor, int emptyFactor, int defaultFactor,
            bool isBoss, bool isQuest) spawnFactors, bool enableDetailedLogging = true)
        {
            // Initialize parameters
            this.length = length;
            this.collisionRadius = collisionRadius;
            this.spawnFactor = (
                spawnFactors.enemyFactor,
                spawnFactors.landmarkFactor,
                spawnFactors.treasureFactor,
                spawnFactors.emptyFactor,
                spawnFactors.defaultFactor
            );
            this.Seed = seed;
            this.random = new Random(seed);
            this.thickness = thickness;
            this.spawnTypeFlags = (spawnFactors.isBoss, spawnFactors.isQuest);
            this.ENABLE_DETAILED_LOGGING = enableDetailedLogging;

            // Initialize NodeContainer
            NodeContainer = new NodeContainerData
            {
                NodesFloor = new HashSet<(int x, int y)>(),
                NodesFloorRaw = new HashSet<(int x, int y)>(),
                NodesGenerate = new Dictionary<(int x, int y), TileSpawnType>(),
                NodesObjects = new Dictionary<(int x, int y), TileSpawnType>()

            };

            // Generate the map
            GenerateMap();

        }

        private void GenerateMap()
        {
            Console.WriteLine("Starting Map Generation...\n");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            GenerateUtils.TypeBasics.GenerateDefaultAndFlaggedNotes(this);
            GenerateUtils.TypeBasics.FillDefaultNodesWithTypeNodes(this);
            GenerateUtils.TypeGenerator.GenerateObjectDictionary(this);
            stopwatch.Stop();
            Console.WriteLine("\n=== Generated Map Statistics ===\n"
                +   $"Floor nodes           {NodeContainer.NodesFloor.Count,6} stk\n"
                +   $"Raw floor nodes       {NodeContainer.NodesFloorRaw.Count,6} stk\n"
                +   $"Parent nodes          {NodeContainer.NodesGenerate.Count,6} stk\n"
                +   $"Object nodes          {NodeContainer.NodesObjects.Count,6} stk\n"
                +   $"Generation time       {stopwatch.ElapsedMilliseconds,6} .ms\n");

            GenerateUtils.TypeIntMap2D.BuildFromNodes(this);
            GenerateUtils.TypeIntMap2D.SaveToImage(this, "map_output.png");
        }

        // === Generation Utilities for Map Generation ===
        private static class GenerateUtils
        {
            // === Node Object Generation ===
            public static class TypeGenerator
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

                // Private Helper Methods for Object Generation
                private static (int x, int y)? RandomFloorTile(MapConstructor map, (int x, int y) center, bool includeWalls = false)
                {
                    int attempts = 0;
                    int radius = map.collisionRadius;
                    var random = map.random;
                    int maxAttempts = radius * radius * 4;

                    int dx, dy;
                    (int x, int y) position;
                    while (true)
                    {
                        dx = random.Next(-radius, radius + 1);
                        dy = random.Next(-radius, radius + 1);
                        position = (center.x + dx, center.y + dy);
                        attempts++;

                        // Continue if outside radius
                        if (dx * dx + dy * dy > radius * radius)
                            continue;

                        // If includeWalls=false, position must be on floor
                        // If includeWalls=true, any position within radius is valid
                        if (!includeWalls && !map.NodeContainer.NodesFloor.Contains(position))
                            continue;

                        if (attempts >= maxAttempts)
                            return null; // Failed to find valid position

                        return position; // Found valid position
                    }
                }

                private static int FloorTileCount((int x, int y) position, int radius, HashSet<(int x, int y)> nodesFloor, bool includeWalls = false)
                {
                    int count = 0;
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            if (dx * dx + dy * dy <= radius * radius)
                            {
                                var p = (position.x + dx, position.y + dy);
                                if (nodesFloor.Contains(p))
                                {
                                    count++;
                                }
                            }
                        }
                    }
                    return count;
                }

                private static void GenerateDefaultObject(MapConstructor map, (int x, int y) position)
                {
                    var random = map.random;
                    var nodesFloor = map.NodeContainer.NodesFloor;
                    int propCount = FloorTileCount(position, map.collisionRadius, nodesFloor) / 2;

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


            // === Node Dictionary Basics ===

            public static class TypeBasics
            {
                public static void GenerateDefaultAndFlaggedNotes(MapConstructor map)
                {
                    if (map.NodeContainer.NodesFloorRaw.Count > 0)
                    {
                        Console.WriteLine("NodesFloor already generated. Skipping GenerateDefaultAndFlaggedNotes.");
                        return;
                    }

                    Console.WriteLine("Generating NodeContainer.NodesFloor...");
                    map.currentPosition = (0, 0);
                    int hitFloorCount = 0;

                    while (map.NodeContainer.NodesFloorRaw.Count < map.length)
                    {
                        bool isStart = map.NodeContainer.NodesFloorRaw.Count == 0;
                        bool isEnd = map.NodeContainer.NodesFloorRaw.Count == map.length - 1;
                        bool isBoss = map.spawnTypeFlags.isBoss &&
                                    map.NodeContainer.NodesFloorRaw.Count == (int)(map.length * 0.66);
                        bool isQuest = map.spawnTypeFlags.isQuest &&
                                    map.NodeContainer.NodesFloorRaw.Count == (int)(map.length * 0.33);

                        if (!map.NodeContainer.NodesFloorRaw.Contains(map.currentPosition))
                        {
                            hitFloorCount = 0;

                            if (isStart) map.NodeContainer.NodesGenerate[map.currentPosition] = TileSpawnType.Start;
                            else if (isEnd) map.NodeContainer.NodesGenerate[map.currentPosition] = TileSpawnType.End;
                            else if (isBoss) map.NodeContainer.NodesGenerate[map.currentPosition] = TileSpawnType.BossGenerator;
                            else if (isQuest) map.NodeContainer.NodesGenerate[map.currentPosition] = TileSpawnType.QuestGenerator;

                            map.NodeContainer.NodesFloorRaw.Add(map.currentPosition);

                            if (map.ENABLE_DETAILED_LOGGING)
                                Console.WriteLine($"Added node at {map.currentPosition.x}, {map.currentPosition.y} (total {map.NodeContainer.NodesFloorRaw.Count}/{map.length})");
                        }
                        else
                        {
                            hitFloorCount++;
                            if (map.ENABLE_DETAILED_LOGGING)
                                Console.WriteLine($"Position {map.currentPosition.x}, {map.currentPosition.y} already occupied. Hit count: {hitFloorCount}");
                        }

                        var directions = new List<(int x, int y)> { (1, 0), (-1, 0), (0, 1), (0, -1) };
                        var dir = directions[map.random.Next(directions.Count)];

                        if (hitFloorCount < 2)
                            map.currentPosition = (map.currentPosition.x + dir.x, map.currentPosition.y + dir.y);
                        else
                            while (map.NodeContainer.NodesFloorRaw.Contains(map.currentPosition))
                                map.currentPosition = (map.currentPosition.x + dir.x, map.currentPosition.y + dir.y);

                        if (map.NodeContainer.NodesFloorRaw.Count % 1_000_000 == 0)
                            Console.WriteLine($"Progress: {map.NodeContainer.NodesFloorRaw.Count / 1_000_000}/{map.length / 1_000_000} 1-million packs generated...");
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

                    // Fisher–Yates
                    int n = candidates.Count;
                    while (n > 1)
                    {
                        int k = map.random.Next(n--);
                        (candidates[n], candidates[k]) = (candidates[k], candidates[n]);
                    }

                    foreach (var pos in candidates)
                    {
                        if (map.random.NextDouble() < 0.98) continue; // skip-chance
                        if (IsGenerateNodesRadiusOccupied(map, pos, map.collisionRadius)) continue;

                        var type = GetRandomSpawnTypeFromSpawnFactors(map);
                        map.NodeContainer.NodesGenerate[pos] = type;

                        if (map.ENABLE_DETAILED_LOGGING)
                            Console.WriteLine($"Assigned {map.NodeContainer.NodesGenerate[pos]} to node at {pos}");
                    }
                }

                // ------- private helpers (now hidden inside utils) -------
                private static bool IsGenerateNodesRadiusOccupied(MapConstructor map, (int x, int y) position, int radius)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            var p = (position.x + dx, position.y + dy);
                            if (map.NodeContainer.NodesGenerate.TryGetValue(p, out var node) &&
                                node != MapConstructor.TileSpawnType.Default)
                                return true;
                        }
                    }
                    return false;
                }

                private static TileSpawnType GetRandomSpawnTypeFromSpawnFactors(MapConstructor map)
                {
                    int total = map.spawnFactor.enemy + map.spawnFactor.landmark + map.spawnFactor.treasure + map.spawnFactor.empty + map.spawnFactor._default;
                    int roll = map.random.Next(1, total + 1), cum = 0;

                    if ((cum += map.spawnFactor.enemy)    >= roll) return TileSpawnType.EnemyGenerator;
                    if ((cum += map.spawnFactor.landmark) >= roll) return TileSpawnType.LandmarkGenerator;
                    if ((cum += map.spawnFactor.treasure) >= roll) return TileSpawnType.TreasureGenerator;
                    if ((cum += map.spawnFactor.empty) >= roll) return TileSpawnType.EmptyGenerator;
                    if ((cum += map.spawnFactor._default) >= roll) return TileSpawnType.DefaultGenerator;
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

                    int offsetX = map.padding - minX + map.thickness;
                    int offsetY = map.padding - minY + map.thickness;

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

                    if (map.thickness <= 0)
                    {
                        map.NodeContainer.NodesFloor = map.NodeContainer.NodesFloorRaw;
                        return;
                    }

                    var result = new HashSet<(int x, int y)>();
                    foreach (var p in map.NodeContainer.NodesFloorRaw)
                    {
                        for (int dx = -map.thickness; dx <= map.thickness; dx++)
                            for (int dy = -map.thickness; dy <= map.thickness; dy++)
                                if (dx * dx + dy * dy <= map.thickness * map.thickness)
                                    result.Add((p.x + dx, p.y + dy));

                        if (result.Count % 1_000_000 == 0)
                            Console.WriteLine($"Progress: {result.Count / 1_000_000}/{map.length / 1_000_000} 1-million packs generated...");
                    }

                    map.NodeContainer.NodesFloor = result;
                }
            }



            // === IntMap2D Generation and Image Saving ===

            public static class TypeIntMap2D
            {
                // Builds TileMap2D from NodesGenerate, NodesFloor and NodesObjects
                public static void BuildFromNodes(MapConstructor map)
                {
                    if (map.NodeContainer.NodesFloor == null || map.NodeContainer.NodesFloor.Count == 0 || map.length <= 0)
                        throw new InvalidOperationException("No NodesFloor to convert to map.");

                    // Determine map dimensions
                    int maxX = 0, maxY = 0;
                    foreach (var p in map.NodeContainer.NodesFloor)
                    {
                        if (p.x > maxX) maxX = p.x;
                        if (p.y > maxY) maxY = p.y;
                    }

                    var width = maxX + map.thickness + map.padding + 1;
                    var height = maxY + map.thickness + map.padding + 1;
                    var grid = map.TileMap2D = new int[width, height];

                    Console.WriteLine($"Converting to IntMap2D ({width}x{height})");

                    // Default floor nodes
                    foreach (var p in map.NodeContainer.NodesFloor)
                    {
                        if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height)
                            grid[p.x, p.y] = (int)TileSpawnType.Default;
                    }

                    // Override with generate types
                    foreach (var kv in map.NodeContainer.NodesGenerate)
                    {
                        var p = kv.Key;
                        if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height)
                            grid[p.x, p.y] = (int)kv.Value;
                    }

                    // Override with object types
                    foreach (var kv in map.NodeContainer.NodesObjects)
                    {
                        var p = kv.Key;
                        if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height)
                            grid[p.x, p.y] = (int)kv.Value;
                    }
                }

                // Creates PNG image from TileMap2D
                public static void SaveToImage(MapConstructor map, string filePath)
                {
                    if (map.TileMap2D == null)
                        throw new InvalidOperationException("TileMap2D is null. Build map first.");

                    int w = map.TileMap2D.GetLength(0);
                    int h = map.TileMap2D.GetLength(1);
                    Console.WriteLine($"Saving map to {filePath} ({w}x{h})...");

                    using var image = new Image<Rgba32>(w, h);
                    for (int x = 0; x < w; x++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            var tileType = (TileSpawnType)map.TileMap2D[x, y];
                            if (tileType == TileSpawnType.Empty)
                                continue;
                            var color = GetColorForTileType(tileType);
                            image[x, y] = color;
                        }
                    }

                    image.Save(filePath, new PngEncoder());
                    Console.WriteLine("Map image saved successfully.");
                }
            }
            
            // === Color Mapping for Tile Types ===
            public static Rgba32 BlendColors(Rgba32 background, Rgba32 foreground, float alpha = 0.5f)
            {
                return new Rgba32(
                    (byte)(background.R * (1 - alpha) + foreground.R * alpha),
                    (byte)(background.G * (1 - alpha) + foreground.G * alpha),
                    (byte)(background.B * (1 - alpha) + foreground.B * alpha),
                    255 // Full opacity
                );
            }
            public static Rgba32 GetColorForTileType(TileSpawnType type)
            {
                return type switch
                {
                    TileSpawnType.Empty => new Rgba32(0, 0, 0, 255),
                    TileSpawnType.Default => new Rgba32(128, 128, 128, 255),
                    TileSpawnType.Start => new Rgba32(0, 255, 0, 255),
                    TileSpawnType.End => new Rgba32(255, 0, 0, 255),
                    TileSpawnType.TreasureGenerator => new Rgba32(255, 255, 0, 255),
                    TileSpawnType.EnemyGenerator => new Rgba32(128, 0, 128, 255),
                    TileSpawnType.LandmarkGenerator => new Rgba32(0, 0, 255, 255),
                    TileSpawnType.BossGenerator => new Rgba32(139, 0, 0, 255),
                    TileSpawnType.QuestGenerator => new Rgba32(255, 165, 0, 255),
                    TileSpawnType.DefaultGenerator => new Rgba32(192, 192, 192, 255),
                    TileSpawnType.EmptyGenerator => new Rgba32(64, 64, 64, 255),
                    TileSpawnType.TreasureObject => new Rgba32(255, 215, 0, 255),
                    TileSpawnType.EnemyCollector => new Rgba32(75, 0, 130, 255),
                    TileSpawnType.EnemyObject => new Rgba32(148, 0, 211, 255),
                    TileSpawnType.BossObject => new Rgba32(220, 20, 60, 255),
                    TileSpawnType.MainBossObject => new Rgba32(178, 34, 34, 255),
                    TileSpawnType.LandmarkObject => new Rgba32(70, 130, 180, 255),
                    TileSpawnType.QuestObject => new Rgba32(255, 140, 0, 255),
                    TileSpawnType.PropObject => new Rgba32(139, 69, 19, 255),
                    TileSpawnType.WaterTile => new Rgba32(0, 191, 255, 255),
                    TileSpawnType.LavaTile => new Rgba32(255, 69, 0, 255),
                    _ => new Rgba32(255, 0, 0, 255),
                };
            }
        }

        public enum TileSpawnType
        {
            // 0-3 basic type
            Empty = 0,
            Default = 1,
            Start = 2,
            End = 3,

            // 4-50 generators
            TreasureGenerator = 4,
            EnemyGenerator = 5,
            LandmarkGenerator = 6,
            BossGenerator = 7,
            QuestGenerator = 8,
            DefaultGenerator = 9,
            EmptyGenerator = 10,

            // 50-100 objects 
            TreasureObject = 50,
            EnemyCollector = 60,
            EnemyObject = 61,
            BossObject = 62,
            MainBossObject = 63,
            LandmarkObject = 70,
            QuestObject = 80,
            PropObject = 90,

            // 100-150 special tiles
            WaterTile = 100,
            LavaTile = 101,
        }
    }
}