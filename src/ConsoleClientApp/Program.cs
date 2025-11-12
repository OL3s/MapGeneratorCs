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
        var closest = map.FindClosestObjectNodesOfTypeByAirDistance(
            new Vect2D(10, 10),
            map.NodeContainer.NodesObjects,
            maxSearchDistance: 50,
            maxObjectCount: 5
        );

        var startPos = map.StartPosition;
        var goalPos = map.EndPosition;


        var pathToGoal = pathGenerator.FindPath(
            startPos,
            goalPos
        );

        pathGenerator.SavePathAndMapToImage(map, pathToGoal);
    }
}