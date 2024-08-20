using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinding
{
    readonly List<Vector3> EMPTY = new List<Vector3>();

    public List<Vector3> AStar(Node start, Node end)
    {
        if (start == null) return default;
        var frontier = new PriorityQueue<Node>();
        frontier.Enqueue(start, 0);
        var cameFrom = new Dictionary<Node, Node>();
        cameFrom.Add(start, null);

        var costSoFar = new Dictionary<Node, int>();
        costSoFar.Add(start, 0);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current == end)
            {
                var path = new List<Vector3>();
                while (current != start)
                {
                    path.Add(current.transform.position);
                    current = cameFrom[current];
                }
                path.Add(start.transform.position); //# optional
                path.Reverse(); // optional
                return path;
            }

            foreach (var item in current.Neighbors)
            {
                if (item.isBlocked) continue;

                int newCost = costSoFar[current] + item.Cost;

                if (!costSoFar.ContainsKey(item) || newCost < costSoFar[current])
                {
                    frontier.Enqueue(item, newCost + Heuristic(end.transform.position, item.transform.position));
                    cameFrom[item] = current;
                    costSoFar[item] = newCost;
                }
            }

        }
        return default;
    }

    public List<Vector3> ThetaStar(Node start, Node end, LayerMask wallLayer)
    {
        if (start == null || end == null) return EMPTY;
        var path = AStar(start, end);

        if (path.Count == 0) return EMPTY;

        int current = 0;
        while (current + 2 < path.Count)
        {
            if (path[current].InLineOfSightOf(path[current + 2], wallLayer))
                path.RemoveAt(current + 1);
            else current++;
        }

        return path;
    }

    float Heuristic(Vector3 start, Vector3 end)
    {
        return Vector3.Distance(start, end);
    }

    float HeuristicManhattan(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    float Heuristic2(Vector3 start, Vector3 end)
    {
        return (start - end).sqrMagnitude;
    }
}
