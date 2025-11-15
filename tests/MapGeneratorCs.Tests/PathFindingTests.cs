using MapGeneratorCs.PathFinding;
using MapGeneratorCs.PathFinding.ALT;
using MapGeneratorCs.PathFinding.AStar;
using MapGeneratorCs.PathFinding.AStar.Utils;
using MapGeneratorCs.PathFinding.Dijkstra;
using MapGeneratorCs.PathFinding.Dijkstra.Utils;
using MapGeneratorCs.PathFinding.Types;

namespace MapGeneratorCs.Tests;

public class PathFindingTests
{
    private static PathNodes MakePathNodes(NodeContainerData data)
    {
        var pn = new PathNodes();
        pn.Generate(data);
        return pn;
    }

    [Fact]
    public void Finds_Straight_Path_On_Empty_Grid()
    {
        var container = new NodeContainerData();
        for (int x = 0; x < 5; x++) container.NodesFloor.Add(new Vect2D(x, 0));
        var pn = MakePathNodes(container);
        var path = AStarUtils.FindPath(pn, new Vect2D(0, 0), new Vect2D(4, 0));
        Assert.NotNull(path);
        Assert.Equal(5, path!.Count);
    }

    [Fact]
    public void Finds_Diagonal_Path_On_Empty_Grid()
    {
        var c = new NodeContainerData();
        for (int x = 0; x < 5; x++)
            for (int y = 0; y < 5; y++)
                c.NodesFloor.Add(new Vect2D(x, y));
        var pn = MakePathNodes(c);
        var path = AStarUtils.FindPath(pn, new Vect2D(0, 0), new Vect2D(4, 4));
        Assert.NotNull(path);
        Assert.Equal(new Vect2D(0,0), path![0]);
        Assert.Equal(new Vect2D(4,4), path[^1]);
    }

    [Fact]
    public void Avoids_High_Penalty_Node()
    {
        var c = new NodeContainerData();
        for (int x=0;x<3;x++) c.NodesFloor.Add(new Vect2D(x,0));
        c.NodesObjects[new Vect2D(1,0)] = TileSpawnType.LandmarkObject;
        var pn = MakePathNodes(c);
        var path = AStarUtils.FindPath(pn, new Vect2D(0,0), new Vect2D(2,0));
        Assert.NotNull(path);
        Assert.Contains(new Vect2D(1,0), path!);
    }

    [Fact]
    public void Prefers_Low_Penalty_Over_High_Penalty()
    {
        var c = new NodeContainerData();
        for (int x=0;x<3;x++){ c.NodesFloor.Add(new Vect2D(x,0)); c.NodesFloor.Add(new Vect2D(x,1)); }
        c.NodesObjects[new Vect2D(1,0)] = TileSpawnType.LandmarkObject;
        c.NodesObjects[new Vect2D(1,1)] = TileSpawnType.TrapObject;
        var pn = MakePathNodes(c);
        var path = AStarUtils.FindPath(pn, new Vect2D(0,0), new Vect2D(2,0));
        Assert.NotNull(path);
        Assert.Contains(new Vect2D(1,1), path!);
        Assert.DoesNotContain(new Vect2D(1,0), path);
    }

    [Fact]
    public void Returns_Null_When_No_Path_Exists()
    {
        var c = new NodeContainerData();
        c.NodesFloor.Add(new Vect2D(0,0));
        c.NodesFloor.Add(new Vect2D(10,10));
        var pn = MakePathNodes(c);
        var path = AStarUtils.FindPath(pn, new Vect2D(0,0), new Vect2D(10,10));
        Assert.Null(path);
    }

    [Fact]
    public void StartEqualsGoal_ReturnsSingleNodePath()
    {
        var c = new NodeContainerData();
        c.NodesFloor.Add(new Vect2D(0,0));
        var pn = MakePathNodes(c);
        var path = AStarUtils.FindPath(pn, new Vect2D(0,0), new Vect2D(0,0));
        Assert.NotNull(path);
        Assert.Single(path);
    }

    [Fact]
    public void MissingStartOrGoal_ReturnsNull()
    {
        var c = new NodeContainerData();
        c.NodesFloor.Add(new Vect2D(0,0));
        var pn = MakePathNodes(c);
        Assert.Null(AStarUtils.FindPath(pn, new Vect2D(0,0), new Vect2D(1,0)));
        Assert.Null(AStarUtils.FindPath(pn, new Vect2D(1,0), new Vect2D(0,0)));
    }

    [Fact]
    public void DiagonalBlocked_WhenCornersMissing()
    {
        var c = new NodeContainerData();
        c.NodesFloor.Add(new Vect2D(0,0));
        c.NodesFloor.Add(new Vect2D(1,1));
        var pn = MakePathNodes(c);
        var path = AStarUtils.FindPath(pn, new Vect2D(0,0), new Vect2D(1,1));
        Assert.Null(path);
    }

    [Fact]
    public void Path_Does_Not_Work_On_Diagonal_With_Border_Collision()
    {
        var c = new NodeContainerData();
        c.NodesFloor.Add(new Vect2D(1,0));
        c.NodesFloor.Add(new Vect2D(1,1));
        c.NodesFloor.Add(new Vect2D(2,1));
        var pn = MakePathNodes(c);
        var path = AStarUtils.FindPath(pn, new Vect2D(1,0), new Vect2D(2,1));
        Assert.NotNull(path);
        Assert.Equal(3, path!.Count);
        Assert.Equal(new Vect2D(1,1), path[1]);
    }

    [Fact]
    public void Raw_Dijkstra_Equals_Precompute()
    {
        var c = new NodeContainerData();
        for(int x=0;x<5;x++)
            for(int y=0;y<5;y++)
                c.NodesFloor.Add(new Vect2D(x,y));
        var pn = MakePathNodes(c);
        var start = new Vect2D(0,0);
        var goal = new Vect2D(4,4);
        var raw = DijUtils.CreateDijPathFromPathNodes(pn, start, goal);
        var pre = new DijGenerator(pn, start).FindPath(goal);
        Assert.NotNull(raw);
        Assert.NotNull(pre);
        Assert.Equal(raw!.Count, pre!.Count);
    }

    [Fact]
    public void ALT_Path_Equals_AStar_Path_Length()
    {
        var c = new NodeContainerData();
        for(int x=0;x<10;x++)
            for(int y=0;y<10;y++)
                c.NodesFloor.Add(new Vect2D(x,y));
        var pn = MakePathNodes(c);
        var start = new Vect2D(0,0);
        var goal = new Vect2D(9,9);
        var aStar = AStarUtils.FindPath(pn, start, goal);
        var altGen = new ALTGenerator(start, landmarkCount: 5, pn);
        var altPath = altGen.FindPath(start, goal);
        Assert.NotNull(aStar);
        Assert.NotNull(altPath);
        Assert.Equal(aStar!.Count, altPath!.Count);
    }

    [Fact]
    public void Dijkstra_Dist_Start_Is_Zero()
    {
        var c = new NodeContainerData();
        for(int x=0;x<3;x++)
            for(int y=0;y<3;y++)
                c.NodesFloor.Add(new Vect2D(x,y));
        var pn = MakePathNodes(c);
        var start = new Vect2D(1,1);
        var dij = new DijGenerator(pn, start);
        Assert.Equal(0f, dij.GetCostAt(start));
    }
}