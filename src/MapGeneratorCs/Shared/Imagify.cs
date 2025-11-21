using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.ExportKit;

namespace MapGeneratorCs.Image;

public class Imagify
{
    public static void SavePathToImage(PathNodes pathNodes, List<Vect2D> path, string filename, string filePath = "export/")
    {

        if (path == null || path.Count == 0)
        {
            Console.WriteLine("No path provided to visualize.");
            return;
        }

        // add .png tag if missing
        if (!filename.EndsWith(".png"))
            filename += ".png";

        (int maxX, int maxY) = (0, 0);
        foreach (var kvp in pathNodes)
        {
            var pos = kvp.Key;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y > maxY) maxY = pos.y;
        }

        HashSet<Vect2D> pathSet = path.ToHashSet();

        int width = maxX + 1;
        int height = maxY + 1;
        using var image = new Image<Rgba32>(width, height);
        foreach (var kvp in pathNodes)
        {
            var pos = kvp.Key;

            // base floor
            image[pos.x, pos.y] = new Rgba32(50, 50, 50); // Floor dark gray

            // override object
            if (pathNodes[pos].NodeType != TileSpawnType.Default)

                // You may want to check if the object at this node is a TrapObject
                if (pathNodes[pos].NodeType == TileSpawnType.TrapObject)
                    image[pos.x, pos.y] = new Rgba32(20, 20, 100); // Blue floor object
                else
                    image[pos.x, pos.y] = new Rgba32(10, 10, 50); // Dark blue object

            //override path
            if (pathSet.Contains(pos))
            {
                image[pos.x, pos.y] = new Rgba32(255, 255, 0); // Yellow for path

                // startnode green
                if (pos == path[0])
                    image[pos.x, pos.y] = new Rgba32(0, 255, 0);
                // endnode red
                if (pos == path[^1])
                    image[pos.x, pos.y] = new Rgba32(255, 0, 0);
            }
        }

        image[path[0].x, path[0].y] = new Rgba32(0, 255, 0); // Green for start
        image[path[^1].x, path[^1].y] = new Rgba32(255, 0, 0); // Red for goal

