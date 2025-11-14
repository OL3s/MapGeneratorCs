using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using MapGeneratorCs.Types;

namespace MapGeneratorCs.PathFinding.Image;

public class PathImagify
{
    public static void SavePathAndMapToImage(MapConstructor map, List<Vect2D> path, string filename, string filePath = "export/")
    {

        // add .png tag if missing
        if (!filename.EndsWith(".png"))
            filename += ".png";

        (int maxX, int maxY) = (0, 0);
        foreach (var floorNode in map.NodeContainer.NodesFloor)
        {
            if (floorNode.x > maxX) maxX = floorNode.x;
            if (floorNode.y > maxY) maxY = floorNode.y;
        }

        HashSet<Vect2D> pathSet = path.ToHashSet();

        int width = maxX + 1;
        int height = maxY + 1;
        using var image = new Image<Rgba32>(width, height);
        foreach (var floorNode in map.NodeContainer.NodesFloor)
        {
            // base floor
            image[floorNode.x, floorNode.y] = new Rgba32(50, 50, 50); // Floor dark gray

            // override object
            if (map.NodeContainer.NodesObjects.ContainsKey(floorNode))

                // You may want to check if the object at this node is a TrapObject
                if (map.NodeContainer.NodesObjects[floorNode] == TileSpawnType.TrapObject)
                    image[floorNode.x, floorNode.y] = new Rgba32(20, 20, 100); // Blue floor object
                else
                    image[floorNode.x, floorNode.y] = new Rgba32(10, 10, 50); // Dark blue object

            //override path
            if (pathSet.Contains(floorNode))
            {
                image[floorNode.x, floorNode.y] = new Rgba32(255, 255, 0); // Yellow for path

                // startnode green
                if (floorNode == path[0])
                    image[floorNode.x, floorNode.y] = new Rgba32(0, 255, 0);

                // endnode red
                if (floorNode == path[^1])
                    image[floorNode.x, floorNode.y] = new Rgba32(255, 0, 0);
            }
        }

        image[path[0].x, path[0].y] = new Rgba32(0, 255, 0); // Green for start
        image[path[^1].x, path[^1].y] = new Rgba32(255, 0, 0); // Red for goal

        image.Save(filePath + filename, new PngEncoder());
        Console.WriteLine($"Path image saved to {filePath + filename}");
    }
}