using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.PathFinding.AStar;
using MapGeneratorCs.PathFinding.Dijkstra;
using MapGeneratorCs.PathFinding.Dijkstra.Utils;
using MapGeneratorCs.PathFinding.ALT;

namespace MapGeneratorCs.Tests;

public class PathFindingTest
{
    // Build a simple grid as floor nodes, with optional blocked tiles (holes) and object penalties.
    private static PathNodes BuildGrid(int w, int h, HashSet<Vect2D>? blocked = null, Dictionary<Vect2D, TileSpawnType>? objects = null)
    {
        var container = new NodeContainerData();

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var p = new Vect2D(x, y);
                if (blocked != null && blocked.Contains(p)) continue;
                container.NodesFloor.Add(p);
            }
        }

        if (objects != null)
        {
            foreach (var kv in objects)
            {
                container.NodesObjects[kv.Key] = kv.Value;
                container.NodesFloor.Add(kv.Key); // ensure floor exists under objects
            }
        }

        var nodes = new PathNodes();
        nodes.Generate(container);
        return nodes;
    }

    [Fact]
    public void AStar_StraightDiagonal_OnEmpty3x3()
    {
        var nodes = BuildGrid(3, 3);
        var a = new AStarGenerator(nodes);

        var path = a.FindPath(new Vect2D(0, 0), new Vect2D(2, 2));

        Assert.NotNull(path);
        Assert.Equal(3, path!.Count);
        Assert.Equal(new Vect2D(0, 0), path[0]);
        Assert.Equal(new Vect2D(2, 2), path[^1]);
        Assert.InRange(path.Cost, 2 * MathF.Sqrt(2) - 1e-3f, 2 * MathF.Sqrt(2) + 1e-3f);
    }

    [Fact]
    public void Diagonal_Disallowed_WhenOrthAdjMissing()
    {
        // Remove (1,0) so the immediate diagonal (0,0)->(1,1) is illegal (one orth is missing)
        var blocked = new HashSet<Vect2D> { new Vect2D(1, 0) };
        var nodes = BuildGrid(3, 3, blocked);
        var a = new AStarGenerator(nodes);

        var path = a.FindPath(new Vect2D(0, 0), new Vect2D(2, 2));

        Assert.NotNull(path);
        // Must not take the illegal first diagonal step
        Assert.NotEqual(new Vect2D(1, 1), path![1]);
        // Still should reach goal via a detour
        Assert.True(path.Count >= 4);
    }

    [Fact]
    public void CornerPenalty_MakesDiagonalMoreExpensive()
    {
        // Put TrapObjects in the two corner-adjacent tiles of a diagonal move (1,0) and (0,1)
        // Diagonal (0,0)->(1,1) cost ~ sqrt(2) + 6 ≈ 7.414
        // Orth path (0,0)->(1,0)->(1,1) or via (0,1) costs 1+3 + 1+0 = 5
        var objects = new Dictionary<Vect2D, TileSpawnType>
        {
            [new Vect2D(1, 0)] = TileSpawnType.TrapObject,
            [new Vect2D(0, 1)] = TileSpawnType.TrapObject
        };
        var nodes = BuildGrid(2, 2, null, objects);
        var a = new AStarGenerator(nodes);

        var path = a.FindPath(new Vect2D(0, 0), new Vect2D(1, 1));

        Assert.NotNull(path);
        Assert.Equal(3, path!.Count);
        Assert.InRange(path.Cost, 5f - 1e-3f, 5f + 1e-3f);
    }

    [Fact]
    public void DijkstraRaw_vs_AStar_On5x5_WithWallBarrier()
    {
        // Vertical wall with a gap forces a detour; compare A* vs raw Dijkstra costs/length
        var blocked = new HashSet<Vect2D> { new Vect2D(2, 1), new Vect2D(2, 2), new Vect2D(2, 3) };
        var nodes = BuildGrid(5, 5, blocked);
        var start = new Vect2D(0, 2);
        var goal = new Vect2D(4, 2);

        var a = new AStarGenerator(nodes);
        var pathA = a.FindPath(start, goal);
        Assert.NotNull(pathA);

        var pathD = DijUtils.CreateDijPathFromPathNodes(nodes, start, goal);
        Assert.NotNull(pathD);

        Assert.Equal(pathA!.Count, pathD!.Count);
        Assert.InRange(pathA.Cost, pathD.Cost - 1e-3f, pathD.Cost + 1e-3f);
    }

    [Fact]
    public void DijkstraPrecompute_FindsSameCost_AsRaw()
    {
        var nodes = BuildGrid(5, 5);
        var start = new Vect2D(0, 0);
        var goal = new Vect2D(4, 4);
        var pre = new DijGenerator(nodes, start).FindPath(goal);
        var raw = DijUtils.CreateDijPathFromPathNodes(nodes, start, goal);

        Assert.NotNull(pre);
        Assert.NotNull(raw);
        Assert.InRange(pre!.Cost, raw!.Cost - 1e-3f, raw!.Cost + 1e-3f);
    }

    [Fact]
    public void ALT_FindsPath_WithCostEqualTo_AStar()
    {
        var nodes = BuildGrid(5, 5);
        var start = new Vect2D(0, 0);
        var goal = new Vect2D(4, 4);

        var alt = new ALTGenerator(start, landmarkCount: 4, nodes);
        var pathAlt = alt.FindPath(start, goal);

        var a = new AStarGenerator(nodes);
        var pathA = a.FindPath(start, goal);

        Assert.NotNull(pathAlt);
        Assert.NotNull(pathA);
        Assert.InRange(pathAlt!.Cost, pathA!.Cost - 1e-3f, pathA.Cost + 1e-3f);
    }

    [Fact]
    public void GeneratedMap_HasPath_StartToEnd()
    {
        var map = new MapConstructor(overwriteExisting: true);
        map.GenerateMap();
        map.GeneratePathNodes();

        Assert.NotNull(map.PathNodes);
        var a = new AStarGenerator(map.PathNodes!);
        var path = a.FindPath(map.StartPosition, map.EndPosition);

        Assert.NotNull(path);
        Assert.True(path!.Count > 1);
    }

    [Fact]
    public void OvercostedPath_IsRejected_ByAStar_AndDijkstra()
    {
        var nodes = BuildGrid(5, 5);
        var start = new Vect2D(0, 0);
        var goal = new Vect2D(4, 4);

        var a = new AStarGenerator(nodes);
        var pathA = a.FindPath(start, goal, 1f);
        Assert.Null(pathA);

        var dij = new DijGenerator(nodes, start);
        var pathD = dij.FindPath(goal, maxSearchCost: 1f);
        Assert.Null(pathD);
    }
}