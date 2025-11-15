using MapGeneratorCs.PathFinding.ALT;
using MapGeneratorCs.PathFinding.AStar;
using MapGeneratorCs.PathFinding.Dijkstra;
using MapGeneratorCs.PathFinding.Dijkstra.Utils;
namespace MapGeneratorCs.Tests;

public class PathFindingOnMapTests
{
    [Fact]
    public async Task GeneratedMap_Pathfinding_Completes_Successfully()
    {
        var map = new MapConstructor();
        map.GenerateMap();
        map.GeneratePathNodes();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        while ((map.NodeContainer == null || !map.NodeContainer.NodesFloor.Any()) && sw.Elapsed < TimeSpan.FromSeconds(10))
        {
            await Task.Delay(100);
        }

        Assert.NotNull(map.NodeContainer);
        Assert.True(map.NodeContainer!.NodesFloor.Any(), "NodeContainer has no floor nodes after generation");

        Assert.NotNull(map.PathNodes);
        var pathGenerator = new AStarGenerator(map.PathNodes!); // UPDATED

        var path = pathGenerator.FindPath(map.StartPosition, map.EndPosition);

        Assert.NotNull(path);
        Assert.True(path!.Count >= 2);
        Assert.Equal(map.StartPosition, path[0]);
        Assert.Equal(map.EndPosition, path[^1]);

        // Dijkstra raw vs precompute vs ALT equivalence
        var rawDij = DijUtils.CreateDijPathFromPathNodes(map.PathNodes, map.StartPosition, map.EndPosition);
        Assert.NotNull(rawDij);
        var preDij = new DijGenerator(map.PathNodes, map.StartPosition).FindPath(map.EndPosition);
        Assert.NotNull(preDij);
        Assert.Equal(rawDij!.Count, preDij!.Count);

        var alt = new ALTGenerator(map.StartPosition, landmarkCount: 4, map.PathNodes);
        var altPath = alt.FindPath(map.StartPosition, map.EndPosition);
        Assert.NotNull(altPath);
        Assert.Equal(path!.Count, altPath!.Count);
    }
}