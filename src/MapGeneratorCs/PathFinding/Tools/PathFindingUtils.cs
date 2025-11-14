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

    public static void ResetNodeCosts(PathNodes pathNodes)
    {
        var timeLogger = new TimeLogger();
        timeLogger.Print("PathFindingUtils: Resetting node costs", false);
        foreach (var node in pathNodes.Values)
        {
            node.CostFromStart = float.MaxValue;
            node.HeuristicCost = float.MaxValue;
            node.ParentNode = null;
        }
        timeLogger.Print("PathFindingUtils: Finished resetting node costs");
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
        var timeLogger = new TimeLogger();

        timeLogger.Print("PathFindingUtils: Creating path node generation", false);
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

        timeLogger.Print("PathFindingUtils: Finished path node generation");
        timeLogger.Print("PathFindingUtils: Assigning neighbours", false);
        foreach (var pair in nodes)
        {
            pair.Value.Neighbours = GetNeighbours(pair.Key, nodes);
        }

        timeLogger.Print("PathFindingUtils: Finished assigning node neighbours");
        return nodes;
    }
}
