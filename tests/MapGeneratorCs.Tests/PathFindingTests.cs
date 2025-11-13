using MapGeneratorCs.PathFinding;
using MapGeneratorCs.PathFinding.AStar;
namespace MapGeneratorCs.Tests;

public class PathFindingTests
{
    [Fact]
    public void Finds_Straight_Path_On_Empty_Grid()
    {
        var container = new NodeContainerData();
        for (int x = 0; x < 5; x++)
            container.NodesFloor.Add(new Vect2D(x, 0));

        var pathGen = new AStarGenerator(container);
        var path = pathGen.FindPath(new Vect2D(0, 0), new Vect2D(4, 0));

        Assert.NotNull(path);
        Assert.Equal(5, path!.Count);
        Assert.Equal(new Vect2D(0, 0), path[0]);
        Assert.Equal(new Vect2D(4, 0), path[^1]);
    }

    [Fact]
    public void Finds_Diagonal_Path_On_Empty_Grid()
    {
        var container = new NodeContainerData();
        // Fill a 5x5 grid so all diagonal and orthogonal neighbors exist
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                container.NodesFloor.Add(new Vect2D(x, y));

        var pathGen = new AStarGenerator(container);
        var path = pathGen.FindPath(new Vect2D(0, 0), new Vect2D(4, 4));

        Assert.NotNull(path);
        Assert.Equal(5, path!.Count);
        Assert.Equal(new Vect2D(0, 0), path[0]);
        Assert.Equal(new Vect2D(4, 4), path[^1]);
    }

    [Fact]
    public void Avoids_High_Penalty_Node()
    {
        var container = new NodeContainerData();
        // 3x1 grid: (0,0), (1,0), (2,0)
        for (int x = 0; x < 3; x++)
            container.NodesFloor.Add(new Vect2D(x, 0));
        // Set middle node to high penalty
        container.NodesObjects[new Vect2D(1, 0)] = TileSpawnType.LandmarkObject;

        var pathGen = new AStarGenerator(container);
        var path = pathGen.FindPath(new Vect2D(0, 0), new Vect2D(2, 0));

        Assert.NotNull(path);
        // Should still find a path, but cost will be high due to penalty
        Assert.Equal(3, path!.Count);
        Assert.Contains(new Vect2D(1, 0), path);
    }

    [Fact]
    public void Prefers_Low_Penalty_Over_High_Penalty()
    {
        var container = new NodeContainerData();
        // 3x2 grid
        for (int x = 0; x < 3; x++)
        {
            container.NodesFloor.Add(new Vect2D(x, 0));
            container.NodesFloor.Add(new Vect2D(x, 1));
        }
        // Block (1,0) with high penalty, (1,1) with low penalty
        container.NodesObjects[new Vect2D(1, 0)] = TileSpawnType.LandmarkObject;
        container.NodesObjects[new Vect2D(1, 1)] = TileSpawnType.TrapObject;

        var pathGen = new AStarGenerator(container);
        var path = pathGen.FindPath(new Vect2D(0, 0), new Vect2D(2, 0));

        Assert.NotNull(path);
        // Should go through (1,1) instead of (1,0)
        Assert.Contains(new Vect2D(1, 1), path);
        Assert.DoesNotContain(new Vect2D(1, 0), path);
    }

    [Fact]
    public void Returns_Null_When_No_Path_Exists()
    {
        var container = new NodeContainerData();
        // Only two isolated nodes, not neighbors
        container.NodesFloor.Add(new Vect2D(0, 0));
        container.NodesFloor.Add(new Vect2D(10, 10));

        var pathGen = new AStarGenerator(container);
        var path = pathGen.FindPath(new Vect2D(0, 0), new Vect2D(10, 10));

        Assert.Null(path);
    }