        image.Save(filePath + filename, new PngEncoder());
        Console.WriteLine($"Path image saved to {filePath + filename}");
    }

    public static void SavePathValuesToImage(PathNodes graph, Dictionary<Vect2D, float> dist, string filename, string filePath = "export/")
    {
        if (!filename.EndsWith(".png"))
            filename += ".png";

        int maxX = 0, maxY = 0;
        foreach (var kv in graph)
        {
            var p = kv.Key;
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
        }

        int width = maxX + 1;
        int height = maxY + 1;

        // Prevent OOM on huge images
        const long MAX_PIXELS = 50_000_000;
        long total = (long)width * height;
        if (total > MAX_PIXELS)
        {
            Console.WriteLine($"PathImagify: skip saving '{filename}' - {width}x{height} exceeds pixel limit.");
            return;
        }

        using var image = new Image<Rgba32>(width, height);

        var finiteCosts = dist.Values.Where(v => v < float.MaxValue).ToList();
        float maxCost = finiteCosts.Count > 0 ? finiteCosts.Max() : 0f;

        foreach (var kv in graph)
        {
            var pos = kv.Key;
            if (!dist.TryGetValue(pos, out var c) || c >= float.MaxValue || maxCost <= 0f)
            {
                image[pos.x, pos.y] = new Rgba32(0, 0, 0);
                continue;
            }
            byte intensity = (byte)(255f * (c / maxCost));
            image[pos.x, pos.y] = new Rgba32(intensity, intensity, intensity);
        }

        image.Save(filePath + filename, new PngEncoder());
        Console.WriteLine($"Path values image saved to {filePath + filename}");
    }

    public static void SavePointOfInterestToImage(PathNodes pathNodes, List<Vect2D> pointPositions, string filename, string filePath = "export/", int radius = 3)
    {
        // add .png tag if missing
        if (!filename.EndsWith(".png"))
            filename += ".png";

        (int maxX, int maxY) = (0, 0);
        foreach (var kvp in pathNodes)
        {
            var pos = kvp.Key;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y > maxY) maxY = pos.y;
        }

        int width = maxX + 1;
        int height = maxY + 1;
        using var image = new Image<Rgba32>(width, height);
        foreach (var kvp in pathNodes)
        {
            var pos = kvp.Key;

            // base floor
            image[pos.x, pos.y] = new Rgba32(50, 50, 50); // Floor dark gray
        }

        // mark POI positions
        foreach (var point in pointPositions)
        {  

            // Draw a filled square (cube in 2D) of given radius around the POI
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = point.x + dx;
                    int y = point.y + dy;
                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        image[x, y] = new Rgba32(255, 0, 255); // Magenta for POI area
                    }
                }
            }
        }

        image.Save(filePath + filename, new PngEncoder());
        Console.WriteLine($"POI image saved to {filePath + filename}");
    }

    public static void SavePointOfInterestToImage(PathNodes pathNodes, List<PathResult> pointPositions, string filename, string filePath = "export/", int radius = 3)
    {
        // add .png tag if missing
        if (!filename.EndsWith(".png"))
            filename += ".png";

        (int maxX, int maxY) = (0, 0);
        foreach (var kvp in pathNodes)
        {
            var pos = kvp.Key;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y > maxY) maxY = pos.y;
        }

        int width = maxX + 1;
        int height = maxY + 1;
        using var image = new Image<Rgba32>(width, height);
        foreach (var kvp in pathNodes)
        {
            var pos = kvp.Key;
            // base floor
            image[pos.x, pos.y] = new Rgba32(50, 50, 50); // Floor dark gray
        }

        // mark POI positions (use the final node of each PathResult as the POI center)
        foreach (var pathResult in pointPositions)
        {
            if (pathResult == null || pathResult.Count == 0) continue;
            var poi = pathResult[^1]; // destination / object position

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = poi.x + dx;
                    int y = poi.y + dy;
                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        image[x, y] = new Rgba32(255, 0, 255); // Magenta for POI area
                    }
                }
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        image.Save(filePath + filename, new PngEncoder());
        Console.WriteLine($"POI image saved to {filePath + filename}");
    }

   public static void SaveMapToImage(MapConstructor map, string filename, string filePath = "export/", bool includeGenerateNodes = false)
    {
        if (map == null) throw new ArgumentNullException(nameof(map));
        if (map.NodeContainer == null || map.NodeContainer.NodesFloor == null || map.NodeContainer.NodesFloor.Count == 0 || map.mapConfig.Length <= 0)
            throw new InvalidOperationException("No NodesFloor to convert to map.");

        int maxX = 0, maxY = 0;
        foreach (var p in map.NodeContainer.NodesFloor)
        {
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
        }

        var width = maxX + map.mapConfig.Thickness + map.padding;
        var height = maxY + map.mapConfig.Thickness + map.padding;

        Console.WriteLine($"Imagify: Saving MapConstructor to image ({width}x{height})...");

        using var image = new Image<Rgba32>(width, height);

        // initialize to Empty (black)
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                image[x, y] = GetColorForTileType(TileSpawnType.Empty);

        // mark floor
        foreach (var p in map.NodeContainer.NodesFloor)
        {
            if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height)
                image[p.x, p.y] = GetColorForTileType(TileSpawnType.Default);
        }

        // override with object nodes
        if (map.NodeContainer.NodesObjects != null)
        {
            foreach (var kv in map.NodeContainer.NodesObjects)
            {
                var p = kv.Key;
                var tile = kv.Value;
                if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < height)
                    image[p.x, p.y] = GetColorForTileType(tile);
            }
        }

        // override with generate nodes
        if (map.NodeContainer.NodesGenerate != null)
        {
            foreach (var kv in map.NodeContainer.NodesGenerate)
            {
                var p = kv.Key;
                var tile = kv.Value;
                if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height) continue;

                if (!includeGenerateNodes && IsGenerator(tile))
                    image[p.x, p.y] = GetColorForTileType(TileSpawnType.Default);
                else
                    image[p.x, p.y] = GetColorForTileType(tile);
            }
        }

        // save
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        if (!filename.EndsWith(".png"))
            filename += ".png";
        image.Save(filePath + filename, new PngEncoder());
        Console.WriteLine($"Map image saved to {filePath + filename}");
    }

    // determine if a tile is a generator type
    private static bool IsGenerator(TileSpawnType type)
    {
        // Check if the enum name ends with "Generator"
        return type.ToString().EndsWith("Generator");
    }

    // simple color mapping for TileSpawnType
    private static Rgba32 GetColorForTileType(TileSpawnType tileType)
    {
        return tileType switch
        {
            TileSpawnType.Empty => new Rgba32(0, 0, 0), // Black
            TileSpawnType.Default => new Rgba32(60, 60, 60), // Medium gray (floor)
            TileSpawnType.TrapObject => new Rgba32(10, 10, 90), // Dark blue (trap)
            TileSpawnType.StartObject => new Rgba32(0, 255, 0), // Green (start)
            TileSpawnType.EndObject => new Rgba32(255, 0, 255), // Magenta (end)
            TileSpawnType.EnemyObject => new Rgba32(220, 40, 40), // Bright red (enemy)
            TileSpawnType.TreasureObject => new Rgba32(255, 220, 40), // Gold (treasure)
            TileSpawnType.BossObject => new Rgba32(160, 0, 160), // Purple (boss)
            TileSpawnType.QuestObject => new Rgba32(0, 255, 255), // Cyan (quest)
            TileSpawnType.LandmarkObject => new Rgba32(20, 20, 255), // Light blue (landmark)
            TileSpawnType.PropObject => new Rgba32(30, 30, 90), // blue (prop)
            // generator visuals (if included)
            TileSpawnType.DefaultGenerator => new Rgba32(120, 120, 120), // Light gray
            TileSpawnType.EnemyGenerator => new Rgba32(255, 100, 100), // Light red
            TileSpawnType.TreasureGenerator => new Rgba32(255, 255, 100), // Light yellow
            TileSpawnType.TrapGenerator => new Rgba32(100, 100, 255), // Light blue
            TileSpawnType.EmptyGenerator => new Rgba32(40, 40, 40), // Dark gray
            TileSpawnType.StartGenerator => new Rgba32(0, 180, 0), // Darker green
            TileSpawnType.EndGenerator => new Rgba32(180, 0, 0), // Darker red
            TileSpawnType.BossGenerator => new Rgba32(120, 0, 120), // Dark purple
            TileSpawnType.QuestGenerator => new Rgba32(0, 180, 180), // Dark cyan
            TileSpawnType.LandmarkGenerator => new Rgba32(180, 0, 180), // Dark magenta
            // fallback
            _ => new Rgba32(10, 10, 50),
        };
    }

    public static void SaveExportedBackgroundToImage(ExportedTilessetBackground background, string filename, string filePath = "export/")
    {
        if (!filename.EndsWith(".png"))
            filename += ".png";

        int maxX = 0, maxY = 0;
        foreach (var kv in background)
        {
            var p = kv.Key;
            if (p.x > maxX) maxX = p.x;
            if (p.y > maxY) maxY = p.y;
        }

        int width = maxX + 1;
        int height = maxY + 1;

        using var image = new Image<Rgba32>(width, height);

        foreach (var kv in background)
        {
            var pos = kv.Key;
            var tileBg = kv.Value;
            Rgba32 color = tileBg switch
            {
                ExportTileBackground.Default => new Rgba32(40, 80, 40),
                ExportTileBackground.Padded => new Rgba32(80, 80, 80),
                ExportTileBackground.TopWall => new Rgba32(20, 40, 20),
                _ => new Rgba32(0, 0, 0),
            };
            image[pos.x, pos.y] = color;
        }

        image.Save(filePath + filename, new PngEncoder());
        Console.WriteLine($"Exported background image saved to {filePath + filename}");
    }
}