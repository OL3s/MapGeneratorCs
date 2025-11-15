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

    public static void SavePathValuesToImage(PathNodes pathNodes, string filename, string filePath = "export/")
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
        float maxCost = pathNodes.Values.Any() ? pathNodes.Values.Max(n => n.CostFromStart) : 0f;
        foreach (var kvp in pathNodes)
        {
            var pos = kvp.Key;
            var node = kvp.Value;

            // Normalize cost to 0-255 range
            byte intensity = maxCost > 0 ? (byte)(255 * (node.CostFromStart / maxCost)) : (byte)0;

            // Grayscale based on cost
            image[pos.x, pos.y] = new Rgba32(intensity, intensity, intensity);
        }

        image.Save(filePath + filename, new PngEncoder());
        Console.WriteLine($"Path values image saved to {filePath + filename}");
    }
}