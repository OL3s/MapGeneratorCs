using MapGeneratorCs.Types;
using MapGeneratorCs.Generator.Types;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.PathFinding.ALT;
using MapGeneratorCs.PathFinding.Dijkstra;
using MapGeneratorCs.PathFinding.Dijkstra.Utils;
using MapGeneratorCs.PathFinding.AStar.Utils;

namespace MapGeneratorCs.Tests;

public class AltTests
{
    private PathNodes BuildFullGrid(int w,int h)
    {
        var c = new NodeContainerData();
        for(int x=0;x<w;x++)
            for(int y=0;y<h;y++)
                c.NodesFloor.Add(new Vect2D(x,y));
        var pn = new PathNodes();
        pn.Generate(c);
        return pn;
    }

    [Fact]
    public void Landmarks_Are_Different_Positions()
    {
        var pn = BuildFullGrid(8,8);
        var start = new Vect2D(0,0);
        var alt = new ALTGenerator(start, landmarkCount: 4, pn);
        var pos = alt.Landmarks.GetPositions();
        Assert.True(pos.Distinct().Count() == pos.Count);
    }

    [Fact]
    public void Alt_Heuristic_Admissible()
    {
        var pn = BuildFullGrid(6,6);
        var start = new Vect2D(0,0);
        var goal = new Vect2D(5,5);
        var alt = new ALTGenerator(start, landmarkCount: 4, pn);
        // run raw A* cost path
        var aStarPath = AStarUtils.FindPath(pn, start, goal);
        var altPath = alt.FindPath(start, goal);
        Assert.NotNull(aStarPath);
        Assert.NotNull(altPath);
        // ALT must not produce longer path than A*
        Assert.True(altPath!.Count <= aStarPath!.Count);
    }
}