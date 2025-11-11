using MapGeneratorCs;
using MapGeneratorCs.PathFinding;
using MapGeneratorCs.Types;

internal class Program
{
    private static void Main(string[] args)
    {
        var map = new MapConstructor();
        map.GenerateMap();
        map.SaveMapAsImage();

        var pathGenerator = new PathGenerator(map.NodeContainer, includePrintLog: true);

        // use static helper instead of instance method
        var closest = pathGenerator.FindClosestObjectNodesOfType(
            new Vect2D(10, 10),
            map.NodeContainer.NodesObjects,
            maxSearchDistance: 50,
            maxObjectCount: 5
        );

        Console.WriteLine($"Found {closest.Count} matching object nodes within distance 50.");
        foreach (var pos in closest)
        {
            Console.WriteLine($" - Object at position {pos}");
        }
    }
}