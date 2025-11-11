using MapGeneratorCs.Types;
using System;
using System.Collections.Generic;

namespace MapGeneratorCs.PathFinding.Utils;

public static class PathFindingUtils
{
    // Create path nodes from the map data
    public static Dictionary<Vect2D, PathNode> CreatePathNodesFromMap(
        NodeContainerData container)
    {
        var nodes = new Dictionary<Vect2D, PathNode>();

        foreach (var position in container.NodesFloor)
        {
            nodes.Add(position, new PathNode
            {
                Position = position,
                NodeType = TileSpawnType.Default
            });
        }

        foreach (var pair in nodes)
        {
            var position = pair.Key;
            var node = pair.Value;

            node.NodeType = GetNodeType(position, container);
            node.MovementPenalty = GetNodeMovementPenalty(node.NodeType);
            node.Neighbours = GetNeighbours(position, nodes);
        }

        return nodes;
    }

    // Determine the node type based on object spawns first, then default
    private static TileSpawnType GetNodeType(Vect2D pos, NodeContainerData container)
    {
        if (container.NodesObjects.ContainsKey(pos))
            return container.NodesObjects[pos];

        return TileSpawnType.Default;
    }

    // Determine movement penalty based on node type
    private static float GetNodeMovementPenalty(TileSpawnType type) =>
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

    // Retrieve orthogonal and diagonal neighbours from the node map
    private static HashSet<PathNode> GetNeighbours(
        Vect2D pos,
        Dictionary<Vect2D, PathNode> nodes)
    {
        var neighbours = new HashSet<PathNode>();

        var orthogonal = new[]
        {
            new Vect2D( 1, 0),
            new Vect2D(-1, 0),
            new Vect2D( 0, 1),
            new Vect2D( 0,-1),
        };

        foreach (var offset in orthogonal)
        {
            var neighbourPos = new Vect2D(pos.x + offset.x, pos.y + offset.y);
            if (nodes.TryGetValue(neighbourPos, out var neighbour))
                neighbours.Add(neighbour);
        }

        // diagonal only if both corners exist
        var diagonal = new[]
        {
            new Vect2D( 1, 1),
            new Vect2D( 1,-1),
            new Vect2D(-1, 1),
            new Vect2D(-1,-1)
        };

        foreach (var offset in diagonal)
        {
            var a = new Vect2D(pos.x + offset.x, pos.y);
            var b = new Vect2D(pos.x, pos.y + offset.y);

            if (!nodes.ContainsKey(a) || !nodes.ContainsKey(b))
                continue;

            var diagPos = new Vect2D(pos.x + offset.x, pos.y + offset.y);
            if (nodes.TryGetValue(diagPos, out var neighbour))
                neighbours.Add(neighbour);
        }

        return neighbours;
    }

    // Reset costs for all nodes in the dictionary
    public static void ResetNodeCosts(Dictionary<Vect2D, PathNode> nodes)
    {
        foreach (var node in nodes.Values)
        {
            node.CostFromStart = float.MaxValue;
            node.HeuristicCost = float.MaxValue;
            node.ParentNode = null;
        }
    }
}
