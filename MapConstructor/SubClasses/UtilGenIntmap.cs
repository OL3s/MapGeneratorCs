using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;

namespace MapGeneratorCs
{
    partial class MapConstructor
    {
        internal static class IntMapBuilder
        {
            public static void BuildFromNodes(MapConstructor map)
            {
                if (map.NodeContainer.NodesFloor == null || map.NodeContainer.NodesFloor.Count == 0 || map.Length <= 0)
                    throw new InvalidOperationException("No NodesFloor to convert to map.");

                if (map.TileMap2D != null)
                    throw new InvalidOperationException("TileMap2D has already been built.");

                int maxX = 0, maxY = 0;
                foreach (var p in map.NodeContainer.NodesFloor)
                {
                    if (p.x > maxX) maxX = p.x;
                    if (p.y > maxY) maxY = p.y;
                }

                var width = maxX + map.Thickness + map.Padding + 1;
                var height = maxY + map.Thickness + map.Padding + 1;
                var grid = map.TileMap2D = new int[width, height];

                Console.WriteLine($"Converting to IntMap2D ({width}x{height})");

                foreach (var p in map.NodeContainer.NodesFloor)
                    if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height)
                        grid[p.x, p.y] = (int)TileSpawnType.Default;

                // Override with object nodes
                foreach (var kv in map.NodeContainer.NodesObjects)
                {
                    var p = kv.Key;
                    if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height)
                        grid[p.x, p.y] = (int)kv.Value;
                }

                // Override with generate nodes
                foreach (var kv in map.NodeContainer.NodesGenerate)
                {
                    var p = kv.Key;
                    if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height)
                        grid[p.x, p.y] = (int)kv.Value;
                }
            }

            public static void SaveToImage(MapConstructor map, string filePath)
            {
                if (map.Grid == null)
                    throw new InvalidOperationException("TileMap2D is null. Build map first.\n  ");

                int w = map.Grid.GetLength(0);
                int h = map.Grid.GetLength(1);
                Console.WriteLine($"Saving map to {filePath} ({w}x{h})...");

                using var image = new Image<Rgba32>(w, h);
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        var tileType = (TileSpawnType)map.Grid[x, y];
                        if (tileType == TileSpawnType.Empty)
                            continue;
                        image[x, y] = TileColorUtils.GetColorForTileType(tileType);
                    }
                }

                image.Save(filePath, new PngEncoder());
                Console.WriteLine("Map image saved successfully.");
            }
        }
    }
}