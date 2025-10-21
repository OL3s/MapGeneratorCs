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

        public MapConstructor(int length, int thickness)
        {
            this.length = length;
            this.seed = Random.Shared.Next();
            this.thickness = thickness;
            this.padding = thickness + 1;

            GenerateNodes();
            FillNodeTypes(new List<TileSpawnType>
            {
                TileSpawnType.Treasure,
                TileSpawnType.EnemySpawn,
                TileSpawnType.Landmark,
                TileSpawnType.BossSpawn,
                TileSpawnType.Quest
            }, 4);
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
                var (x, y) = point.Key;
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

        private void FillNodeTypes(List<TileSpawnType> typesToFill, int collisionRadius = 4)
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

            // Try to assign remaining important types
            var retries = 0;
            while (importantTypes.Count > 0)
            {
                retries++;
                if (retries > 1000)
                    throw new InvalidOperationException("Failed to assign important node types after 1000 retries.");
                var randomNodeKey = new List<(int x, int y)>(Nodes.Keys)[random.Next(Nodes.Count)];
                var randomNode = Nodes[randomNodeKey];

                if (randomNode.spawnType == TileSpawnType.None
                || randomNode.spawnType == TileSpawnType.Start
                || randomNode.spawnType == TileSpawnType.End)
                    continue;
                
                int index = random.Next(importantTypes.Count);
                TileSpawnType typeToAssign = importantTypes[index];

                if (!IsNodesRadiusOccupied(randomNode.position, collisionRadius))
                {
                    keysToUpdate.Add(randomNodeKey);
                    Nodes[randomNodeKey] = new Node { position = randomNode.position, spawnType = typeToAssign };
                    importantTypes.RemoveAt(index);
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