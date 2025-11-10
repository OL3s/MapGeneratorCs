
using MapGeneratorCs.Types;
namespace MapGeneratorCs.PathFinding;

public static class PathFindingTools
{
    public static float GetObjectWeight(TileSpawnType nodeType){
        return nodeType switch
        {
            TileSpawnType.TrapObject => 10,
            TileSpawnType.TreasureObject => 20,
            TileSpawnType.LandmarkObject => 400,
            TileSpawnType.PropObject => 20,
            _ => 0
        };
    }
}
public static class PathFindingStatic
{

}

public class PathFindingDynamic
{
    Dictionary<Vect2D, PathNode> Nodes = new Dictionary<Vect2D, PathNode>();
    public class PathNode
    {
        public float? Value = null;
        public TileSpawnType Type;
        public PathNode[] Neighbours;
        public PathNode(PathNode[] neighbours, TileSpawnType type) {
            Neighbours = neighbours;
            Type = type;
        }
    }
    public float GetObjectWeight(TileSpawnType nodeType)
    {
        return nodeType switch
        {
            TileSpawnType.TrapObject => 10,
            TileSpawnType.TreasureObject => 20,
            TileSpawnType.LandmarkObject => 400,
            TileSpawnType.PropObject => 20,
            _ => 0
        };
    }
}