    [Fact]
    public void Find_Path_On_Fake_Map()
    {
        // Use a small 5x5 grid with some obstacles
        var container = new NodeContainerData();
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                container.NodesFloor.Add(new Vect2D(x, y));

        // Place a barrier of high-penalty nodes at x=2, except for a gap at (2,2)
        for (int y = 0; y < 5; y++)
            if (y != 2)
                if (y % 2 == 0)
                    container.NodesObjects[new Vect2D(2, y)] = TileSpawnType.TrapObject;
                else
                    container.NodesObjects[new Vect2D(2, y)] = TileSpawnType.LandmarkObject;

        var pathGen = new AStarGenerator(container);
        var path = pathGen.FindPath(new Vect2D(0, 0), new Vect2D(4, 4));

        Assert.NotNull(path);
        // Path should go through the gap at (2,2)
        Assert.Contains(new Vect2D(2, 2), path!);
        // Path should not contain any of the high-penalty wall except the gap
        Assert.DoesNotContain(new Vect2D(2, 0), path);
        Assert.DoesNotContain(new Vect2D(2, 1), path);
        Assert.DoesNotContain(new Vect2D(2, 3), path);
        Assert.DoesNotContain(new Vect2D(2, 4), path);
    }

        [Fact]
    public void StartEqualsGoal_ReturnsSingleNodePath()
    {
        var container = new NodeContainerData();
        container.NodesFloor.Add(new Vect2D(0, 0));

        var pg = new AStarGenerator(container);
        var path = pg.FindPath(new Vect2D(0, 0), new Vect2D(0, 0));

        Assert.NotNull(path);
        Assert.Single(path);
        Assert.Equal(new Vect2D(0, 0), path[0]);
    }

    [Fact]
    public void MissingStartOrGoal_ReturnsNull()
    {
        var container = new NodeContainerData();
        container.NodesFloor.Add(new Vect2D(0, 0));
        // goal missing
        var pg = new AStarGenerator(container);
        var path = pg.FindPath(new Vect2D(0, 0), new Vect2D(1, 0));
        Assert.Null(path);

        // start missing
        path = pg.FindPath(new Vect2D(1, 0), new Vect2D(0, 0));
        Assert.Null(path);
    }

    [Fact]
    public void DiagonalBlocked_WhenCornersMissing()
    {
        var container = new NodeContainerData();
        // only diagonal nodes, but not orthogonal neighbors -> diagonal should be blocked
        container.NodesFloor.Add(new Vect2D(0, 0));
        container.NodesFloor.Add(new Vect2D(1, 1));

        var pg = new AStarGenerator(container);
        var path = pg.FindPath(new Vect2D(0, 0), new Vect2D(1, 1));
        Assert.Null(path);
    }

    [Fact]
    public void Finds_CostOptimal_Path_With_Penalties()
    {
        var container = new NodeContainerData();
        // 3x1 corridor and an alternative 3x2 detour
        for (int x = 0; x < 3; x++)
        {
            container.NodesFloor.Add(new Vect2D(x, 0));
            container.NodesFloor.Add(new Vect2D(x, 1));
        }
        // Make middle straight node very expensive so algorithm prefers detour via y=1
        container.NodesObjects[new Vect2D(1, 0)] = TileSpawnType.LandmarkObject; // high penalty via GetNodeMovementPenalty
        container.NodesObjects[new Vect2D(1, 1)] = TileSpawnType.TrapObject; // lower penalty

        var pg = new AStarGenerator(container);
        var path = pg.FindPath(new Vect2D(0, 0), new Vect2D(2, 0));

        Assert.NotNull(path);
        Assert.Contains(new Vect2D(1, 1), path);
        Assert.DoesNotContain(new Vect2D(1, 0), path);
    }

    [Fact]
    public void Path_Does_Not_Work_On_Diagonal_With_Border_Collision()
    {
        var container = new NodeContainerData();
        // "Plus" formation: top, center and right exist, but top-right corner (2,0) is missing.
        // Start at top (1,0) -> goal at right (2,1).
        // Diagonal (1,0)->(2,1) should be blocked, so path must be (1,0) -> (1,1) -> (2,1).
        container.NodesFloor.Add(new Vect2D(1, 0)); // top (start)
        container.NodesFloor.Add(new Vect2D(1, 1)); // center (intermediate)
        container.NodesFloor.Add(new Vect2D(2, 1)); // right (goal)
        // Note: do NOT add (2,0) so diagonal is disallowed by GetNeighbours

        var pg = new AStarGenerator(container);
        var path = pg.FindPath(new Vect2D(1, 0), new Vect2D(2, 1));

        Assert.NotNull(path);
        Assert.Equal(3, path!.Count);
        Assert.Equal(new Vect2D(1, 0), path[0]);
        Assert.Equal(new Vect2D(1, 1), path[1]); // down first
        Assert.Equal(new Vect2D(2, 1), path[2]); // then right
    }
}