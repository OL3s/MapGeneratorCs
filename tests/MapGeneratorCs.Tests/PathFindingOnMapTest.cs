using System.Linq;
using MapGeneratorCs.PathFinding;
using MapGeneratorCs.Types;
namespace MapGeneratorCs.Tests;

public class PathFindingOnMapTests
{
    // mark as integration so you can filter it out in quick runs
    [Fact]
    public async Task GeneratedMap_Pathfinding_Completes_Successfully()
    {
        PathFinding.Utils.PathFindingUtils.IncludeTimerLog = true;
        var map = new MapConstructor();
        map.GenerateMap();

        // Wait for generation to finish (guard against background/async generation)
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while ((map.NodeContainer == null || !map.NodeContainer.NodesFloor.Any()) && sw.Elapsed < TimeSpan.FromSeconds(10))
        {
            await Task.Delay(100);
        }

        Assert.NotNull(map.NodeContainer);
        Assert.True(map.NodeContainer!.NodesFloor.Any(), "NodeContainer has no floor nodes after generation");

        var pathGenerator = new PathGenerator(map.NodeContainer);

        // Act
        var path = pathGenerator.FindPath(map.StartPosition, map.EndPosition);

        // Assert
        Assert.NotNull(path);
        Assert.True(path!.Count >= 2); // At least start and end positions
        Assert.Equal(map.StartPosition, path[0]);
        Assert.Equal(map.EndPosition, path[^1]);
    }

}