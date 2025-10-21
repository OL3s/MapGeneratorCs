using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;

namespace MapGeneratorCs
{
    class MapConstructor
    {
        // Internal state
        private ((int x, int y) topLeft, (int x, int y) bottomRight) bounds = ((0, 0), (0, 0));
        private (int width, int height) mapSize => (bounds.bottomRight.x - bounds.topLeft.x + 1,
                                                  bounds.bottomRight.y - bounds.topLeft.y + 1);
        private (int x, int y) currentPosition;
        private int seed;
        private int length;
        private int thickness;
        private int padding;
        private Dictionary<(int x, int y), Node> Nodes
            = new Dictionary<(int x, int y), Node>();

        public MapConstructor(
            int length, int thickness,
            int enemyFactor, int landmarkFactor, int treasureFactor,
            bool isBoss, bool isQuest)
        {
            // Initialize parameters
            this.length = length;
            this.seed = Random.Shared.Next();
            this.thickness = thickness;
            this.padding = thickness + 1;

            // Add spawn types based on factors
            var spawns = new List<TileSpawnType>();
            for (int i = 0; i < enemyFactor; i++) spawns.Add(TileSpawnType.EnemySpawn);
            for (int i = 0; i < landmarkFactor; i++) spawns.Add(TileSpawnType.Landmark);
            for (int i = 0; i < treasureFactor; i++) spawns.Add(TileSpawnType.Treasure);
            if (isBoss) spawns.Add(TileSpawnType.BossSpawn);
            if (isQuest) spawns.Add(TileSpawnType.Quest);

            // Generate the map
            GenerateNodes();
            FillNodeTypes(spawns, 4);
            ConvertToImage("map_output.png");
        }

        private void GenerateNodes()
        {
            Console.WriteLine("Generating Nodes...");
            var random = new Random(seed);
            currentPosition = (0, 0);

            while (Nodes.Count < length)
            {
                bool isStart = Nodes.Count == 0;
                bool isEnd = Nodes.Count == length - 1;

                if (!Nodes.ContainsKey(currentPosition))
                {
                    if (isStart) Nodes[currentPosition] = new Node { position = currentPosition, spawnType = TileSpawnType.Start };
                    else if (isEnd) Nodes[currentPosition] = new Node { position = currentPosition, spawnType = TileSpawnType.End };
                    else Nodes[currentPosition] = new Node { position = currentPosition, spawnType = TileSpawnType.None };

                    Console.WriteLine($"Added node at {currentPosition.x}, {currentPosition.y} (total {Nodes.Count}/{length})");
                }

                var directions = new List<(int x, int y)> {  (1, 0), (-1, 0), (0, 1), (0, -1) };
                var dir = directions[random.Next(directions.Count)];


                currentPosition = (
                    currentPosition.x + dir.x,
                    currentPosition.y + dir.y
                );

            }
        }

        // Updates the bounds of the map based on the current node positions
        private void UpdateBounds()
        {
            Console.WriteLine("Updating Bounds...");

            if (Nodes.Count == 0 || Nodes == null)
                throw new InvalidOperationException("No nodes to update bounds.");


            foreach (var node in Nodes.Values)
            {
                if (node.position.x < bounds.topLeft.x) bounds.topLeft.x = node.position.x;
                if (node.position.y < bounds.topLeft.y) bounds.topLeft.y = node.position.y;
                if (node.position.x > bounds.bottomRight.x) bounds.bottomRight.x = node.position.x;
                if (node.position.y > bounds.bottomRight.y) bounds.bottomRight.y = node.position.y;
            }
        }
        private int[,] ConvertToMap(Dictionary<(int, int), Node> Nodes)
        {
            if (Nodes.Count == 0 || length <= 0 || Nodes == null) 
                throw new InvalidOperationException("No nodes to convert to map.");

            UpdateBounds();
            int[,] map = new int[mapSize.width + padding * 2, mapSize.height + padding * 2];
            Console.WriteLine("Converting to Map...");
            foreach (var point in Nodes)
            {
                // Add thickness around node
                var (x, y) = point.Key;
                for (int dx = -thickness; dx <= thickness; dx++)
                {
                    for (int dy = -thickness; dy <= thickness; dy++)
                    {
                        int distSq = dx * dx + dy * dy;
                        if (distSq <= thickness * thickness)
                        {
                            if (map[bounds.bottomRight.x - x + padding + dx, bounds.bottomRight.y - y + padding + dy] == (int)TileSpawnType.Empty)
                            map[bounds.bottomRight.x - x + padding + dx, bounds.bottomRight.y - y + padding + dy] = (int)TileSpawnType.None;
                        }
                    }
                }

                // Add main node
                map[bounds.bottomRight.x - x + padding, bounds.bottomRight.y - y + padding] = (int)point.Value.spawnType;
            }

            return map;
        }
        public void ConvertToImage(string filePath)
        {
            int[,] map = ConvertToMap(Nodes);

            Console.WriteLine("Converting to Image...");
            using (var image = new Image<Rgba32>(map.GetLength(0), map.GetLength(1)))
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    for (int y = 0; y < map.GetLength(1); y++)
                    {
                        
                        // Set pixel color based on map value
                        //image[x, y] = map[x, y] == 1 ? Color.Black : Color.White;
                        image[x, y] = GetColorForTileType(map[x, y]);
                    }
                }

