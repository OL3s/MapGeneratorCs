using MapGeneratorCs.PathFinding;
using MapGeneratorCs.Types;

namespace MapGeneratorCs.Tests;

public class PathFindingOnMapTests
{
    // mark as integration so you can filter it out in quick runs
    [Fact]
    public void GeneratedMap_Pathfinding_FindsPathBetweenNearbyNodes()
    {
        // Arrange: generate a map (deterministic if you adjust config/seed beforehand)
        var map = new MapConstructor();
        map.GenerateMap(); // uses ConfigLoader to create defaults if missing

        // pick two nodes that should be connected (use nearest to start to reduce flakiness)
        var nodes = map.NodeContainer.NodesFloor.ToList();
        Assert.True(nodes.Count >= 3, "Generated map too small for test");

        var start = nodes[0];
        var goalCandidate = nodes.FirstOrDefault(n => (Math.Abs(n.x - start.x) + Math.Abs(n.y - start.y)) > 2);
        var goal = !goalCandidate.Equals(default(Vect2D)) ? goalCandidate : nodes[1];

        var pg = new PathGenerator(map.NodeContainer);

        // Act
        var path = pg.FindPath(start, goal);

        // Assert
        Assert.NotNull(path);
        Assert.True(path!.Count >= 2);
        Assert.Equal(start, path[0]);
        Assert.Equal(goal, path[^1]);
    }
}