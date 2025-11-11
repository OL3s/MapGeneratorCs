using System.Linq;
using MapGeneratorCs.PathFinding;
using MapGeneratorCs.Types;
namespace MapGeneratorCs.Tests;

public class PathFindingOnMapTests
{
    // mark as integration so you can filter it out in quick runs
    [Fact]
    public void GeneratedMap_Pathfinding_Completes_Successfully()
    {
        PathFinding.Utils.PathFindingUtils.IncludeTimerLog = true;
        var map = new MapConstructor();
        map.GenerateMap();

        // Find start object node
        if (!map.NodeContainer.NodesObjects.Any(kv => kv.Value == TileSpawnType.StartObject))
            Assert.True(false, "Generated map contains no StartObject node");
        Vect2D start = map.NodeContainer.NodesObjects.First(kv => kv.Value == TileSpawnType.StartObject).Key;

        // Find goal object node
        if (!map.NodeContainer.NodesObjects.Any(kv => kv.Value == TileSpawnType.EndObject))
            Assert.True(false, "Generated map contains no EndObject node");
        Vect2D goal = map.NodeContainer.NodesObjects.First(kv => kv.Value == TileSpawnType.EndObject).Key;

        // Check if start node and end node are on floor
        Assert.True(map.NodeContainer.NodesFloor.Contains(start), "StartObject node is not on floor");
        Assert.True(map.NodeContainer.NodesFloor.Contains(goal), "EndObject node is not on floor");

        var pathGenerator = new PathGenerator(map.NodeContainer);

        // Act
        var path = pathGenerator.FindPath(start, goal);

        // Assert
        Assert.NotNull(path);
        Assert.True(path!.Count >= 2);
        Assert.Equal(start, path[0]);
        Assert.Equal(goal, path[^1]);
    }

}