                Console.WriteLine("Saving Image...");
                image.Save(filePath, new PngEncoder());
            }
        }

        public Dictionary<(int x, int y), Node> GetNodesInRadius((int x, int y) center, int radius)
        {
            Console.WriteLine($"Getting nodes in radius {radius} around {center.x}, {center.y}");
            var nodesInRadius = new Dictionary<(int x, int y), Node>();

            foreach (var node in Nodes.Values)
            {
                if (Math.Abs(node.position.x - center.x) <= radius && Math.Abs(node.position.y - center.y) <= radius)
                {
                    nodesInRadius.Add(node.position, node);
                }
            }

            return nodesInRadius;
        }

        private bool IsNodesRadiusOccupied((int x, int y) position, int radius, bool includeTypeNone = false)
        {
            var nodesInRadius = GetNodesInRadius(position, radius);

            foreach (var node in nodesInRadius.Values)
            {
                if (!includeTypeNone && node.spawnType == TileSpawnType.None)
                    continue;

                return true;
            }

            return false;
        }

        private void FillNodeTypes(List<TileSpawnType> typesToFill, int collisionRadius)
        {
            Console.WriteLine("Filling Node Types...");
            var random = new Random(seed);

            var keysToUpdate = new List<(int x, int y)>();

            // Fill empty nodes
            foreach (var kvp in Nodes)
            {
                var node = kvp.Value;
                // Skip if already assigned
                if (node.spawnType != TileSpawnType.None)
                    continue;

                // Select random type from typesToFill
                int index = random.Next(typesToFill.Count);
                TileSpawnType typeToAssign = typesToFill[index];

                // remove is special type
                if (typeToAssign == TileSpawnType.BossSpawn
                || typeToAssign == TileSpawnType.Quest)
                    typesToFill.RemoveAt(index);

                if (!IsNodesRadiusOccupied(node.position, collisionRadius))
                {
                    keysToUpdate.Add(kvp.Key);
                    Nodes[kvp.Key] = new Node { position = node.position, spawnType = typeToAssign };
                }
            }

            // --- Ensure important types are assigned ---
            var importantTypes = new List<TileSpawnType> { TileSpawnType.BossSpawn, TileSpawnType.Quest };

            // Remove already assigned important types
            foreach (var kvp in Nodes)
            {
                var node = kvp.Value;
                if (importantTypes.Contains(node.spawnType))
                {
                    importantTypes.Remove(node.spawnType);
                }
            }
        }

        public Color GetColorForTileType(int type)
        {
            return type switch
            {
                (int)TileSpawnType.None => Color.Gray,
                (int)TileSpawnType.Start => Color.Green,
                (int)TileSpawnType.End => Color.Red,
                (int)TileSpawnType.Treasure => Color.Gold,
                (int)TileSpawnType.EnemySpawn => Color.Purple,
                (int)TileSpawnType.Landmark => Color.Blue,
                (int)TileSpawnType.BossSpawn => Color.DarkRed,
                (int)TileSpawnType.Quest => Color.Orange,
                _ => Color.Black,
            };
        }

        public class PrefabGenerator
        {

        }

        public enum TileSpawnType
        {
            Empty,
            Start,
            End,
            Treasure,
            EnemySpawn,
            Landmark,
            BossSpawn,
            Quest,
            None
        }

        enum TerrainType
        {
            Wall,
            Path,
            Floor,
            Water,
            Nature,
            Lava,
            Bridge,
        }
        public struct Node 
        {
            public (int x, int y) position;
            public TileSpawnType spawnType;
        }
    }
}