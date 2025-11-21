using MapGeneratorCs;
using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.AStar;
using MapGeneratorCs.PathFinding.Dijkstra.Utils;
using MapGeneratorCs.PathFinding.Utils;
using MapGeneratorCs.PathFinding.Dijkstra;
using MapGeneratorCs.Image;
using MapGeneratorCs.PathFinding.ALT;
using MapGeneratorCs.Generator.Types;
using MapGeneratorCs.Logging;
using MapGeneratorCs.ExportKit;
using MapGeneratorCs.PathFinding.Types;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var stopwatch = new Stopwatch();
        var schoolInfo = new SchoolAssignmentImportantInfo();
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

        // pathfinding AStar
        var aStar = new AStarGenerator(map.PathNodes);
        var startPos = map.StartPosition;
        var goalPos = map.EndPosition;

        stopwatch.Start();
        var pathToGoal = aStar.FindPath(
            startPos,
            goalPos
        );
        stopwatch.Stop();
        schoolInfo.AStar = (stopwatch.ElapsedMilliseconds, pathToGoal.Cost, pathToGoal.VisitedCount);

        // pathfinding Dijkstra Raw
        stopwatch.Restart();
        var dijPath = DijUtils.CreateDijPathFromPathNodes(
            map.PathNodes,
            startPos,
            goalPos
        );
        stopwatch.Stop();
        schoolInfo.DijkstraRaw = (stopwatch.ElapsedMilliseconds, dijPath.Cost, dijPath.VisitedCount);


        // ALT generator
        var altGenerator = new ALTGenerator(startPos, landmarkCount: 5, map.PathNodes);
        stopwatch.Restart();
        var altPath = altGenerator.FindPath(startPos, goalPos);
        stopwatch.Stop();
        schoolInfo.ALT = (stopwatch.ElapsedMilliseconds, altPath.Cost, altPath.VisitedCount);

        // Find 5 closest objects
        var closestByAir = map.FindClosestObjectNodesOfTypeByAirDistance(
            map.StartPosition,
            type: TileSpawnType.TreasureObject,
            radius: 200,
            count: 200
        );

        // collect up to 5 best paths
        var objPathList = new List<PathResult>();

        foreach (var objPos in closestByAir)
        {
            var pathToObj = altGenerator.FindPath(
                startPos,
                objPos,
                maxSearchCost: 300f
            );
            if (pathToObj != null)
                objPathList.Add(pathToObj);
        }

        objPathList.Sort((a, b) => a.Cost.CompareTo(b.Cost));
        objPathList = objPathList.Take(5).ToList();
        schoolInfo.ClosestObjects = objPathList.Select(p => p.Last()).ToArray();

        foreach (var result in objPathList)
        {
            Console.WriteLine($"Found path to object {result}");
        }

        // Save results in images
        altGenerator.SaveLandmarkPositionAsImage(12);
        aStar.SavePathAndMapToImage(pathToGoal);
        Imagify.SavePathToImage(altGenerator.pathNodes, altPath, "alt_path_output.png");

        // draw precomputed dist map
        Imagify.SavePathToImage(map.PathNodes, dijPath, "dijkstra_rawpath_output.png");

        int i = 0;
        foreach (var landmark in altGenerator.Landmarks)
        {
            Imagify.SavePathValuesToImage(map.PathNodes, landmark.Dist, $"alt_landmark_{i+1}_cost_output.png");
            i++;
        }

        // Export tilesset background
        var exportedBackground = ExportKit.GenerateDefaultBackground(map.NodeContainer);
        Imagify.SaveExportedBackgroundToImage(exportedBackground, "tilesset_background_output.png");

        // Export tilesset with objects
        Imagify.SavePointOfInterestToImage(map.PathNodes, objPathList, "tilesset_with_5_closest_treasure_output.png", radius: 6);

        Console.WriteLine("School Assignment Important Info:");
        Console.WriteLine(schoolInfo.ToString());
    }

    private struct SchoolAssignmentImportantInfo
    {
        public (float TimerAStar, float PathLength, int VisitedCount) AStar;
        public (float TimerDijkstrRaw, float PathLength, int VisitedCount) DijkstraRaw;
        public (float TimerALT, float PathLength, int VisitedCount) ALT;
        public Vect2D[] ClosestObjects;
        public override string ToString()
        {
            var closest = (ClosestObjects == null || ClosestObjects.Length == 0)
                ? "none"
                : string.Join(", ", ClosestObjects);
            return
                $"  - A*: {AStar.TimerAStar} ms, length {AStar.PathLength}, visited {AStar.VisitedCount}\n" +
                $"  - Dijkstra Raw: {DijkstraRaw.TimerDijkstrRaw} ms, length {DijkstraRaw.PathLength}, visited {DijkstraRaw.VisitedCount}\n" +
                $"  - ALT: {ALT.TimerALT} ms, length {ALT.PathLength}, visited {ALT.VisitedCount}\n" +
                "  - Closest Objects: " + closest;
        }
    }
}

