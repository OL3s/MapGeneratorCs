using MapGeneratorCs.Types;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using static MapGeneratorCs.Generator.Utils.TileColorUtils;

namespace MapGeneratorCs.Generator.Utils.Builders;

public static class IntMapBuilder
{
    public static int[,] CreateGridFromMap(MapConstructor map)
    {
        if (map.NodeContainer.NodesFloor == null || map.NodeContainer.NodesFloor.Count == 0 || map.mapConfig.Length <= 0)
            throw new InvalidOperationException("No NodesFloor to convert to map.");

        int maxX = 0, maxY = 0;
        foreach (var p in map.NodeContainer.NodesFloor)
        {
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
        }

        var width = maxX + map.mapConfig.Thickness + map.padding + 1;
        var height = maxY + map.mapConfig.Thickness + map.padding + 1;
        var grid = new int[width, height];

        Console.WriteLine($"Converting to IntMap2D ({width}x{height})...");

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

        return grid;
    }

    public static void SaveGridToImage(int[,] grid, string filePath, bool includeGenerateNodes = false)
    {
        int w = grid.GetLength(0);
        int h = grid.GetLength(1);
        Console.WriteLine($"Saving map to {filePath} ({w}x{h})...");

        using var image = new Image<Rgba32>(w, h);
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var tileType = (TileSpawnType)grid[x, y];
                if (tileType == TileSpawnType.Empty)
                    continue;

                // includeGenerateNodes flag check
                if (!includeGenerateNodes &&
                    (tileType == TileSpawnType.DefaultGenerator ||
                        tileType == TileSpawnType.EnemyGenerator ||
                        tileType == TileSpawnType.TreasureGenerator ||
                        tileType == TileSpawnType.TrapGenerator ||
                        tileType == TileSpawnType.EmptyGenerator ||
                        tileType == TileSpawnType.StartGenerator ||
                        tileType == TileSpawnType.EndGenerator ||
                        tileType == TileSpawnType.BossGenerator ||
                        tileType == TileSpawnType.QuestGenerator ||
                        tileType == TileSpawnType.TrapGenerator ||
                        tileType == TileSpawnType.LandmarkGenerator))
                {
                    tileType = TileSpawnType.Default;
                }

                image[x, y] = GetColorForTileType(tileType);
            }
        }

        image.Save(filePath, new PngEncoder());
        Console.WriteLine("Map image saved successfully.");
    }
}
