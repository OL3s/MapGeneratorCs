using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Types;

namespace MapGeneratorCs.PathFinding.Image;

public class PathImagify
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
}