using MapGeneratorCs;
using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.AStar;
using MapGeneratorCs.PathFinding.Dijkstra.Utils;
using MapGeneratorCs.PathFinding.Utils;
using MapGeneratorCs.PathFinding.Dijkstra;
using MapGeneratorCs.PathFinding.Image;
using MapGeneratorCs.Generator.Types;

internal class Program
{
    private static void Main(string[] args)
    {
        MapConstructor map;

        if (args.Length > 0 && int.TryParse(args[0], out int len))
            map = new MapConstructor(
                overwriteExisting: true,
                spawnWeights: null,
                mapConfig: new MapConfig() { Length = len }
            );
        else
            map = new MapConstructor();
        map.GenerateMap();
        map.GeneratePathNodes();
        map.SaveMapAsImage();

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

        // pathfinding Dijkstra Raw
        var dijPath = DijUtils.CreateDijPathFromPathNodes(
            map.PathNodes,
            startPos,
            goalPos
        );

        // pathfinding precomputed Dij
        var dijGenerator = new DijGenerator(map.PathNodes, startPos);
        dijGenerator.DijNodes.InitFullMap();
        var dijPrecompPath = dijGenerator.FindPath(goalPos);

        aStar.SavePathAndMapToImage(pathToGoal);
        dijGenerator.SavePathToImage(dijPrecompPath);
        PathImagify.SavePathValuesToImage(dijGenerator.DijNodes, "dijkstra_cost_output.png");
        PathImagify.SavePathToImage(map.PathNodes, dijPath, "dijkstra_path_output.png");
    }
}