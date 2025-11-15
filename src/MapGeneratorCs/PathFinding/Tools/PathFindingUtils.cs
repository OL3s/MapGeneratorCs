using System;
using System.Collections.Generic;
using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.Logging;
using MapGeneratorCs.Generator.Types;

namespace MapGeneratorCs.PathFinding.Utils;

public static class PathFindingUtils
{

    // Pre-allocated offsets for optimized neighbour queries
    private static readonly Vect2D[] OrthogonalOffsets = new[]
    {
        new Vect2D( 1, 0),
        new Vect2D(-1, 0),
        new Vect2D( 0, 1),
        new Vect2D( 0,-1),
    };

    private static readonly Vect2D[] DiagonalOffsets = new[]
    {
        new Vect2D( 1, 1),
        new Vect2D( 1,-1),
        new Vect2D(-1, 1),
        new Vect2D(-1,-1)
    };


    public static float GetNodeMovementPenalty(TileSpawnType type) =>
        type switch
        {
            // Movementcost = (1, sqrt(2) for diagonal) + GetNodeMovementPenalty()
            TileSpawnType.Default => 0f,
            TileSpawnType.TrapObject => 3f,
            TileSpawnType.TreasureObject => 100f,
            TileSpawnType.LandmarkObject => 100f,
            TileSpawnType.PropObject => 100f,
            _ => 10f
        };

    public static HashSet<PathNode> GetNeighbours(
        Vect2D pos,
        PathNodes nodes)
    {
        // small expected size (up to 8), pre-size to reduce rehashes
        var neighbours = new HashSet<PathNode>(8);

        // orthogonal neighbours (4 checks)
        foreach (var offset in OrthogonalOffsets)
        {
            var neighbourPos = new Vect2D(pos.x + offset.x, pos.y + offset.y);
            if (nodes.TryGetValue(neighbourPos, out var neighbour))
                neighbours.Add(neighbour);
        }

        // diagonal only if both adjacent orthogonal tiles exist
        foreach (var offset in DiagonalOffsets)
        {
            var a = new Vect2D(pos.x + offset.x, pos.y);
            var b = new Vect2D(pos.x, pos.y + offset.y);

            // TryGetValue to avoid double dictionary lookups with ContainsKey
            if (!nodes.TryGetValue(a, out var aNode) || !nodes.TryGetValue(b, out var bNode))
                continue;

            var diagPos = new Vect2D(pos.x + offset.x, pos.y + offset.y);
            if (nodes.TryGetValue(diagPos, out var neighbour))
                neighbours.Add(neighbour);
        }

        return neighbours;
    }

    // Extra cost when moving diagonally: sum of penalties of the two corner-adjacent tiles.
    public static float CalculateCornerPenalty(PathNodes nodes, Vect2D from, Vect2D to)
    {
        // Only applies to diagonal moves
        if (from.x == to.x || from.y == to.y)
            return 0f;

        var a = new Vect2D(to.x, from.y); // horizontal corner
        var b = new Vect2D(from.x, to.y); // vertical corner

        // Neighbours are created only if both exist, but be safe
        if (!nodes.TryGetValue(a, out var aNode) || !nodes.TryGetValue(b, out var bNode))
            return 0f;

        return aNode.MovementPenalty + bNode.MovementPenalty;
    }

    public static void ResetNodeCosts(PathNodes pathNodes)
    {
        foreach (var node in pathNodes.Values)
        {
            node.CostFromStart = float.MaxValue;
            node.HeuristicCost = float.MaxValue;
            node.ParentNode = null;
        }
    }

    public static float CalculateHeuristic(Vect2D a, Vect2D b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    public static List<Vect2D> RetracePath(PathNode goal)
    {
        var path = new List<Vect2D>();
        var current = goal;

        while (current != null)
        {
            path.Add(current.Position);
            current = current.ParentNode!;
        }

        path.Reverse();
        return path;
    }

    public static PathNodes CreatePathNodesFromMap(
        NodeContainerData container)
    {
        var nodes = new PathNodes();

        Console.WriteLine("PathFindingUtils: Creating path node generation...");
        foreach (var position in container.NodesFloor)
        {
            var type = container.NodesObjects.ContainsKey(position)
                ? container.NodesObjects[position]
                : TileSpawnType.Default;
            nodes.Add(position, new PathNode
            {
                Position = position,
                NodeType = type,
                MovementPenalty = GetNodeMovementPenalty(type)
            });
        }

        foreach (var pair in nodes)
        {
            pair.Value.Neighbours = GetNeighbours(pair.Key, nodes);
        }

        return nodes;
    }
}
