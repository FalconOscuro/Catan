using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Catan;

static class Utility
{
    public static void ShuffleArray<T>(this Random rand, T[] array, int iterations = 1)
    {
        int length = array.Length;

        for (int i = 0; i < 2; i++)
        {
            int n = length;

            while (n > 1)
            {
                int pos = rand.Next(n--);

                T temp = array[n];
                array[n] = array[pos];
                array[pos] = temp;
            }
        }
    }

    public static Vector2 FlipY(this Vector2 vector, float screenHeight)
    {
        vector.Y = screenHeight * (1f - (vector.Y / screenHeight));
        return vector;
    }

    public static Vector2 FlipY(this Point point, float screenHeight)
    {
        return new Vector2(point.X, screenHeight * (1f - (point.Y / screenHeight)));
    }

    public static List<Edge> Dijkstra(this Node start, in Player targetPlayer)
    {
        Queue<DQueueItem> priorityQueue = new Queue<DQueueItem>();
        Stack<DQueueItem> searched = new Stack<DQueueItem>();

        priorityQueue.Enqueue(new DQueueItem(start));

        while (priorityQueue.Count > 0)
        {
            DQueueItem currentNode = priorityQueue.Dequeue();
            searched.Push(currentNode);

            // Shortest path found
            if (currentNode.IsTarget(targetPlayer))
                return currentNode.GetPath();
            
            // Cannot traverse
            else if (currentNode.IsSettled())
                continue;
            
            // Add new nodes
            for (int i = 0; i < 3; i++)
            {
                DQueueItem newNode = currentNode.SearchNeighbour(i);
                if (newNode == null)
                    continue;
                
                // All edge distances are equal so if a node exists, shortest path has already been found
                else if (priorityQueue.Contains(newNode) || searched.Contains(newNode))
                    continue;
                
                priorityQueue.Enqueue(newNode);
            }
        }

        // No path found
        return null;
    }

    private class DQueueItem : IEquatable<DQueueItem>
    {
        public DQueueItem(Node target, DQueueItem parent = null)
        {
            Target = target;
            Parent = parent;
        }

        public bool IsTarget(in Player targetPlayer)
        {
            if (Target.Owner == targetPlayer)
                return true;

            else if (IsSettled())
                return false;
            
            foreach (Edge edge in Target.Edges)
                if (edge != null)
                    if (edge.Owner == targetPlayer)
                        return true;
            
            return false;
        }

        public bool IsSettled()
        {
            return Target.Owner != null;
        }

        public bool Equals(DQueueItem queueItem)
        {
            return Target == queueItem.Target;
        }

        public DQueueItem SearchNeighbour(int index)
        {
            Edge targetEdge = Target.Edges[index];

            if (targetEdge == null)
                return null;
            
            else if (targetEdge.Owner != null)
                return null;
            
            return new DQueueItem(Target.GetNeighbourNode(index), this);
        }

        public List<Edge> GetPath()
        {
            if (Parent == null)
                return new List<Edge>();
            
             List<Edge> path = Parent.GetPath();
             path.Add(GetConnectingEdge());

             return path;
        }

        public Edge GetConnectingEdge()
        {
            if (Parent == null)
                return null;

            for (int i = 0; i < 3; i++)
            {
                Node node = Target.GetNeighbourNode(i);

                if (node == null)
                    continue;
                
                else if (node == Parent.Target)
                    return Target.Edges[i];
            }

            return null;
        }

        public Node Target { get; private set; }

        public DQueueItem Parent;
    }
}