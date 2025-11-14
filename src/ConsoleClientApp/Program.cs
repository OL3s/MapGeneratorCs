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
        map.GenerateMap(includePathNodes: true);
        map.SaveMapAsImage();

        var dij = new DijGenerator(map.PathNodes, map.StartPosition);

        // Find closest object nodes of a specific type
        var closest = map.FindClosestObjectNodesOfTypeByAirDistance(
            new Vect2D(10, 10),
            map.NodeContainer.NodesObjects,
            maxSearchDistance: 50,
            maxObjectCount: 5
        );

        // pathfinding A*
        var aStar = new AStarGenerator(map.PathNodes);
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
        var dijSingleTargetPath = DijUtils.FindDijPathFromPathNodes(
            map.PathNodes,
            map.StartPosition,
            map.EndPosition
        );

        PathImagify.SavePathAndMapToImage(map, dijSingleTargetPath, "dij_single_target_path_output.png");
        PathImagify.SaveDijFullMapToImage(dij.dijNodes, "dij_full_map_output.png");
    }
}