using MapGeneratorCs;
using MapGeneratorCs.PathFinding.AStar;

internal class Program
{
    private static void Main(string[] args)
    {
        var map = new MapConstructor();
        map.GenerateMap();
        map.SaveMapAsImage();

        var aStar = new AStarGenerator(map.NodeContainer, includePrintLog: true);

        // use static helper instead of instance method
        var closest = map.FindClosestObjectNodesOfTypeByAirDistance(
            new Vect2D(10, 10),
            map.NodeContainer.NodesObjects,
            maxSearchDistance: 50,
            maxObjectCount: 5
        );

        var startPos = map.StartPosition;
        var goalPos = map.EndPosition;


        var pathToGoal = aStar.FindPath(
            startPos,
            goalPos
        );

        if (pathToGoal == null)
        {
            Console.WriteLine("No path found from start to goal.");
            return;
        }
        
        aStar.SavePathAndMapToImage(map, pathToGoal);
    }
}