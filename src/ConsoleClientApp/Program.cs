using MapGeneratorCs;
using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.AStar;
using MapGeneratorCs.PathFinding.Dijkstra.Utils;
using MapGeneratorCs.PathFinding.Utils;
using MapGeneratorCs.PathFinding.Dijkstra;
using MapGeneratorCs.PathFinding.Image;

internal class Program
{
    private static void Main(string[] args)
    {
        var map = new MapConstructor();
        map.GenerateMap();
        map.SaveMapAsImage();

        // Find closest object nodes of a specific type
        var closest = map.FindClosestObjectNodesOfTypeByAirDistance(
            new Vect2D(10, 10),
            map.NodeContainer.NodesObjects,
            maxSearchDistance: 50,
            maxObjectCount: 5
        );

        // pathfinding A*
        var aStar = new AStarGenerator(map.NodeContainer);
        var startPos = map.StartPosition;
        var goalPos = map.EndPosition;
        var pathToGoal = aStar.FindPath(
            startPos,
            goalPos
        );

        aStar.SavePathAndMapToImage(map, pathToGoal);
    }
}