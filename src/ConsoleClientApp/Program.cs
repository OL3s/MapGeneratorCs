using MapGeneratorCs;
using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.AStar;
using MapGeneratorCs.PathFinding.Dijkstra.Utils;
using MapGeneratorCs.PathFinding.Utils;
using MapGeneratorCs.PathFinding.Dijkstra;

internal class Program
{
    private static void Main(string[] args)
    {
        var map = new MapConstructor();
        map.GenerateMap();
        map.SaveMapAsImage();

        
        var dij = new DijGenerator(map.NodeContainer, map.StartPosition);

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

        // pathfinding Dijkstra
        var dijPathToGoal = dij.FindPath(
            goalPos
        );
        
        aStar.SavePathAndMapToImage(map, pathToGoal);
        dij.SavePathAndMapToImage(map, dijPathToGoal);

        // dij single-target pathfinding
        var dijSingleTargetPath = DijUtils.FindDijPathFromMap(
            map.NodeContainer,
            map.StartPosition,
            map.EndPosition
        );
        dij.SavePathAndMapToImage(map, dijSingleTargetPath);
    }
}