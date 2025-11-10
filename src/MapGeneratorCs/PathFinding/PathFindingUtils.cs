using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using MapGeneratorCs.Types;
namespace MapGeneratorCs.PathFinding.Utils;

// pathfinding utility functions
public static class PathFindingUtils
{
    public static Dictionary<Vect2D, PathNode> CreatePathNodesFromMap(NodeContainerData nodeContainer)
    {
        // Fill empty nodes thoughout the floor map
        var Nodes = new Dictionary<Vect2D, PathNode>();
        foreach (var position in nodeContainer.NodesFloor)
        {
            Nodes.Add(position, new PathNode());
        }

        // Set types and neighbours
        foreach (var position in Nodes)
        {
            var node = position.Value;
            node.Type = GetObjectType(position.Key, nodeContainer);
            node.Neighbours = GetNeighbours(position.Key, Nodes);
        }
        return Nodes;
    }

    private static TileSpawnType GetObjectType(Vect2D key, NodeContainerData nodeContainer)
    {
        // Override type if in object nodes
        if (nodeContainer.NodesObjects.ContainsKey(key))
            return nodeContainer.NodesObjects[key];
        return TileSpawnType.Default;
    }

    private static HashSet<PathNode> GetNeighbours(Vect2D key, Dictionary<Vect2D, PathNode> nodes)
    {
        var neighbours = new HashSet<PathNode>();
        var directions = new List<Vect2D>
        {
            new Vect2D(1, 0),
            new Vect2D(-1, 0),
            new Vect2D(0, 1),
            new Vect2D(0, -1)
        };
        // Check all 4 directions for neighbours
        foreach (var direction in directions)
        {
            var neighbourPos = new Vect2D(key.x + direction.x, key.y + direction.y);
            if (nodes.TryGetValue(neighbourPos, out var neighbour))
            {
                neighbours.Add(neighbour);
            }
        }

        // Diagonals: only allow if both adjacent orthogonal tiles exist (prevents corner-cutting through walls)
        var diagonalDirections = new List<Vect2D>
        {
            new Vect2D(1, 1),
            new Vect2D(1, -1),
            new Vect2D(-1, 1),
            new Vect2D(-1, -1)
        };

        foreach (var d in diagonalDirections)
        {
            // orthogonal neighbours that must exist to allow diagonal movement
            var orthA = new Vect2D(key.x + d.x, key.y);
            var orthB = new Vect2D(key.x, key.y + d.y);

            // only consider diagonal if both orthogonals are present (not walls)
            if (!nodes.ContainsKey(orthA) || !nodes.ContainsKey(orthB))
                continue;

            var diagPos = new Vect2D(key.x + d.x, key.y + d.y);
            if (nodes.TryGetValue(diagPos, out var diagNeighbour))
            {
                neighbours.Add(diagNeighbour);
            }
        }

        return neighbours;
    }
    public static float GetObjectWeight(TileSpawnType nodeType)
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

    public static void ResetPathNodeValues(Dictionary<Vect2D, PathNode> pathNodes)
    {
        foreach (var node in pathNodes.Values)
        {
            node.Cost = null;
        }
    }

    public static void UpdateAllPathNodesFromPosition(Dictionary<Vect2D, PathNode> pathNodes, Vect2D startPosition)
    {
        if (!pathNodes.ContainsKey(startPosition))
            throw new ArgumentException("Start position not found in path nodes.");

        ResetPathNodeValues(pathNodes);
        var startNode = pathNodes[startPosition];
        startNode.Cost = 0;

        var queue = new Queue<PathNode>();
        queue.Enqueue(startNode);

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();
            foreach (var neighbour in currentNode.Neighbours)
            {
                // Only process neighbours that haven't been discovered yet (Cost is null)
                if (neighbour.Cost != null)
                    continue;

                // Calculate tentative cost to reach neighbour
                if (currentNode.Cost == null)
                    continue;
                    
                float tentativeValue = (float)currentNode.Cost + 1 + GetObjectWeight(neighbour.Type);
                neighbour.Cost = tentativeValue;
                queue.Enqueue(neighbour);
            }
        }
    }
}

public class PathNode
{
    public float Weight = 1.0f;
    public float? Cost = null;
    public TileSpawnType Type = TileSpawnType.Default;
    public HashSet<PathNode> Neighbours = new HashSet<PathNode>();
    public PathNode(HashSet<PathNode> neighbours, TileSpawnType type)
    {
        Neighbours = neighbours;
        Type = type;
        Weight = PathFindingUtils.GetObjectWeight(type);
    }
    public PathNode() { }
}

public static class ImageConverter
{
    private static Dictionary<Vect2D, PathNode> MovePathNodesToPositiveVect2D(Dictionary<Vect2D, PathNode> pathNodes)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;

        foreach (var key in pathNodes.Keys)
        {
            if (key.x < minX) minX = key.x;
            if (key.y < minY) minY = key.y;
        }

        var offsetX = -minX;
        var offsetY = -minY;

        var newPathNodes = new Dictionary<Vect2D, PathNode>();
        foreach (var kvp in pathNodes)
        {
            var newKey = new Vect2D(kvp.Key.x + offsetX, kvp.Key.y + offsetY);
            newPathNodes[newKey] = kvp.Value;
        }

        return newPathNodes;
    }

    public static void ExportPathNodesToImage(Dictionary<Vect2D, PathNode> pathNodes, string filePath)
    {
        var adjustedPathNodes = MovePathNodesToPositiveVect2D(pathNodes);

        int width = 0;
        int height = 0;
        foreach (var key in adjustedPathNodes.Keys)
        {
            if (key.x + 1 > width) width = key.x + 1;
            if (key.y + 1 > height) height = key.y + 1;
        }

        using (var image = new Image<Rgba32>(width, height))
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pos = new Vect2D(x, y);
                    if (adjustedPathNodes.TryGetValue(pos, out var node))
                    {
                        byte intensity = node.Cost.HasValue ? (byte)Math.Clamp((int)(255 - node.Cost.Value), 0, 255) : (byte)0;
                        image[x, y] = new Rgba32(intensity, intensity, intensity);
                    }
                    else
                    {
                        // transparent for non-existing nodes
                        image[x, y] = new Rgba32(0, 0, 0, 0);
                    }
                }
            }
            image.Save(filePath, new PngEncoder());
        }
    }

    public static class AStarPathfinding
    {
        // A* pathfinding algorithm implementation can be added here in the future
    }
}