using System.Collections.Generic;

namespace Coursova.Models
{
    abstract class Algorithm
    {
        public abstract (int distance, List<int> path, List<(int, int)> edges, int operations) FindPath(int[,] graph, int u, int v);
    }

    class FloydWarshall : Algorithm
    {
        public override (int distance, List<int> path, List<(int, int)> edges, int operations) FindPath(int[,] graph, int u, int v)
        {
            int n = graph.GetLength(0);
            int[,] dist = new int[n, n];
            int[,] next = new int[n, n];
            int operations = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    operations++;
                    dist[i, j] = graph[i, j];

                    next[i, j] = -1;

                    if (i != j && graph[i, j] > 0)
                        next[i, j] = j;

                    if (i != j && dist[i, j] == 0)
                        dist[i, j] = int.MaxValue;
                }
            }

            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        operations++;
                        if (dist[i, k] != int.MaxValue && dist[k, j] != int.MaxValue)
                        {
                            int newDist = dist[i, k] + dist[k, j];
                            if (newDist < dist[i, j])
                            {
                                dist[i, j] = newDist;
                                next[i, j] = next[i, k];
                            }
                        }
                    }
                }
            }

            List<int> path = [];
            List<(int, int)> edges = [];

            if (dist[u, v] == int.MaxValue || next[u, v] == -1)
            {
                return (int.MaxValue, path, edges, operations);
            }

            path.Add(u);
            int current = u;

            while (current != v)
            {
                operations++;
                int nextVertex = next[current, v];

                if (nextVertex == -1)
                    break;

                edges.Add((current, nextVertex));
                current = nextVertex;
                path.Add(current);
            }

            return (dist[u, v], path, edges, operations);
        }
    }

    class Dantzig : Algorithm
    {
        public override (int distance, List<int> path, List<(int, int)> edges, int operations) FindPath(int[,] graph, int u, int v)
        {
            int n = graph.GetLength(0);
            int[,] dist = new int[n, n];
            int[,] next = new int[n, n];
            int operations = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    operations++;
                    if (i == j)
                    {
                        dist[i, j] = 0;
                        next[i, j] = -1;
                    }
                    else if (graph[i, j] > 0)
                    {
                        dist[i, j] = graph[i, j];
                        next[i, j] = j;
                    }
                    else
                    {
                        dist[i, j] = int.MaxValue;
                        next[i, j] = -1;
                    }
                }
            }

            for (int m = 0; m < n; m++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        operations++;
                        if (dist[i, m] != int.MaxValue && dist[m, j] != int.MaxValue)
                        {
                            int throughM = dist[i, m] + dist[m, j];
                            if (throughM < dist[i, j])
                            {
                                dist[i, j] = throughM;
                                next[i, j] = next[i, m];
                            }
                        }
                    }
                }
            }

            List<int> path = new List<int>();
            List<(int, int)> edges = new List<(int, int)>();

            if (dist[u, v] == int.MaxValue || next[u, v] == -1)
                return (int.MaxValue, path, edges, operations);

            int current = u;
            path.Add(current);

            while (current != v)
            {
                int nextNode = next[current, v];
                if (nextNode == -1)
                    break;

                edges.Add((current, nextNode));
                current = nextNode;
                path.Add(current);
            }

            return (dist[u, v], path, edges, operations);
        }
    }
}