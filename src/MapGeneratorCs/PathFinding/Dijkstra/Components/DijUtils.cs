using System;
using System.Collections.Generic;
using MapGeneratorCs.Types;
using MapGeneratorCs.PathFinding.Types;
using MapGeneratorCs.Logging;
using MapGeneratorCs.PathFinding.Utils;
using MapGeneratorCs.Generator.Types;

namespace MapGeneratorCs.PathFinding.Dijkstra.Utils;
public static class DijUtils
{

    // Creates a full Dijkstra path map from the start position to all reachable nodes (used for precomputing in ALT)
    public static DijNodes CreateFullDijPathMap(NodeContainerData container, Vect2D start)
    {
        var dijDict = new DijNodes(PathFindingUtils.CreatePathNodesFromMap(container), start);
        
        // Return if start node not in pathNodes dictionary
        if (!dijDict.ContainsKey(start))
            throw new ArgumentException("Start node not found in pathNodes dictionary.");

        var timeLogger = new TimeLogger();
        timeLogger.Print("DijUtils: Starting full Dij path map computation...", false);
        dijDict[start].CostFromStart = 0f; // Set start node cost to 0

        // Priority queue to avoid O(n^2)
        var priorityQueue = new PriorityQueue<Vect2D, float>();
        priorityQueue.Enqueue(start, 0f);

        int processed = 0;
        int estimatedTotal = dijDict.Count;
        while (priorityQueue.TryDequeue(out var currentPos, out var currentPriority))
        {
            // Get the node from the dictionary
            var currentNode = dijDict[currentPos];

            // Skip stale entries: if the priority doesn't match the current known cost
            if (!currentNode.CostFromStart.Equals(currentPriority))
                continue;

            processed++;
            if (processed % 1_000_000 == 0)
                timeLogger.Print($"DijUtils: Processed {processed / 1_000_000}/{estimatedTotal / 1_000_000} million nodes...", false);

            // For each neighbor, update tentative cost
            foreach (var neighborRef in currentNode.Neighbours)
            {
                var neighborNode = dijDict[neighborRef.Position];

                // Diagonal check
                bool diagonal = neighborRef.Position.x != currentNode.Position.x &&
                                neighborRef.Position.y != currentNode.Position.y;
                float stepCost = diagonal ? MathF.Sqrt(2) : 1f;

                float tentativeCost = currentNode.CostFromStart + stepCost + neighborRef.MovementPenalty;
                if (tentativeCost < neighborNode.CostFromStart)
                {
                    neighborNode.CostFromStart = tentativeCost;
                    neighborNode.ParentNode = currentNode;
                    priorityQueue.Enqueue(neighborRef.Position, tentativeCost);
                }
            }
        }
        timeLogger.Print("DijUtils: Finished full Dij path map computation");
        return dijDict;
    }


    // Creates a raw Dijkstra path from start to end using the computed Dij path map
   public static List<Vect2D>? FindDijPathFromMap(NodeContainerData container, Vect2D start, Vect2D end)
    {
        var dijDict = new DijNodes(PathFindingUtils.CreatePathNodesFromMap(container), start);

        // Validate start/end presence
        if (!dijDict.ContainsKey(start) || !dijDict.ContainsKey(end))
            return null;

        // If start == end, return trivial path
        if (start.Equals(end))
            return new List<Vect2D> { start };

        var timeLogger = new TimeLogger();
        timeLogger.Print("DijUtils: Starting Dij single-target pathfinding...", false);

        dijDict[start].CostFromStart = 0f;

        // Use a priority queue for single-target Dijkstra as well.
        var pq2 = new PriorityQueue<Vect2D, float>();
        pq2.Enqueue(start, 0f);

        while (pq2.TryDequeue(out var currentPos2, out var currentPriority2))
        {
            var currentNode = dijDict[currentPos2];

            // Skip stale entries
            if (!currentNode.CostFromStart.Equals(currentPriority2))
                continue;

            // If we've reached the target, build and return path immediately
            if (currentPos2.Equals(end))
            {
                timeLogger.Print("DijUtils: Reached target during Dij normal pathfinding");
                return FindDijPathFromDijNodes(dijDict, end);
            }

            foreach (var neighborRef in currentNode.Neighbours)
            {
                var neighborNode = dijDict[neighborRef.Position];

                bool diagonal = neighborRef.Position.x != currentNode.Position.x &&
                                neighborRef.Position.y != currentNode.Position.y;
                float stepCost = diagonal ? MathF.Sqrt(2) : 1f;

                float tentativeCost = currentNode.CostFromStart + stepCost + neighborRef.MovementPenalty;
                if (tentativeCost < neighborNode.CostFromStart)
                {
                    neighborNode.CostFromStart = tentativeCost;
                    neighborNode.ParentNode = currentNode;
                    pq2.Enqueue(neighborRef.Position, tentativeCost);
                }
            }
        }

        timeLogger.Print("DijUtils: Finished Dij normal pathfinding, no path found");
        return null;
    }


    // Used to find the fastest path from a precomputed Dijkstra path map
    public static List<Vect2D>? FindDijPathFromDijNodes(DijNodes dijNodes, Vect2D end)
    {

        if (!dijNodes.ContainsKey(end))
            return null;

        var path = new List<Vect2D>();
        var currentNode = dijNodes[end];

        while (currentNode != null)
        {
            path.Add(currentNode.Position);
            if (currentNode.Position.Equals(dijNodes.StartPosition))
                break;
            currentNode = currentNode.ParentNode;
        }

        path.Reverse();
        return path;
    }
}

public class DijNodes : PathNodes
{
    public Vect2D StartPosition;
    public DijNodes(PathNodes pathNodes, Vect2D startPosition) : base(pathNodes)
    {
        StartPosition = startPosition;
    }
}